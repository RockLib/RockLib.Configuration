using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
      private delegate object CreateProxyDelegate(IConfiguration configuration, DefaultTypes? defaultTypes, ValueConverters? valueConverters,
         Type? declaringType, string? memberName, IResolver? resolver);

      private const TypeAttributes ProxyClassAttributes = TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
      private const MethodAttributes ExplicitInterfaceMethodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
      private const MethodAttributes TransferStateAttributes = MethodAttributes.FamORAssem | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final;
      private const MethodAttributes ConstructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      private static readonly MethodInfo _delegateCombineMethod = typeof(Delegate).GetMethod(nameof(Delegate.Combine), new[] { typeof(Delegate), typeof(Delegate) })!;
      private static readonly MethodInfo _delegateRemoveMethod = typeof(Delegate).GetMethod(nameof(Delegate.Remove), new[] { typeof(Delegate), typeof(Delegate) })!;
      private static readonly CustomAttributeBuilder _debuggerBrowsableNeverAttribute = new CustomAttributeBuilder(
         typeof(DebuggerBrowsableAttribute).GetConstructor(new[] { typeof(DebuggerBrowsableState) })!, new object[] { DebuggerBrowsableState.Never });

      private static readonly ConcurrentDictionary<Type, CreateProxyDelegate> _proxyFactories = new ConcurrentDictionary<Type, CreateProxyDelegate>();

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
      public static TInterface CreateReloadingProxy<TInterface>(this IConfiguration configuration, DefaultTypes defaultTypes, ValueConverters valueConverters) =>
          configuration.CreateReloadingProxy<TInterface>(defaultTypes, valueConverters, null);

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
      /// <param name="resolver">
      /// An object that can retrieve constructor parameter values that are not found in configuration. This
      /// object is an adapter for dependency injection containers, such as Ninject, Unity, Autofac, or
      /// StructureMap. Consider using the <see cref="Resolver"/> class for this parameter, as it supports
      /// most depenedency injection containers.
      /// </param>
      /// <returns>
      /// An object of type <typeparamref name="TInterface"/> with values set from the configuration that reloads
      /// itself when the configuration changes.
      /// </returns>
      public static TInterface CreateReloadingProxy<TInterface>(this IConfiguration configuration,
         DefaultTypes? defaultTypes = null, ValueConverters? valueConverters = null, IResolver? resolver = null) =>
            (TInterface)configuration.CreateReloadingProxy(typeof(TInterface), defaultTypes, valueConverters, resolver);

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
      public static object CreateReloadingProxy(this IConfiguration configuration, Type interfaceType, DefaultTypes defaultTypes, ValueConverters valueConverters) =>
          configuration.CreateReloadingProxy(interfaceType, defaultTypes, valueConverters, null);

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
      /// <param name="resolver">
      /// An object that can retrieve constructor parameter values that are not found in configuration. This
      /// object is an adapter for dependency injection containers, such as Ninject, Unity, Autofac, or
      /// StructureMap. Consider using the <see cref="Resolver"/> class for this parameter, as it supports
      /// most depenedency injection containers.
      /// </param>
      /// <returns>
      /// An object of type <paramref name="interfaceType"/> with values set from the configuration that reloads
      /// itself when the configuration changes.
      /// </returns>
      public static object CreateReloadingProxy(this IConfiguration configuration, Type interfaceType,
         DefaultTypes? defaultTypes = null, ValueConverters? valueConverters = null, IResolver? resolver = null) =>
            configuration.CreateReloadingProxy(interfaceType, defaultTypes, valueConverters, null, null, resolver ?? Resolver.Empty);

      internal static object CreateReloadingProxy(this IConfiguration configuration, Type interfaceType,
         DefaultTypes? defaultTypes, ValueConverters? valueConverters, Type? declaringType, string? memberName, IResolver? resolver)
      {
         if (configuration is null)
            throw new ArgumentNullException(nameof(configuration));
         if (interfaceType is null)
            throw new ArgumentNullException(nameof(interfaceType));
         if (!interfaceType.IsInterface)
            throw new ArgumentException($"Specified type is not an interface: '{interfaceType.FullName}'.", nameof(interfaceType));
         if (!typeof(IDisposable).IsAssignableFrom(interfaceType))
            throw new ArgumentException($"The specified type, {interfaceType.FullName}, does not implement IDisposable.", nameof(interfaceType));
         if (interfaceType == typeof(IEnumerable))
            throw new ArgumentException("The IEnumerable interface is not supported.");
         if (typeof(IEnumerable).IsAssignableFrom(interfaceType))
            throw new ArgumentException($"Interfaces that inherit from IEnumerable are not suported: '{interfaceType.FullName}'", nameof(interfaceType));

         if (!string.IsNullOrEmpty(configuration[ConfigurationObjectFactory.TypeKey])
             && string.Equals(configuration[ConfigurationObjectFactory.ReloadOnChangeKey], "false", StringComparison.OrdinalIgnoreCase))
            return configuration.BuildTypeSpecifiedObject(interfaceType, declaringType, memberName, valueConverters ?? new ValueConverters(), defaultTypes ?? new DefaultTypes(), resolver);

         var createReloadingProxy = _proxyFactories.GetOrAdd(interfaceType, CreateProxyTypeFactoryMethod);
         return createReloadingProxy.Invoke(configuration, defaultTypes, valueConverters, declaringType, memberName, resolver);
      }

      private static CreateProxyDelegate CreateProxyTypeFactoryMethod(Type interfaceType)
      {
         var proxyType = CreateProxyType(interfaceType);
         return CompileFactoryMethod(proxyType);
      }

      private static TypeInfo CreateProxyType(Type interfaceType)
      {
         var baseClass = typeof(ConfigReloadingProxy<>).MakeGenericType(interfaceType);

         var baseConstructor = baseClass.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
         var baseGetObjectMethod = baseClass.GetProperty("Object")!.GetMethod!;
         var baseTransferStateMethod = baseClass.GetMethod("TransferState", BindingFlags.NonPublic | BindingFlags.Instance);

         var eventFields = new Dictionary<EventInfo, FieldBuilder>();
         var implementedMethods = new List<MethodInfo>();

         var proxyTypeBuilder = CreateProxyTypeBuilder(interfaceType, baseClass);

         AddConstructor(proxyTypeBuilder, baseConstructor);

         // As a side-effect, AddProperty adds getter and setter methods to the implementedMethods list.
         foreach (var property in interfaceType.GetAllProperties())
            AddProperty(proxyTypeBuilder, property, baseGetObjectMethod, implementedMethods);

         // As a side-effect, AddEvent adds:
         // - add and remove event handler methods to the implementedMethods list.
         // - the interface event + backing event handler field to the eventFields dictionary.
         foreach (var evt in interfaceType.GetAllEvents())
            AddEvent(proxyTypeBuilder, evt, baseGetObjectMethod, eventFields, implementedMethods);

         // Only add methods that weren't already created in AddProperty and AddEvent.
         foreach (var method in interfaceType.GetAllMethods().Where(method => !implementedMethods.Contains(method) && method.Name is not "Dispose"))
            AddMethod(proxyTypeBuilder, method, baseGetObjectMethod);

         // The eventFields dictionary needs to be fully populated in order to correctly
         // implement the TransferState method.
         AddTransferStateOverrideMethod(proxyTypeBuilder, interfaceType, eventFields, baseTransferStateMethod!);

         return proxyTypeBuilder.CreateTypeInfo()!;
      }

      private static TypeBuilder CreateProxyTypeBuilder(Type interfaceType, Type baseType)
      {
         var assemblyName = $"<{interfaceType.Name}>a__RockLibDynamicAssembly";
         var name = $"<{interfaceType.Name}>c__RockLibConfigReloadingProxyClass";
         var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
         var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
         return moduleBuilder.DefineType(name, ProxyClassAttributes, baseType, new[] { interfaceType });
      }

      private static void AddConstructor(TypeBuilder proxyTypeBuilder, ConstructorInfo baseConstructor)
      {
         var constructorBuilder = proxyTypeBuilder.DefineConstructor(ConstructorAttributes, baseConstructor.CallingConvention,
             new[] { typeof(IConfiguration), typeof(DefaultTypes), typeof(ValueConverters), typeof(Type), typeof(string), typeof(IResolver) });

         var il = constructorBuilder.GetILGenerator();

         il.Emit(OpCodes.Ldarg_0);
         il.Emit(OpCodes.Ldarg_1);
         il.Emit(OpCodes.Ldarg_2);
         il.Emit(OpCodes.Ldarg_3);
         il.Emit(OpCodes.Ldarg_S, 4);
         il.Emit(OpCodes.Ldarg_S, 5);
         il.Emit(OpCodes.Ldarg_S, 6);
         il.Emit(OpCodes.Call, baseConstructor);
         il.Emit(OpCodes.Ret);
      }

      private static void AddProperty(TypeBuilder proxyTypeBuilder, PropertyInfo interfaceProperty, MethodInfo baseGetObjectMethod, ICollection<MethodInfo> implementedMethods)
      {
         var parameters = interfaceProperty.GetIndexParameters().Select(p => p.ParameterType).ToArray();
         var propertyBuilder = proxyTypeBuilder.DefineProperty(interfaceProperty.Name, interfaceProperty.Attributes, interfaceProperty.PropertyType, parameters);

         if (interfaceProperty.CanRead)
         {
            var getMethodBuilder = proxyTypeBuilder.DefineMethod("get_" + interfaceProperty.Name, ExplicitInterfaceMethodAttributes, interfaceProperty.PropertyType, parameters);

            var il = getMethodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, baseGetObjectMethod);
            for (var i = 0; i < parameters.Length; i++)
               il.Emit(OpCodes.Ldarg, i + 1);
            il.Emit(OpCodes.Callvirt, interfaceProperty.GetMethod!);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetCustomAttribute(_debuggerBrowsableNeverAttribute);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            proxyTypeBuilder.DefineMethodOverride(getMethodBuilder, interfaceProperty.GetMethod!);
            implementedMethods.Add(interfaceProperty.GetMethod!);
         }

         if (interfaceProperty.CanWrite)
         {
            var setMethodBuilder = proxyTypeBuilder.DefineMethod("set_" + interfaceProperty.Name, ExplicitInterfaceMethodAttributes, typeof(void), parameters.Concat(new[] { interfaceProperty.PropertyType }).ToArray());
            var il = setMethodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, baseGetObjectMethod);
            il.Emit(OpCodes.Ldarg_1);
            for (int i = 0; i < parameters.Length; i++)
               il.Emit(OpCodes.Ldarg, i + 2);
            il.Emit(OpCodes.Callvirt, interfaceProperty.SetMethod!);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setMethodBuilder);
            proxyTypeBuilder.DefineMethodOverride(setMethodBuilder, interfaceProperty.SetMethod!);
            implementedMethods.Add(interfaceProperty.SetMethod!);
         }
      }

      private static void AddEvent(TypeBuilder proxyTypeBuilder, EventInfo interfaceEvent, MethodInfo baseGetObjectMethod, IDictionary<EventInfo, FieldBuilder> eventFields, ICollection<MethodInfo> implementedMethods)
      {
#if NET48
         var interfaceEventDeclaringTypeName = interfaceEvent.DeclaringType!.FullName!.Replace(".", "_");
#else
         var interfaceEventDeclaringTypeName = interfaceEvent.DeclaringType!.FullName!.Replace(".", "_", StringComparison.OrdinalIgnoreCase);
#endif
         var eventField = proxyTypeBuilder.DefineField($"_{interfaceEventDeclaringTypeName}_{interfaceEvent.Name}",
            interfaceEvent.EventHandlerType!, FieldAttributes.Private);
         eventField.SetCustomAttribute(_debuggerBrowsableNeverAttribute);
         eventFields.Add(interfaceEvent, eventField);

         var parameters = new[] { interfaceEvent.EventHandlerType! };

         var eventBuilder = proxyTypeBuilder.DefineEvent(interfaceEvent.Name, interfaceEvent.Attributes, interfaceEvent.EventHandlerType!);
         var addMethod = proxyTypeBuilder.DefineMethod(interfaceEvent.AddMethod!.Name, ExplicitInterfaceMethodAttributes, typeof(void), parameters);
         var removeMethod = proxyTypeBuilder.DefineMethod(interfaceEvent.RemoveMethod!.Name, ExplicitInterfaceMethodAttributes, typeof(void), parameters);

         var il = addMethod.GetILGenerator();

         // Add the event handler to Object's event
         il.Emit(OpCodes.Ldarg_0);
         il.Emit(OpCodes.Call, baseGetObjectMethod);
         il.Emit(OpCodes.Ldarg_1);
         il.Emit(OpCodes.Callvirt, interfaceEvent.AddMethod);

         // Add the event handler to the event's "tracking" field
         il.Emit(OpCodes.Ldarg_0);
         il.Emit(OpCodes.Ldarg_0);
         il.Emit(OpCodes.Ldfld, eventField);
         il.Emit(OpCodes.Ldarg_1);
         il.Emit(OpCodes.Call, _delegateCombineMethod);
         il.Emit(OpCodes.Castclass, interfaceEvent.EventHandlerType!);
         il.Emit(OpCodes.Stfld, eventField);
         il.Emit(OpCodes.Ret);

         eventBuilder.SetAddOnMethod(addMethod);
         proxyTypeBuilder.DefineMethodOverride(addMethod, interfaceEvent.AddMethod);
         implementedMethods.Add(interfaceEvent.AddMethod);

         il = removeMethod.GetILGenerator();

         // Remove the event handler from Object's event
         il.Emit(OpCodes.Ldarg_0);
         il.Emit(OpCodes.Call, baseGetObjectMethod);
         il.Emit(OpCodes.Ldarg_1);
         il.Emit(OpCodes.Callvirt, interfaceEvent.RemoveMethod);

         // Remove the event handler from the event's "tracking" field
         il.Emit(OpCodes.Ldarg_0);
         il.Emit(OpCodes.Ldarg_0);
         il.Emit(OpCodes.Ldfld, eventField);
         il.Emit(OpCodes.Ldarg_1);
         il.Emit(OpCodes.Call, _delegateRemoveMethod);
         il.Emit(OpCodes.Castclass, interfaceEvent.EventHandlerType!);
         il.Emit(OpCodes.Stfld, eventField);
         il.Emit(OpCodes.Ret);

         eventBuilder.SetRemoveOnMethod(removeMethod);
         proxyTypeBuilder.DefineMethodOverride(removeMethod, interfaceEvent.RemoveMethod);
         implementedMethods.Add(interfaceEvent.RemoveMethod);
      }

      private static void AddMethod(TypeBuilder proxyTypeBuilder, MethodInfo interfaceMethod, MethodInfo baseGetObjectMethod)
      {
         var parameters = interfaceMethod.GetParameters().Select(p => p.ParameterType).ToArray();
         var methodBuilder = proxyTypeBuilder.DefineMethod(interfaceMethod.Name, ExplicitInterfaceMethodAttributes, interfaceMethod.ReturnType, parameters);

         var il = methodBuilder.GetILGenerator();

         il.Emit(OpCodes.Ldarg_0);
         il.Emit(OpCodes.Call, baseGetObjectMethod);

         for (int i = 0; i < parameters.Length; i++)
            il.Emit(OpCodes.Ldarg, i + 1);

         il.Emit(OpCodes.Callvirt, interfaceMethod);
         il.Emit(OpCodes.Ret);

         proxyTypeBuilder.DefineMethodOverride(methodBuilder, interfaceMethod);
      }

      private static void AddTransferStateOverrideMethod(TypeBuilder proxyTypeBuilder, Type interfaceType,
         IReadOnlyDictionary<EventInfo, FieldBuilder> eventFields, MethodInfo baseTransferStateMethod)
      {
         var transferStateMethod = proxyTypeBuilder.DefineMethod("TransferState", TransferStateAttributes, typeof(void), new[] { interfaceType, interfaceType });

         var il = transferStateMethod.GetILGenerator();

         // Event handlers from the old object need to be copied to the new one.
         foreach (var evt in interfaceType.GetAllEvents())
         {
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, eventFields[evt]);
            il.Emit(OpCodes.Callvirt, evt.AddMethod!);
         }

         // Special case for when the interface has a read/write property: if the new
         // property value is null and the old property value is not null, then copy
         // the value from old to new.
         foreach (var property in interfaceType.GetAllProperties().Where(p => p.CanRead && p.CanWrite && !p.PropertyType.IsValueType))
         {
            var doNotCopyProperty = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, property.GetMethod!);
            il.Emit(OpCodes.Brfalse_S, doNotCopyProperty);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Callvirt, property.GetMethod!);
            il.Emit(OpCodes.Brtrue_S, doNotCopyProperty);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, property.GetMethod!);
            il.Emit(OpCodes.Callvirt, property.SetMethod!);
            il.MarkLabel(doNotCopyProperty);
         }

         il.Emit(OpCodes.Ret);

         proxyTypeBuilder.DefineMethodOverride(transferStateMethod, baseTransferStateMethod);
      }

      private static CreateProxyDelegate CompileFactoryMethod(TypeInfo proxyType)
      {
         var constructor = proxyType.GetConstructors()[0];

         var sectionParameter = Expression.Parameter(typeof(IConfiguration), "section");
         var defaultTypesParameter = Expression.Parameter(typeof(DefaultTypes), "defaultTypes");
         var valueConvertersParameter = Expression.Parameter(typeof(ValueConverters), "valueConverters");
         var declaringTypeParameter = Expression.Parameter(typeof(Type), "declaringType");
         var memberNameParameter = Expression.Parameter(typeof(string), "memberName");
         var resolverParameter = Expression.Parameter(typeof(IResolver), "resolver");

         var lambda = Expression.Lambda<CreateProxyDelegate>(
             Expression.New(constructor, sectionParameter, defaultTypesParameter, valueConvertersParameter, declaringTypeParameter, memberNameParameter, resolverParameter),
             sectionParameter, defaultTypesParameter, valueConvertersParameter, declaringTypeParameter, memberNameParameter, resolverParameter);

         return lambda.Compile();
      }

      private static IEnumerable<PropertyInfo> GetAllProperties(this Type type) =>
          type.GetProperties().Concat(type.GetInterfaces().SelectMany(i => i.GetProperties()));

      private static IEnumerable<MethodInfo> GetAllMethods(this Type type) =>
          type.GetMethods().Concat(type.GetInterfaces().SelectMany(i => i.GetMethods()));

      private static IEnumerable<EventInfo> GetAllEvents(this Type type) =>
          type.GetEvents().Concat(type.GetInterfaces().SelectMany(i => i.GetEvents()));
   }
}
