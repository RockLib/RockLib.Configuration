using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
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
    public static class ConfigReloadingProxyFactory
    {
        private const MethodAttributes ExplicitInterfaceImplementation = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;

        private static readonly MethodInfo _delegateCombineMethod = typeof(Delegate).GetTypeInfo().GetMethod(nameof(Delegate.Combine), new[] { typeof(Delegate), typeof(Delegate) });
        private static readonly MethodInfo _delegateRemoveMethod = typeof(Delegate).GetTypeInfo().GetMethod(nameof(Delegate.Remove), new[] { typeof(Delegate), typeof(Delegate) });
        private static readonly MethodInfo _disposeMethod = typeof(IDisposable).GetTypeInfo().GetMethod(nameof(IDisposable.Dispose));

        private static readonly MethodInfo _genericCreateMethod = typeof(ConfigurationObjectFactory).GetTypeInfo().GetMethod(nameof(ConfigurationObjectFactory.Create),
            new[] { typeof(IConfiguration), typeof(DefaultTypes), typeof(ValueConverters) });

        private static readonly FieldInfo _eventArgsEmptyField = typeof(EventArgs).GetTypeInfo().GetField(nameof(EventArgs.Empty), BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo _eventArgsInvokeMethod = typeof(EventHandler).GetTypeInfo().GetMethod(nameof(EventHandler.Invoke));

        private static readonly ConstructorInfo _objectConstructor = typeof(object).GetTypeInfo().GetConstructors()[0];

        private static readonly MethodInfo _getReloadTokenMethod = typeof(IConfiguration).GetTypeInfo().GetMethod(nameof(IConfiguration.GetReloadToken));

        private static readonly ConcurrentDictionary<Type, Func<IConfiguration, DefaultTypes, ValueConverters, object>> _proxyFactories = new ConcurrentDictionary<Type, Func<IConfiguration, DefaultTypes, ValueConverters, object>>();

        private static readonly ConstructorInfo _changeTokenFuncConstructor = typeof(Func<IChangeToken>).GetTypeInfo().GetConstructors()[0];

        private static readonly ConstructorInfo _actionConstructor = typeof(Action).GetTypeInfo().GetConstructors()[0];

        private static readonly MethodInfo _changeTokenOnChangeMethod = typeof(ChangeToken).GetTypeInfo().GetMethod(nameof(ChangeToken.OnChange), new[] { typeof(Func<IChangeToken>), typeof(Action) });

        public static TInterface CreateReloadingProxy<TInterface>(this IConfiguration configuration, DefaultTypes defaultTypes = null, ValueConverters valueConverters = null) =>
            (TInterface)configuration.CreateReloadingProxy(typeof(TInterface), defaultTypes, valueConverters);

        public static object CreateReloadingProxy(this IConfiguration configuration, Type interfaceType, DefaultTypes defaultTypes = null, ValueConverters valueConverters = null)
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
            return createProxy(configuration, defaultTypes, valueConverters);
        }

        private static Func<IConfiguration, DefaultTypes, ValueConverters, object> CreateProxyTypeFactoryMethod(Type type)
        {
            var assemblyName = "<" + type.Name + ">a__RockLibDynamicAssembly";
            var name = "<" + type.Name + ">c__RockLibConfigReloadingProxyClass";
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
                typeof(object), new[] { type, typeof(IDisposable), typeof(IConfigReloadingProxy<>).MakeGenericType(type) });

            var sectionField = typeBuilder.DefineField("_section", typeof(IConfiguration), FieldAttributes.Private | FieldAttributes.InitOnly);
            var defaultTypesField = typeBuilder.DefineField("_defaultTypes", typeof(DefaultTypes), FieldAttributes.Private | FieldAttributes.InitOnly);
            var valueConvertersField = typeBuilder.DefineField("_valueConverters", typeof(ValueConverters), FieldAttributes.Private | FieldAttributes.InitOnly);
            var objectField = typeBuilder.DefineField("_object", type, FieldAttributes.Private);

            var eventFields = new Dictionary<EventInfo, FieldBuilder>();

            var reloadingField = typeBuilder.DefineField("_reloading", typeof(EventHandler), FieldAttributes.Private);
            var reloadedField = typeBuilder.DefineField("_reloaded", typeof(EventHandler), FieldAttributes.Private);

            var reloadObjectMethod = GetReloadObjectMethod(typeBuilder, type, sectionField, objectField, reloadingField, reloadedField, defaultTypesField, valueConvertersField, eventFields);

            AddConstructor(typeBuilder, sectionField, objectField, defaultTypesField, valueConvertersField, reloadObjectMethod, type);

            foreach (var property in type.GetAllProperties())
                AddProperty(typeBuilder, property, objectField);

            foreach (var method in type.GetAllMethods().Where(m => ShouldAdd(m, type)))
                AddMethod(typeBuilder, method, objectField);

            foreach (var evt in type.GetAllEvents())
                AddEvent(typeBuilder, evt, objectField, eventFields);

            AddDisposeMethod(typeBuilder, objectField);

            AddConfigReloadingProxyMembers(typeBuilder, objectField, type, reloadingField, reloadedField);

            var proxyType = typeBuilder.CreateTypeInfo();

            var sectionParameter = Expression.Parameter(typeof(IConfiguration), "section");
            var defaultTypesParameter = Expression.Parameter(typeof(DefaultTypes), "defaultTypes");
            var valueConvertersParameter = Expression.Parameter(typeof(ValueConverters), "valueConverters");
            var lambda = Expression.Lambda<Func<IConfiguration, DefaultTypes, ValueConverters, object>>(
                Expression.New(proxyType.GetConstructors()[0], sectionParameter, defaultTypesParameter, valueConvertersParameter),
                sectionParameter, defaultTypesParameter, valueConvertersParameter);
            return lambda.Compile();
        }

        private static MethodBuilder GetReloadObjectMethod(TypeBuilder typeBuilder, Type type, FieldBuilder sectionField, FieldBuilder objectField,
            FieldBuilder reloadingField, FieldBuilder reloadedField, FieldBuilder defaultTypesField, FieldBuilder valueConvertersField,
            Dictionary<EventInfo, FieldBuilder> eventFields)
        {
            var reloadObjectMethod = typeBuilder.DefineMethod("ReloadObject", MethodAttributes.PrivateScope | MethodAttributes.Private | MethodAttributes.HideBySig);

            var il = reloadObjectMethod.GetILGenerator();

            var oldObjectVariable = il.DeclareLocal(type);
            var newObjectVariable = il.DeclareLocal(type);

            var exitLabel = il.DefineLabel();
            var reloadingLabel1 = il.DefineLabel();
            var reloadingLabel2 = il.DefineLabel();
            var reloadedLabel = il.DefineLabel();

            // Invoke the Reloading event
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, reloadingField);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Brtrue_S, reloadingLabel1);
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Br_S, reloadingLabel2);
            il.MarkLabel(reloadingLabel1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldsfld, _eventArgsEmptyField);
            il.Emit(OpCodes.Callvirt, _eventArgsInvokeMethod);
            il.MarkLabel(reloadingLabel2);

            // Capture the old object, instantiate the new one (but don't set the field)
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, objectField);
            il.Emit(OpCodes.Stloc, oldObjectVariable);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, sectionField);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, defaultTypesField);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, valueConvertersField);
            il.Emit(OpCodes.Call, _genericCreateMethod.MakeGenericMethod(type));
            il.Emit(OpCodes.Stloc, newObjectVariable);

            // For each read/write property, if the new value is null and the old value is not null, copy the value over
            foreach (var property in type.GetAllProperties().Where(p => p.CanRead && p.CanWrite && !p.PropertyType.GetTypeInfo().IsValueType))
            {
                var readWritePropertyLabel = il.DefineLabel();

                il.Emit(OpCodes.Ldloc, newObjectVariable);
                il.Emit(OpCodes.Callvirt, property.GetMethod);
                il.Emit(OpCodes.Brtrue_S, readWritePropertyLabel);
                il.Emit(OpCodes.Ldloc, oldObjectVariable);
                il.Emit(OpCodes.Callvirt, property.GetMethod);

                il.Emit(OpCodes.Brfalse_S, readWritePropertyLabel);
                il.Emit(OpCodes.Ldloc, newObjectVariable);
                il.Emit(OpCodes.Ldloc, oldObjectVariable);
                il.Emit(OpCodes.Callvirt, property.GetMethod);
                il.Emit(OpCodes.Callvirt, property.SetMethod);

                il.MarkLabel(readWritePropertyLabel);
            }

            // For each event, copy handlers to the new object
            foreach (var evt in type.GetAllEvents())
            {
                eventFields.Add(evt, typeBuilder.DefineField($"_{evt.DeclaringType.FullName.Replace(".", "_")}_{evt.Name}", evt.EventHandlerType, FieldAttributes.Private));

                il.Emit(OpCodes.Ldloc, newObjectVariable);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, eventFields[evt]);
                il.Emit(OpCodes.Callvirt, evt.AddMethod);
            }

            // Set the _object field, then if the old object is IDisposable, dispose it
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc, newObjectVariable);
            il.Emit(OpCodes.Stfld, objectField);
            il.Emit(OpCodes.Ldloc, oldObjectVariable);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Isinst, typeof(IDisposable));
            il.Emit(OpCodes.Brtrue_S, exitLabel);
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(exitLabel);
            il.Emit(OpCodes.Callvirt, _disposeMethod);

            // Invoke the Reloaded event
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, reloadedField);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Brtrue_S, reloadedLabel);
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(reloadedLabel);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldsfld, _eventArgsEmptyField);
            il.Emit(OpCodes.Callvirt, _eventArgsInvokeMethod);

            il.Emit(OpCodes.Ret);

            return reloadObjectMethod;
        }

        private static void AddConstructor(TypeBuilder typeBuilder, FieldBuilder sectionField, FieldBuilder objectField,
            FieldBuilder defaultTypesField, FieldBuilder valueConvertersField, MethodBuilder reloadObjectMethod, Type type)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(IConfiguration), typeof(DefaultTypes), typeof(ValueConverters) });

            var il = constructorBuilder.GetILGenerator();

            // Call base constructor
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, _objectConstructor);

            // Set the _section field to the parameter
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, sectionField);

            // Set the _defaultTypes field to the parameter
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Stfld, defaultTypesField);

            // Set the _valueConverters field to the parameter
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Stfld, valueConvertersField);

            // Set the _object field by calling the ConfigurationObjectFactory.Create<T> method
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Call, _genericCreateMethod.MakeGenericMethod(type));
            il.Emit(OpCodes.Stfld, objectField);

            // Register the section's change token to call the ReloadObject method
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldvirtftn, _getReloadTokenMethod);
            il.Emit(OpCodes.Newobj, _changeTokenFuncConstructor);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldftn, reloadObjectMethod);
            il.Emit(OpCodes.Newobj, _actionConstructor);
            il.Emit(OpCodes.Call, _changeTokenOnChangeMethod);
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);
        }

        private static void AddProperty(TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder objectField)
        {
            var parameters = property.GetIndexParameters().Select(p => p.ParameterType).ToArray();
            var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, parameters);

            if (property.CanRead)
            {
                var getMethodBuilder = typeBuilder.DefineMethod("get_" + property.Name, ExplicitInterfaceImplementation, property.PropertyType, parameters);

                var il = getMethodBuilder.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, objectField);
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
                il.Emit(OpCodes.Ldfld, objectField);
                il.Emit(OpCodes.Ldarg_1);
                for (int i = 0; i < parameters.Length; i++)
                    il.Emit(OpCodes.Ldarg, i + 2);
                il.Emit(OpCodes.Callvirt, property.SetMethod);
                il.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setMethodBuilder);
                typeBuilder.DefineMethodOverride(setMethodBuilder, property.SetMethod);
            }
        }

        private static void AddMethod(TypeBuilder typeBuilder, MethodInfo method, FieldBuilder objectField)
        {
            var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(method.Name, ExplicitInterfaceImplementation, method.ReturnType, parameters);

            var il = methodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, objectField);

            for (int i = 0; i < parameters.Length; i++)
                il.Emit(OpCodes.Ldarg, i + 1);

            il.Emit(OpCodes.Callvirt, method);
            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder, method);
        }

        private static void AddEvent(TypeBuilder typeBuilder, EventInfo evt, FieldBuilder objectField, Dictionary<EventInfo, FieldBuilder> eventFields)
        {
            var eventField = eventFields[evt];

            var parameters = new[] { evt.EventHandlerType };

            var eventBuilder = typeBuilder.DefineEvent(evt.Name, evt.Attributes, evt.EventHandlerType);
            var addMethod = typeBuilder.DefineMethod(evt.AddMethod.Name, ExplicitInterfaceImplementation, typeof(void), parameters);
            var removeMethod = typeBuilder.DefineMethod(evt.RemoveMethod.Name, ExplicitInterfaceImplementation, typeof(void), parameters);

            var il = addMethod.GetILGenerator();

            // Add the event handler to _object's event
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, objectField);
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
            il.Emit(OpCodes.Ldfld, objectField);
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

        private static void AddDisposeMethod(TypeBuilder typeBuilder, FieldBuilder objectField)
        {
            var methodBuilder = typeBuilder.DefineMethod("Dispose", ExplicitInterfaceImplementation);

            var il = methodBuilder.GetILGenerator();
            var label = il.DefineLabel();

            // If the value of the _object field is IDisposable, dispose it
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, objectField);
            il.Emit(OpCodes.Isinst, typeof(IDisposable));
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Brtrue_S, label);
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(label);
            il.Emit(OpCodes.Callvirt, _disposeMethod);
            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder, _disposeMethod);
        }

        private static void AddConfigReloadingProxyMembers(TypeBuilder typeBuilder, FieldBuilder objectField, Type type, FieldBuilder reloadingField, FieldBuilder reloadedField)
        {
            var proxyInterfaceType = typeof(IConfigReloadingProxy<>).MakeGenericType(type);

            // Add Object property
            var propertyBuilder = typeBuilder.DefineProperty("Object", PropertyAttributes.HasDefault, type, null);
            var methodBuilder = typeBuilder.DefineMethod("get_Object", ExplicitInterfaceImplementation, type, null);
            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, objectField);
            il.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(methodBuilder);
            typeBuilder.DefineMethodOverride(methodBuilder, proxyInterfaceType.GetTypeInfo().GetMethod("get_Object"));

            var eventParameters = new[] { typeof(EventHandler) };

            // Add Reloading event
            var eventBuilder = typeBuilder.DefineEvent("Reloading", EventAttributes.None, typeof(EventHandler));

            methodBuilder = typeBuilder.DefineMethod("add_Reloading", ExplicitInterfaceImplementation, typeof(void), eventParameters);
            il = methodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, reloadingField);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, _delegateCombineMethod);
            il.Emit(OpCodes.Castclass, typeof(EventHandler));
            il.Emit(OpCodes.Stfld, reloadingField);

            il.Emit(OpCodes.Ret);

            eventBuilder.SetAddOnMethod(methodBuilder);
            typeBuilder.DefineMethodOverride(methodBuilder, proxyInterfaceType.GetTypeInfo().GetMethod("add_Reloading"));

            methodBuilder = typeBuilder.DefineMethod("remove_Reloading", ExplicitInterfaceImplementation, typeof(void), eventParameters);
            il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, reloadingField);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, _delegateCombineMethod);
            il.Emit(OpCodes.Castclass, typeof(EventHandler));
            il.Emit(OpCodes.Stfld, reloadingField);
            il.Emit(OpCodes.Ret);
            eventBuilder.SetRemoveOnMethod(methodBuilder);
            typeBuilder.DefineMethodOverride(methodBuilder, proxyInterfaceType.GetTypeInfo().GetMethod("remove_Reloading"));

            // Add Reloaded event
            eventBuilder = typeBuilder.DefineEvent("Reloaded", EventAttributes.None, typeof(EventHandler));

            methodBuilder = typeBuilder.DefineMethod("add_Reloaded", ExplicitInterfaceImplementation, typeof(void), eventParameters);
            il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, reloadedField);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, _delegateCombineMethod);
            il.Emit(OpCodes.Castclass, typeof(EventHandler));
            il.Emit(OpCodes.Stfld, reloadedField);
            il.Emit(OpCodes.Ret);
            eventBuilder.SetAddOnMethod(methodBuilder);
            typeBuilder.DefineMethodOverride(methodBuilder, proxyInterfaceType.GetTypeInfo().GetMethod("add_Reloaded"));

            methodBuilder = typeBuilder.DefineMethod("remove_Reloaded", ExplicitInterfaceImplementation, typeof(void), eventParameters);
            il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, reloadedField);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, _delegateCombineMethod);
            il.Emit(OpCodes.Castclass, typeof(EventHandler));
            il.Emit(OpCodes.Stfld, reloadedField);
            il.Emit(OpCodes.Ret);
            eventBuilder.SetRemoveOnMethod(methodBuilder);
            typeBuilder.DefineMethodOverride(methodBuilder, proxyInterfaceType.GetTypeInfo().GetMethod("remove_Reloaded"));
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
