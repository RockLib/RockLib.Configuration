using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Static class that allows the creation of dynmaic proxy objects that reload their
    /// backing fields upon configuration change.
    /// </summary>
    public static class ConfigReloadingProxyFactory
    {
        private const MethodAttributes ExplicitInterfaceImplementation = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;

        private static readonly MethodInfo _disposeMethod = typeof(IDisposable).GetTypeInfo().GetMethod(nameof(IDisposable.Dispose));
        private static readonly MethodInfo _delegateCombineMethod = typeof(Delegate).GetTypeInfo().GetMethod(nameof(Delegate.Combine), new[] { typeof(Delegate), typeof(Delegate) });
        private static readonly MethodInfo _delegateRemoveMethod = typeof(Delegate).GetTypeInfo().GetMethod(nameof(Delegate.Remove), new[] { typeof(Delegate), typeof(Delegate) });

        private static readonly ConcurrentDictionary<Type, Func<IConfiguration, DefaultTypes, ValueConverters, Type, string, object>> _proxyFactories = new ConcurrentDictionary<Type, Func<IConfiguration, DefaultTypes, ValueConverters, Type, string, object>>();

        /// <summary>
        /// Create an object of type <typeparamref name="TInterface"/> based on the specified configuration. The returned
        /// object delegates its functionality to a backing field that is reloaded when the configuration changes.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to create.</typeparam>
        /// <param name="configuration">The configuration to create the object from.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <returns>
        /// An object of type <typeparamref name="TInterface"/> with values set from the configuration that reloads
        /// itself when the configuration changes.
        /// </returns>
        public static TInterface CreateReloadingProxy<TInterface>(this IConfiguration configuration, DefaultTypes defaultTypes = null, ValueConverters valueConverters = null) =>
            (TInterface)configuration.CreateReloadingProxy(typeof(TInterface), defaultTypes, valueConverters);

        /// <summary>
        /// Create an object of type <paramref name="interfaceType"/> based on the specified configuration. The returned
        /// object delegates its functionality to a backing field that is reloaded when the configuration changes.
        /// </summary>
        /// <param name="configuration">The configuration to create the object from.</param>
        /// <param name="interfaceType">The interface type to create.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <returns>
        /// An object of type <paramref name="interfaceType"/> with values set from the configuration that reloads
        /// itself when the configuration changes.
        /// </returns>
        public static object CreateReloadingProxy(this IConfiguration configuration, Type interfaceType, DefaultTypes defaultTypes = null, ValueConverters valueConverters = null) =>
            configuration.CreateReloadingProxy(interfaceType, defaultTypes ?? ConfigurationObjectFactory.EmptyDefaultTypes, valueConverters ?? ConfigurationObjectFactory.EmptyValueConverters, null, null);

        internal static object CreateReloadingProxy(this IConfiguration configuration, Type interfaceType, DefaultTypes defaultTypes, ValueConverters valueConverters, Type declaringType, string memberName)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));
            if (!interfaceType.GetTypeInfo().IsInterface)
                throw new ArgumentException("Must be an interface type.", nameof(interfaceType));
            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(interfaceType))
                throw new ArgumentException("Cannot implement IEnumerable.", nameof(interfaceType));

            var createProxy = _proxyFactories.GetOrAdd(interfaceType, CreateProxyTypeFactoryMethod);
            return createProxy(configuration, defaultTypes, valueConverters, declaringType, memberName);
        }

        private static Func<IConfiguration, DefaultTypes, ValueConverters, Type, string, object> CreateProxyTypeFactoryMethod(Type type)
        {
            var assemblyName = "<" + type.Name + ">a__RockLibDynamicAssembly";
            var name = "<" + type.Name + ">c__RockLibConfigReloadingProxyClass";
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            var configReloadingProxyType = typeof(ConfigReloadingProxy<>).MakeGenericType(type);
            var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
                configReloadingProxyType, new[] { type });

            var getObjectMethod = configReloadingProxyType.GetTypeInfo().GetProperty("Object").GetMethod;

            var eventFields = new Dictionary<EventInfo, FieldBuilder>();

            AddTransferStateMethod(typeBuilder, type, eventFields, configReloadingProxyType);

            AddConstructor(typeBuilder, type, configReloadingProxyType);

            foreach (var property in type.GetAllProperties())
                AddProperty(typeBuilder, property, getObjectMethod);

            foreach (var method in type.GetAllMethods().Where(m => ShouldAdd(m, type)))
                AddMethod(typeBuilder, method, getObjectMethod);

            foreach (var evt in type.GetAllEvents())
                AddEvent(typeBuilder, evt, getObjectMethod, eventFields);

            var proxyType = typeBuilder.CreateTypeInfo();

            var sectionParameter = Expression.Parameter(typeof(IConfiguration), "section");
            var defaultTypesParameter = Expression.Parameter(typeof(DefaultTypes), "defaultTypes");
            var valueConvertersParameter = Expression.Parameter(typeof(ValueConverters), "valueConverters");
            var declaringTypeParameter = Expression.Parameter(typeof(Type), "declaringType");
            var memberNameParameter = Expression.Parameter(typeof(string), "memberName");
            var lambda = Expression.Lambda<Func<IConfiguration, DefaultTypes, ValueConverters, Type, string, object>>(
                Expression.New(proxyType.GetConstructors()[0], sectionParameter, defaultTypesParameter, valueConvertersParameter, declaringTypeParameter, memberNameParameter),
                sectionParameter, defaultTypesParameter, valueConvertersParameter, declaringTypeParameter, memberNameParameter);
            return lambda.Compile();
        }

        private static void AddTransferStateMethod(TypeBuilder typeBuilder, Type type, Dictionary<EventInfo, FieldBuilder> eventFields, Type configReloadingProxyType)
        {
            var baseTransferStateMethod = configReloadingProxyType.GetTypeInfo().GetMethod("TransferState", BindingFlags.NonPublic | BindingFlags.Instance);

            var transferStateMethod = typeBuilder.DefineMethod("TransferState", MethodAttributes.FamORAssem | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final, typeof(void), new[] { type, type });

            var il = transferStateMethod.GetILGenerator();

            // Event handlers from the old object need to be copied to the new one.
            foreach (var evt in type.GetAllEvents())
            {
                eventFields.Add(evt, typeBuilder.DefineField($"_{evt.DeclaringType.FullName.Replace(".", "_")}_{evt.Name}", evt.EventHandlerType, FieldAttributes.Private));

                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, eventFields[evt]);
                il.Emit(OpCodes.Callvirt, evt.AddMethod);
            }

            // Special case for when the interface has a read/write property: if the new
            // property value is null and the old property value is not null, then copy
            // the value from old to new.
            foreach (var property in type.GetAllProperties().Where(p => p.CanRead && p.CanWrite && !p.PropertyType.GetTypeInfo().IsValueType))
            {
                var doNotCopyProperty = il.DefineLabel();

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Callvirt, property.GetMethod);
                il.Emit(OpCodes.Brfalse_S, doNotCopyProperty);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Callvirt, property.GetMethod);
                il.Emit(OpCodes.Brtrue_S, doNotCopyProperty);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Callvirt, property.GetMethod);
                il.Emit(OpCodes.Callvirt, property.SetMethod);
                il.MarkLabel(doNotCopyProperty);
            }

            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(transferStateMethod, baseTransferStateMethod);
        }

        private static void AddConstructor(TypeBuilder typeBuilder, Type type, Type configReloadingProxyType)
        {
            var baseConstructor = configReloadingProxyType.GetTypeInfo().GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];

            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(IConfiguration), typeof(DefaultTypes), typeof(ValueConverters), typeof(Type), typeof(string) });

            var il = constructorBuilder.GetILGenerator();

            // Call base constructor
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Ldarg_S, 4);
            il.Emit(OpCodes.Ldarg_S, 5);
            il.Emit(OpCodes.Call, baseConstructor);
            il.Emit(OpCodes.Ret);
        }

        private static void AddProperty(TypeBuilder typeBuilder, PropertyInfo property, MethodInfo getObject)
        {
            var parameters = property.GetIndexParameters().Select(p => p.ParameterType).ToArray();
            var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, parameters);

            if (property.CanRead)
            {
                var getMethodBuilder = typeBuilder.DefineMethod("get_" + property.Name, ExplicitInterfaceImplementation, property.PropertyType, parameters);

                var il = getMethodBuilder.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, getObject);
                for (int i = 0; i < parameters.Length; i++)
                    il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Callvirt, property.GetMethod);
                il.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getMethodBuilder);
                typeBuilder.DefineMethodOverride(getMethodBuilder, property.GetMethod);
            }

            if (property.CanWrite)
            {
                var setMethodBuilder = typeBuilder.DefineMethod("set_" + property.Name, ExplicitInterfaceImplementation, typeof(void), parameters.Concat(new[] { property.PropertyType }).ToArray());
                var il = setMethodBuilder.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, getObject);
                il.Emit(OpCodes.Ldarg_1);
                for (int i = 0; i < parameters.Length; i++)
                    il.Emit(OpCodes.Ldarg, i + 2);
                il.Emit(OpCodes.Callvirt, property.SetMethod);
                il.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setMethodBuilder);
                typeBuilder.DefineMethodOverride(setMethodBuilder, property.SetMethod);
            }
        }

        private static void AddMethod(TypeBuilder typeBuilder, MethodInfo method, MethodInfo getObject)
        {
            var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(method.Name, ExplicitInterfaceImplementation, method.ReturnType, parameters);

            var il = methodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, getObject);

            for (int i = 0; i < parameters.Length; i++)
                il.Emit(OpCodes.Ldarg, i + 1);

            il.Emit(OpCodes.Callvirt, method);
            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder, method);
        }

        private static void AddEvent(TypeBuilder typeBuilder, EventInfo evt, MethodInfo getObject, Dictionary<EventInfo, FieldBuilder> eventFields)
        {
            var eventField = eventFields[evt];

            var parameters = new[] { evt.EventHandlerType };

            var eventBuilder = typeBuilder.DefineEvent(evt.Name, evt.Attributes, evt.EventHandlerType);
            var addMethod = typeBuilder.DefineMethod(evt.AddMethod.Name, ExplicitInterfaceImplementation, typeof(void), parameters);
            var removeMethod = typeBuilder.DefineMethod(evt.RemoveMethod.Name, ExplicitInterfaceImplementation, typeof(void), parameters);

            var il = addMethod.GetILGenerator();

            // Add the event handler to _object's event
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, getObject);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, evt.AddMethod);

            // Add the event handler to the event's "tracking" field
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, eventField);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, _delegateCombineMethod);
            il.Emit(OpCodes.Castclass, evt.EventHandlerType);
            il.Emit(OpCodes.Stfld, eventField);
            il.Emit(OpCodes.Ret);

            eventBuilder.SetAddOnMethod(addMethod);
            typeBuilder.DefineMethodOverride(addMethod, evt.AddMethod);

            il = removeMethod.GetILGenerator();

            // Remove the event handler from _object's event
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, getObject);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, evt.RemoveMethod);

            // Remove the event handler from the event's "tracking" field
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, eventField);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, _delegateRemoveMethod);
            il.Emit(OpCodes.Castclass, evt.EventHandlerType);
            il.Emit(OpCodes.Stfld, eventField);
            il.Emit(OpCodes.Ret);

            eventBuilder.SetRemoveOnMethod(removeMethod);
            typeBuilder.DefineMethodOverride(removeMethod, evt.RemoveMethod);
        }

        private static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
        {
            return type.GetTypeInfo().GetProperties().Concat(type.GetTypeInfo().GetInterfaces().SelectMany(i => i.GetTypeInfo().GetProperties()));
        }

        private static IEnumerable<MethodInfo> GetAllMethods(this Type type)
        {
            return type.GetTypeInfo().GetMethods().Concat(type.GetTypeInfo().GetInterfaces().SelectMany(i => i.GetTypeInfo().GetMethods()));
        }

        private static IEnumerable<EventInfo> GetAllEvents(this Type type)
        {
            return type.GetTypeInfo().GetEvents().Concat(type.GetTypeInfo().GetInterfaces().SelectMany(i => i.GetTypeInfo().GetEvents()));
        }

        private static bool ShouldAdd(MethodInfo method, Type type)
        {
            if (method.Name.StartsWith("add_") || method.Name.StartsWith("remove_"))
            {
                var events = type.GetAllEvents();
                if (events.Any(e => e.AddMethod == method || e.RemoveMethod == method))
                    return false;
            }
            if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
            {
                var properties = type.GetAllProperties();
                if (properties.Any(p => p.GetMethod == method || p.SetMethod == method))
                    return false;
            }
            if (method == _disposeMethod)
                return false;
            return true;
        }
    }
}
