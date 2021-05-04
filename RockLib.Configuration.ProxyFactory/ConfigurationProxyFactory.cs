using Microsoft.Extensions.Configuration;
using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace RockLib.Configuration.ProxyFactory
{
    /// <summary>
    /// Static class that creates proxy instances of interfaces from configuration values.
    /// </summary>
    public static class ConfigurationProxyFactory
    {
        private const TypeAttributes _typeAttributes = TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout;
        private const MethodAttributes _methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;

        private static readonly ConcurrentDictionary<Type, Type> _proxyCache = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Returns an instance of a proxy type that implements the interface specified by the <typeparamref name="T"/>
        /// type parameter and has values specified by the <paramref name="configuration"/> parameter.
        /// </summary>
        /// <remarks>
        /// This method uses the <see cref="ConfigurationObjectFactory.Create(IConfiguration, Type, DefaultTypes, ValueConverters)"/>
        /// method to create instances of proxy types and can throw any of the exception that it throws.
        /// </remarks>
        /// <typeparam name="T">An interface with readable properties.</typeparam>
        /// <param name="configuration">The configuration to create the proxy object from.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <returns>
        /// An object that implements the interface specified by the <typeparamref name="T"/> type containing values from the
        /// <paramref name="configuration"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// If <typeparamref name="T"/> is not an interface or has any declared methods, events, or write-only properties.
        /// </exception>
        public static T CreateProxy<T>(this IConfiguration configuration, DefaultTypes defaultTypes = null, ValueConverters valueConverters = null) =>
            (T)configuration.CreateProxy(typeof(T), defaultTypes, valueConverters);

        /// <summary>
        /// Returns an instance of a proxy type that implements the interface specified by the <paramref name="type"/>
        /// parameter and has values specified by the <paramref name="configuration"/> parameter.
        /// <para></para>
        /// </summary>
        /// <remarks>
        /// This method uses the <see cref="ConfigurationObjectFactory.Create(IConfiguration, Type, DefaultTypes, ValueConverters)"/>
        /// method to create instances of proxy types and can throw any of the exception that it throws.
        /// </remarks>
        /// <param name="configuration">The configuration to create the proxy object from.</param>
        /// <param name="type">An interface with readable properties.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <returns>
        /// An object that implements the interface specified by the <paramref name="type"/> parameter containing values from the
        /// <paramref name="configuration"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> or <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="type"/> is not an interface or has any declared methods, events, or write-only properties.
        /// </exception>
        public static object CreateProxy(this IConfiguration configuration, Type type, DefaultTypes defaultTypes = null, ValueConverters valueConverters = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (type == null) throw new ArgumentNullException(nameof(type));
            var proxyType = _proxyCache.GetOrAdd(type, CreateProxyType);
            return configuration.Create(proxyType, defaultTypes, valueConverters);
        }

        private static Type CreateProxyType(Type type)
        {
            ValidateType(type);
            var typeBuilder = GetTypeBuilder(type);

            var readonlyFields = new List<(FieldBuilder FieldBuilder, string PropertyName)>();

            foreach (var property in type.GetTypeInfo().GetProperties())
            {
                var backingFieldName = "<" + property.Name + ">k__BackingField";

                if (property.CanWrite)
                {
                    var fieldBuilder = typeBuilder.DefineField(backingFieldName, property.PropertyType, FieldAttributes.Private);
                    var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);
                    var getMethodBuilder = GetGetMethodBuilder(property.Name, property.PropertyType, typeBuilder, fieldBuilder);
                    var setMethodBuilder = GetSetMethodBuilder(property.Name, property.PropertyType, typeBuilder, fieldBuilder);
                    propertyBuilder.SetGetMethod(getMethodBuilder);
                    propertyBuilder.SetSetMethod(setMethodBuilder);
                }
                else
                {
                    var fieldBuilder = typeBuilder.DefineField(backingFieldName, property.PropertyType, FieldAttributes.Private | FieldAttributes.InitOnly);
                    var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);
                    var getMethodBuilder = GetGetMethodBuilder(property.Name, property.PropertyType, typeBuilder, fieldBuilder);
                    propertyBuilder.SetGetMethod(getMethodBuilder);
                    readonlyFields.Add((fieldBuilder, property.Name));
                }
            }

            AddConstructor(typeBuilder, readonlyFields);

            return typeBuilder.CreateTypeInfo().AsType();
        }

        private static TypeBuilder GetTypeBuilder(Type type)
        {
            var assemblyName = "<" + type.Name + ">a__RockLibDynamicAssembly";
            var name = "<" + type.Name + ">c__RockLibProxyClass";
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var typeBuilder = moduleBuilder.DefineType(name, _typeAttributes, typeof(object), new[] { type });
            return typeBuilder;
        }

        private static void AddConstructor(TypeBuilder typeBuilder, List<(FieldBuilder FieldBuilder, string PropertyName)> readonlyFields)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public, CallingConventions.Standard, readonlyFields.Select(f => f.FieldBuilder.FieldType).ToArray());
            var il = constructorBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(object).GetTypeInfo().GetConstructors()[0]);

            for (int i = 0; i < readonlyFields.Count; i++)
            {
                constructorBuilder.DefineParameter(i + 1, ParameterAttributes.None, readonlyFields[i].PropertyName);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Stfld, readonlyFields[i].FieldBuilder);
            }

            il.Emit(OpCodes.Ret);
        }

        private static MethodBuilder GetGetMethodBuilder(string name, Type type, TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            var getMethodBuilder = typeBuilder.DefineMethod("get_" + name, _methodAttributes, type, Type.EmptyTypes);
            var il = getMethodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldBuilder);

            il.Emit(OpCodes.Ret);
            return getMethodBuilder;
        }

        private static MethodBuilder GetSetMethodBuilder(string name, Type type, TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            var setMethodBuilder = typeBuilder.DefineMethod("set_" + name, _methodAttributes, null, new[] { type });
            var il = setMethodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fieldBuilder);

            il.Emit(OpCodes.Ret);
            return setMethodBuilder;
        }

        private static void ValidateType(Type type)
        {
            if (!type.GetTypeInfo().IsInterface)
                throw Exceptions.CannotCreateProxyOfNonInterfaceType(type);

            foreach (var member in type.GetTypeInfo().GetMembers())
            {
                switch (member)
                {
                    case MethodInfo m when !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_") && !m.Name.StartsWith("add_") && !m.Name.StartsWith("remove_"):
                        throw Exceptions.TargetInterfaceCannotHaveAnyMethods(type, m);
                    case EventInfo e:
                        throw Exceptions.TargetInterfaceCannotHaveAnyEvents(type, e);
                    case PropertyInfo p when p.CanRead && p.GetGetMethod().GetParameters().Length > 0 || p.CanWrite && p.GetSetMethod().GetParameters().Length > 1:
                        throw Exceptions.TargetInterfaceCannotHaveAnyIndexerProperties(type, p);
                    case PropertyInfo p when !p.CanRead:
                        throw Exceptions.TargetInterfaceCannotHaveAnyWriteOnlyProperties(type, p);
                }
            }
        }
    }
}
