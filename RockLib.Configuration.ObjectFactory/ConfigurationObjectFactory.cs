using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Static class that allows the creation of objects based on configuration values.
    /// </summary>
    public static class ConfigurationObjectFactory
    {
        internal const string TypeKey = "type";
        internal const string ValueKey = "value";
        internal const string ReloadOnChangeKey = "reloadOnChange";

        internal static readonly DefaultTypes EmptyDefaultTypes = new DefaultTypes();
        internal static readonly ValueConverters EmptyValueConverters = new ValueConverters();

        /// <summary>
        /// Create an object of type <typeparamref name="T"/> based on the specified configuration.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="configuration">The configuration to create the object from.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <returns>An object of type <typeparamref name="T"/> with values set from the configuration.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// - If the specified convert func returns an object that is not assignable to the target type.
        /// - If a configuration value is not assignable to the target type.
        /// - If the type specified by configuration is not assignable to its target type.
        /// - If the target type is an array but the type specified by configuration does not represent a list.
        /// - If the target type is an array with a rank greater than one.
        /// - If the target type is a list type but the type specified by configuration does not represent a list.
        /// - If the target type is abstract.
        /// - If the target type is System.Object.
        /// - If the target type is an unsupported collection type.
        /// - If the target type is not a supported collection type but the configuration represents a list.
        /// - If multiple properties or constructor parameters that match a member are decorated with a [DefaultType] attribute and the attribute values don't all match.
        /// - If the target type has no public constructors.
        /// - If the target has ambiguous constructors.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If a property or constructor parameter is decorated with a <see cref="DefaultTypeAttribute"/>
        /// that has a value that is not assignable to the property or constructor parameter.
        /// </exception>
        public static T Create<T>(this IConfiguration configuration, DefaultTypes defaultTypes, ValueConverters valueConverters) =>
            configuration.Create<T>(defaultTypes, valueConverters, null);

        /// <summary>
        /// Create an object of type <typeparamref name="T"/> based on the specified configuration.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="configuration">The configuration to create the object from.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <returns>An object of type <typeparamref name="T"/> with values set from the configuration.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// - If the specified convert func returns an object that is not assignable to the target type.
        /// - If a configuration value is not assignable to the target type.
        /// - If the type specified by configuration is not assignable to its target type.
        /// - If the target type is an array but the type specified by configuration does not represent a list.
        /// - If the target type is an array with a rank greater than one.
        /// - If the target type is a list type but the type specified by configuration does not represent a list.
        /// - If the target type is abstract.
        /// - If the target type is System.Object.
        /// - If the target type is an unsupported collection type.
        /// - If the target type is not a supported collection type but the configuration represents a list.
        /// - If multiple properties or constructor parameters that match a member are decorated with a [DefaultType] attribute and the attribute values don't all match.
        /// - If the target type has no public constructors.
        /// - If the target has ambiguous constructors.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If a property or constructor parameter is decorated with a <see cref="DefaultTypeAttribute"/>
        /// that has a value that is not assignable to the property or constructor parameter.
        /// </exception>
        public static T Create<T>(this IConfiguration configuration, DefaultTypes defaultTypes = null, ValueConverters valueConverters = null, IResolver resolver = null) =>
            (T)configuration.Create(typeof(T), defaultTypes, valueConverters, resolver);

        /// <summary>
        /// Create an object of the specified type based on the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration to create the object from.</param>
        /// <param name="type">The type of object to create.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <returns>An object with values set from the configuration.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> or <paramref name="type"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// - If the specified convert func returns an object that is not assignable to the target type.
        /// - If a configuration value is not assignable to the target type.
        /// - If the type specified by configuration is not assignable to its target type.
        /// - If the target type is an array but the type specified by configuration does not represent a list.
        /// - If the target type is an array with a rank greater than one.
        /// - If the target type is a list type but the type specified by configuration does not represent a list.
        /// - If the target type is abstract.
        /// - If the target type is System.Object.
        /// - If the target type is an unsupported collection type.
        /// - If the target type is not a supported collection type but the configuration represents a list.
        /// - If multiple properties or constructor parameters that match a member are decorated with a [DefaultType] attribute and the attribute values don't all match.
        /// - If the target type has no public constructors.
        /// - If the target has ambiguous constructors.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If a property or constructor parameter is decorated with a <see cref="DefaultTypeAttribute"/>
        /// that has a value that is not assignable to the property or constructor parameter.
        /// </exception>
        public static object Create(this IConfiguration configuration, Type type, DefaultTypes defaultTypes, ValueConverters valueConverters) =>
            configuration.Create(type, defaultTypes, valueConverters, null);

        /// <summary>
        /// Create an object of the specified type based on the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration to create the object from.</param>
        /// <param name="type">The type of object to create.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <returns>An object with values set from the configuration.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> or <paramref name="type"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// - If the specified convert func returns an object that is not assignable to the target type.
        /// - If a configuration value is not assignable to the target type.
        /// - If the type specified by configuration is not assignable to its target type.
        /// - If the target type is an array but the type specified by configuration does not represent a list.
        /// - If the target type is an array with a rank greater than one.
        /// - If the target type is a list type but the type specified by configuration does not represent a list.
        /// - If the target type is abstract.
        /// - If the target type is System.Object.
        /// - If the target type is an unsupported collection type.
        /// - If the target type is not a supported collection type but the configuration represents a list.
        /// - If multiple properties or constructor parameters that match a member are decorated with a [DefaultType] attribute and the attribute values don't all match.
        /// - If the target type has no public constructors.
        /// - If the target has ambiguous constructors.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If a property or constructor parameter is decorated with a <see cref="DefaultTypeAttribute"/>
        /// that has a value that is not assignable to the property or constructor parameter.
        /// </exception>
        public static object Create(this IConfiguration configuration, Type type, DefaultTypes defaultTypes = null, ValueConverters valueConverters = null, IResolver resolver = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (type == null) throw new ArgumentNullException(nameof(type));
            return configuration.Create(type, null, null, valueConverters ?? EmptyValueConverters, defaultTypes ?? EmptyDefaultTypes, resolver ?? Resolver.Empty);
        }

        private static object Create(this IConfiguration configuration, Type targetType, Type declaringType, string memberName, ValueConverters valueConverters, DefaultTypes defaultTypes, IResolver resolver)
        {
            if (targetType.IsArray)
                return BuildArray(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes, resolver);
            if (IsGenericList(targetType))
                return BuildGenericList(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes, resolver);
            if (IsNonGenericList(targetType, true))
                return BuildNonGenericList(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes, resolver);
            if (IsValueSection(configuration, out IConfigurationSection valueSection))
                return ConvertToType(valueSection, targetType, declaringType, memberName, valueConverters, defaultTypes, resolver);
            if (IsReloadingObject(configuration))
                return configuration.CreateReloadingProxy(targetType, defaultTypes, valueConverters, declaringType, memberName, resolver);
            if (IsTypeSpecifiedObject(configuration))
                return BuildTypeSpecifiedObject(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes, resolver);
            if (IsStringDictionary(ref targetType, declaringType, memberName, defaultTypes))
                return BuildStringDictionary(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes, resolver);
            return BuildObject(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes, resolver);
        }

        private static bool IsValueSection(IConfiguration configuration)
        {
            var valueSection = configuration as IConfigurationSection;
            return valueSection?.Value != null;
        }

        private static bool IsValueSection(IConfiguration configuration, out IConfigurationSection valueSection)
        {
            valueSection = configuration as IConfigurationSection;
            if (valueSection != null)
                if (valueSection.Value == null) valueSection = null;
                else return true;
            return false;
        }

        private static object ConvertToType(
            IConfigurationSection valueSection, Type targetType, Type declaringType, string memberName, ValueConverters valueConverters, DefaultTypes defaultTypes, IResolver resolver)
        {
            var convert = GetConvertFunc(targetType, declaringType, memberName, valueConverters);
            try
            {
                if (convert != null)
                    return convert(valueSection.Value) ?? throw Exceptions.ResultCannotBeNull(targetType, declaringType, memberName);
                if (targetType.GetTypeInfo().IsAssignableFrom(typeof(string)))
                    return valueSection.Value;
                if (targetType == typeof(Encoding))
                    return Encoding.GetEncoding(valueSection.Value);
                if (targetType == typeof(Type))
                    return Type.GetType(valueSection.Value, true, true);
                var typeConverter = TypeDescriptor.GetConverter(targetType);
                if (typeConverter.CanConvertFrom(typeof(string)))
                {
                    var value = valueSection.Value;
                    if (targetType.GetTypeInfo().IsEnum)
                    {
                        // Replace flags delimiters: c#'s "|" and vb's " Or ".
                        value = Regex.Replace(value, @"\s*\|\s*|\s+[Oo][Rr]\s+", ", ");
                    }
                    return typeConverter.ConvertFromInvariantString(value);
                }
                if (valueSection.Value == "")
                    return new ObjectBuilder(targetType, valueSection, resolver).Build(valueConverters, defaultTypes);
            }
            catch (Exception ex)
            {
                // TODO: Add tests that verify this sad path.
                throw Exceptions.CannotConvertSectionValueToTargetType(valueSection, targetType, ex);
            }
            throw Exceptions.CannotConvertSectionValueToTargetType(valueSection, targetType);
        }

        private static Func<string, object> GetConvertFunc(Type targetType, Type declaringType, string memberName, ValueConverters valueConverters)
        {
            if (!valueConverters.TryGet(declaringType, memberName, out Func<string, object> convert)
                && !valueConverters.TryGet(targetType, out convert))
            {
                Type returnType;
                var convertMethodName = GetConverterMethodNameFromMemberCustomAttributes(declaringType, memberName, out Type declaringTypeOfDecoratedMember);
                if (convertMethodName != null)
                    CreateConvertFunc(declaringTypeOfDecoratedMember ?? declaringType, convertMethodName, out returnType, out convert);
                else
                {
                    convertMethodName = GetConverterMethodNameFromCustomAttributes(targetType.GetTypeInfo().CustomAttributes);
                    if (convertMethodName != null)
                        CreateConvertFunc(targetType, convertMethodName, out returnType, out convert);
                    else
                    {
                        convert = null;
                        returnType = null;
                    }
                }
                if (returnType != null)
                {
                    if (!targetType.GetTypeInfo().IsAssignableFrom(returnType))
                        throw Exceptions.ReturnTypeOfMethodFromAttributeIsNotAssignableToTargetType(targetType, returnType, convertMethodName);
                }
            }

            return convert;
        }

        private static void CreateConvertFunc(Type declaringType, string methodName, out Type returnType, out Func<string, object> convertFunc)
        {
            var convertMethod = declaringType.GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .SingleOrDefault(m =>
                    m.Name == methodName
                    && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string)
                    && m.ReturnType != typeof(void) && m.ReturnType != typeof(object));
            if (convertMethod == null)
                throw Exceptions.NoMethodFound(declaringType, methodName);
            returnType = convertMethod.ReturnType;
            convertFunc = value => convertMethod.Invoke(null, new object[] { value });
        }

        private static bool IsReloadingObject(IConfiguration configuration)
        {
            var reloadOnChangeKeyFound = false;
            var i = 0;
            foreach (var child in configuration.GetChildren())
            {
                if (child.Key.Equals(ReloadOnChangeKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (child.Value?.ToLowerInvariant() != "true") return false;
                    reloadOnChangeKeyFound = true;
                }
                else if (!child.Key.Equals(ValueKey, StringComparison.OrdinalIgnoreCase)
                    && !child.Key.Equals(TypeKey, StringComparison.OrdinalIgnoreCase)) return false;
                i++;
            }
            return reloadOnChangeKeyFound && i >= 1 && i <= 3;
        }

        private static bool IsTypeSpecifiedObject(IConfiguration configuration)
        {
            var typeFound = false;
            var i = 0;
            foreach (var child in configuration.GetChildren())
            {
                if (child.Key.Equals(TypeKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(child.Value)) return false;
                    typeFound = true;
                }
                else if (child.Key.Equals(ReloadOnChangeKey, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(child.Value, "false", StringComparison.OrdinalIgnoreCase)) return false;
                else if (!child.Key.Equals(ValueKey, StringComparison.OrdinalIgnoreCase)) return false;
                i++;
            }
            return typeFound && i >= 1 && i <= 3;
        }

        internal static object BuildTypeSpecifiedObject(this IConfiguration configuration, Type targetType, Type declaringType, string memberName, ValueConverters valueConverters, DefaultTypes defaultTypes, IResolver resolver)
        {
            var typeSection = configuration.GetSection(TypeKey);
            var specifiedType = Type.GetType(typeSection.Value, throwOnError: true);
            if (!targetType.GetTypeInfo().IsAssignableFrom(specifiedType))
                throw Exceptions.ConfigurationSpecifiedTypeIsNotAssignableToTargetType(targetType, specifiedType);
            return BuildObject(configuration.GetSection(ValueKey), specifiedType, declaringType, memberName, valueConverters, defaultTypes, resolver, true);
        }

        private static object BuildArray(IConfiguration configuration, Type targetType, Type declaringType, string memberName, ValueConverters valueConverters, DefaultTypes defaultTypes, IResolver resolver)
        {
            if (targetType.GetArrayRank() > 1) throw Exceptions.ArrayRankGreaterThanOneIsNotSupported(targetType);

            var elementType = targetType.GetElementType();
            var isValueSection = IsValueSection(configuration);

            Array array;
            if (isValueSection && elementType == typeof(byte))
            {
                var item = (string)configuration.Create(typeof(string), declaringType, memberName, valueConverters, defaultTypes, resolver);
                array = Convert.FromBase64String(item);
            }
            else if (isValueSection || !IsList(configuration))
            {
                var item = configuration.Create(elementType, declaringType, memberName, valueConverters, defaultTypes, resolver);
                array = Array.CreateInstance(elementType, 1);
                array.SetValue(item, 0);
            }
            else
            {
                var items = configuration.GetChildren().Select(child =>
                    child.Create(elementType, declaringType, memberName, valueConverters, defaultTypes, resolver)).ToList();
                array = Array.CreateInstance(elementType, items.Count);
                for (int i = 0; i < array.Length; i++)
                    array.SetValue(items[i], i);
            }
            return array;
        }

        private static bool IsGenericList(Type type) =>
            type.GetTypeInfo().IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(List<>)
                    || type.GetGenericTypeDefinition() == typeof(IList<>)
                    || type.GetGenericTypeDefinition() == typeof(ICollection<>)
                    || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    || type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>)
                    || type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));

        private static object BuildGenericList(IConfiguration configuration, Type targetType, Type declaringType, string memberName, ValueConverters valueConverters, DefaultTypes defaultTypes, IResolver resolver)
        {
            var tType = targetType.GetTypeInfo().GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(tType);
            var addMethod = GetListAddMethod(tType);
            var list = Activator.CreateInstance(listType);
            var isValueSection = IsValueSection(configuration);

            if (isValueSection && tType == typeof(byte))
            {
                var base64String = (string)configuration.Create(typeof(string), declaringType, memberName, valueConverters, defaultTypes, resolver);
                var byteArray = Convert.FromBase64String(base64String);
                foreach (var item in byteArray)
                    addMethod.Invoke(list, new object[] { item });
            }
            else if (isValueSection || !IsList(configuration))
                addMethod.Invoke(list, new[] { configuration.Create(tType, declaringType, memberName, valueConverters, defaultTypes, resolver) });
            else
                foreach (var item in configuration.GetChildren().Select(child => child.Create(tType, declaringType, memberName, valueConverters, defaultTypes, resolver)))
                    addMethod.Invoke(list, new[] { item });
            return list;
        }

        internal static bool IsNonGenericList(this Type type, bool defaultConstructorRequired = false)
        {
            if (typeof(IList).GetTypeInfo().IsAssignableFrom(type))
            {
                return
                    (!defaultConstructorRequired
                        || type.GetTypeInfo().GetConstructor(Type.EmptyTypes) != null)
                    && GetNonGenericListItemType(type) != null;
            }
            return false;
        }

        private static Type GetNonGenericListItemType(Type nonGenericListType)
        {
            var itemTypes =
                nonGenericListType.GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == "Add"
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType != typeof(object))
                    .Select(m => m.GetParameters()[0].ParameterType)
                    .Distinct()
                    .ToList();
            if (itemTypes.Count == 1)
                return itemTypes[0];
            return null;
        }

        private static object BuildNonGenericList(IConfiguration configuration, Type targetType, Type declaringType, string memberName, ValueConverters valueConverters, DefaultTypes defaultTypes, IResolver resolver)
        {
            var itemType = GetNonGenericListItemType(targetType);
            var addMethod = GetListAddMethod(null);
            var list = Activator.CreateInstance(targetType);
            var isValueSection = IsValueSection(configuration);

            if (isValueSection || !IsList(configuration))
                addMethod.Invoke(list, new[] { configuration.Create(itemType, declaringType, memberName, valueConverters, defaultTypes, resolver) });
            else
                foreach (var item in configuration.GetChildren().Select(child => child.Create(itemType, declaringType, memberName, valueConverters, defaultTypes, resolver)))
                    addMethod.Invoke(list, new[] { item });
            return list;
        }

        private static bool IsList(IConfiguration configuration, bool includeEmptyList = true)
        {
            int i = 0;
            foreach (var child in configuration.GetChildren())
                if (child.Key != i++.ToString())
                    return false;
            return includeEmptyList || i > 0;
        }

        private static bool IsStringDictionary(ref Type targetType, Type declaringType, string memberName, DefaultTypes defaultTypes)
        {
            var type = targetType;

            if (type == typeof(object) && TryGetDefaultType(defaultTypes, type, declaringType, memberName, out Type defaultType))
                type = defaultType;

            if (type.GetTypeInfo().IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(IDictionary<,>) || type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
                && type.GetTypeInfo().GetGenericArguments()[0] == typeof(string))
            {
                if (type != targetType)
                    targetType = type;

                return true;
            }

            return false;
        }

        private static object BuildStringDictionary(IConfiguration configuration, Type targetType, Type declaringType, string memberName, ValueConverters valueConverters, DefaultTypes defaultTypes, IResolver resolver)
        {
            var tValueType = targetType.GetTypeInfo().GetGenericArguments()[1];
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), tValueType);
            var addMethod = dictionaryType.GetTypeInfo().GetMethod("Add", new[] { typeof(string), tValueType });
            var dictionary = Activator.CreateInstance(dictionaryType);
            foreach (var x in configuration.GetChildren().Select(c => new { c.Key, Value = c.Create(tValueType, declaringType, memberName, valueConverters, defaultTypes, resolver) }))
                addMethod.Invoke(dictionary, new object[] { x.Key, x.Value });
            return dictionary;
        }

        private static object BuildObject(IConfiguration configuration, Type targetType, Type declaringType, string memberName, ValueConverters valueConverters, DefaultTypes defaultTypes, IResolver resolver, bool skipDefaultTypes = false)
        {
            if (!skipDefaultTypes && TryGetDefaultType(defaultTypes, targetType, declaringType, memberName, out Type defaultType))
                targetType = defaultType;
            if (IsSimpleType(targetType, declaringType, memberName, valueConverters))
                throw Exceptions.TargetTypeRequiresConfigurationValue(configuration, targetType, declaringType, memberName);
            if (targetType.GetTypeInfo().IsAbstract)
                throw Exceptions.CannotCreateAbstractType(configuration, targetType);
            if (targetType == typeof(object))
                throw Exceptions.CannotCreateObjectType;
            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(targetType))
                throw Exceptions.UnsupportedCollectionType(targetType);
            if (IsList(configuration, includeEmptyList: false))
                throw Exceptions.ConfigurationIsAList(configuration, targetType);
            return new ObjectBuilder(targetType, configuration, resolver).Build(valueConverters, defaultTypes);
        }

        private static bool IsSimpleType(Type targetType, Type declaringType, string memberName, ValueConverters valueConverters)
        {
            var type = Nullable.GetUnderlyingType(targetType) ?? targetType;
            return type.GetTypeInfo().IsPrimitive
                || type.GetTypeInfo().IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(Guid)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan)
                || type == typeof(Uri)
                || type == typeof(Encoding)
                || type == typeof(Type)
                || GetConvertFunc(targetType, declaringType, memberName, valueConverters) != null;
        }

        internal static bool TryGetDefaultType(DefaultTypes defaultTypes, Type targetType, Type declaringType, string memberName, out Type defaultType)
        {
            if (defaultTypes.TryGet(declaringType, memberName, out defaultType)) return true;
            if (defaultTypes.TryGet(targetType, out defaultType)) return true;

            defaultType =
                GetDefaultTypeFromMemberCustomAttributes(declaringType, memberName)
                ?? GetDefaultTypeFromCustomAttributes(targetType.GetTypeInfo().CustomAttributes);

            if (defaultType != null)
            {
                if (!targetType.GetTypeInfo().IsAssignableFrom(defaultType))
                    throw Exceptions.DefaultTypeIsNotAssignableToTargetType(targetType, defaultType);
                if (defaultType.GetTypeInfo().IsAbstract)
                    throw Exceptions.DefaultTypeFromAttributeCannotBeAbstract(defaultType);
                return true;
            }

            return false;
        }

        /// <summary>Gets the default type, or null if not found.</summary>
        private static Type GetDefaultTypeFromMemberCustomAttributes(Type declaringType, string memberName) =>
            GetFirstParameterValueFromMemberCustomAttributes<Type>(declaringType, memberName, nameof(DefaultTypeAttribute), out var dummy);

        /// <summary>Gets the converter, or null if not found.</summary>
        private static string GetConverterMethodNameFromMemberCustomAttributes(Type declaringType, string memberName, out Type declaringTypeOfDecoratedMember) =>
            GetFirstParameterValueFromMemberCustomAttributes<string>(declaringType, memberName, nameof(ConvertMethodAttribute), out declaringTypeOfDecoratedMember);

        /// <summary>Gets the value of the first parameter, or null if not found.</summary>
        private static T GetFirstParameterValueFromMemberCustomAttributes<T>(
            Type declaringType, string memberName, string attributeTypeName, out Type declaringTypeOfDecoratedMember)
            where T : class
        {
            var parameterValueSet = new HashSet<T>();
            var declaringTypeSet = new HashSet<Type>();

            foreach (var member in Members.Find(declaringType, memberName))
            {
                if (member.MemberType == MemberType.Property)
                {
                    var property = declaringType.GetTypeInfo().GetProperty(member.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    var value = GetFirstParameterValueFromCustomAttributes<T>(property.CustomAttributes, attributeTypeName);
                    if (value != null)
                    {
                        parameterValueSet.Add(value);
                        declaringTypeSet.Add(property.DeclaringType);
                    }
                }
                else
                {
                    foreach (var parameter in declaringType.GetTypeInfo().GetConstructors()
                        .SelectMany(c => c.GetParameters())
                        .Where(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, member.Name)))
                    {
                        var value = GetFirstParameterValueFromCustomAttributes<T>(parameter.CustomAttributes, attributeTypeName);
                        if (value != null)
                        {
                            parameterValueSet.Add(value);
                            declaringTypeSet.Add(parameter.Member.DeclaringType);
                        }
                    }
                }
            }

            switch (declaringTypeSet.Count)
            {
                case 1:
                    declaringTypeOfDecoratedMember = declaringTypeSet.First();
                    break;
                default:
                    declaringTypeOfDecoratedMember = null;
                    break;
            }

            switch (parameterValueSet.Count)
            {
                case 0: return null;
                case 1: return parameterValueSet.First();
                default: throw Exceptions.InconsistentDefaultTypeAttributesForMultipleMembers(memberName);
            }
        }

        /// <summary>Gets the default type, or null if not found.</summary>
        private static Type GetDefaultTypeFromCustomAttributes(IEnumerable<CustomAttributeData> customAttributes) =>
            GetFirstParameterValueFromCustomAttributes<Type>(customAttributes, nameof(DefaultTypeAttribute));

        /// <summary>Gets the converter, or null if not found.</summary>
        private static string GetConverterMethodNameFromCustomAttributes(IEnumerable<CustomAttributeData> customAttributes) =>
            GetFirstParameterValueFromCustomAttributes<string>(customAttributes, nameof(ConvertMethodAttribute));

        /// <summary>Gets the value of the first parameter, or null if not found.</summary>
        private static T GetFirstParameterValueFromCustomAttributes<T>(
            IEnumerable<CustomAttributeData> customAttributes, string attributeTypeName) =>
            customAttributes.Where(attribute =>
                attribute.AttributeType.Name == attributeTypeName
                    && attribute.ConstructorArguments.Count == 1
                    && attribute.ConstructorArguments[0].ArgumentType == typeof(T)
                    && attribute.ConstructorArguments[0].Value != null)
                .Select(attribute => (T)attribute.ConstructorArguments[0].Value)
                .FirstOrDefault();

        private static MethodInfo GetListAddMethod(Type tType) =>
            (tType != null ? typeof(ICollection<>).MakeGenericType(tType) : typeof(IList))
                .GetTypeInfo().GetMethod("Add");

        private static MethodInfo GetListClearMethod(Type tType) =>
            (tType != null ? typeof(ICollection<>).MakeGenericType(tType) : typeof(IList))
                .GetTypeInfo().GetMethod("Clear");

        private static MethodInfo GetDictionaryAddMethod(Type tValueType) =>
            typeof(ICollection<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), tValueType))
                .GetTypeInfo().GetMethod("Add");

        private static MethodInfo GetDictionaryClearMethod(Type tValueType) =>
            typeof(ICollection<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), tValueType))
                .GetTypeInfo().GetMethod("Clear");

        private class ObjectBuilder
        {
            private readonly Dictionary<string, IConfigurationSection> _members = new Dictionary<string, IConfigurationSection>(new IdentifierComparer());

            public ObjectBuilder(Type type, IConfiguration configuration, IResolver resolver)
            {
                Type = type;
                Configuration = configuration;
                Resolver = resolver;
                foreach (var childSection in configuration.GetChildren())
                    _members.Add(childSection.Key, childSection);
            }

            public Type Type { get; }
            public IConfiguration Configuration { get; }
            public IResolver Resolver { get; }

            public object Build(ValueConverters valueConverters, DefaultTypes defaultTypes)
            {
                var constructor = GetConstructor();
                var args = GetArgs(constructor, valueConverters, defaultTypes);
                var obj = constructor.Invoke(args);
                foreach (var property in WritableProperties)
                    SetWritableProperty(obj, property, defaultTypes, valueConverters);
                foreach (var property in ReadonlyListProperties)
                    SetReadonlyListProperty(obj, property, defaultTypes, valueConverters);
                foreach (var property in ReadonlyDictionaryProperties)
                    SetReadonlyDictionaryProperty(obj, property, defaultTypes, valueConverters);
                return obj;
            }

            private void SetWritableProperty(object obj, PropertyInfo property, DefaultTypes defaultTypes, ValueConverters valueConverters)
            {
                if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                {
                    var propertyValue = section.Create(property.PropertyType, Type, property.Name, valueConverters, defaultTypes, Resolver);
                    property.SetValue(obj, propertyValue);
                }
            }

            private void SetReadonlyListProperty(object obj, PropertyInfo property, DefaultTypes defaultTypes, ValueConverters valueConverters)
            {
                if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                {
                    var list = property.GetValue(obj);
                    if (list == null) return;
                    var tType = IsGenericList(property.PropertyType)
                        ? property.PropertyType.GetTypeInfo().GetGenericArguments()[0]
                        : null;
                    var addMethod = GetListAddMethod(tType);
                    var clearMethod = GetListClearMethod(tType);
                    var targetType = property.PropertyType;
                    if (tType == null)
                        targetType = typeof(List<>).MakeGenericType(GetNonGenericListItemType(targetType));
                    var propertyValue = section.Create(targetType, Type, property.Name, valueConverters, defaultTypes, Resolver);
                    clearMethod.Invoke(list, null);
                    foreach (var item in (IEnumerable)propertyValue)
                        addMethod.Invoke(list, new[] { item });
                }
            }

            private void SetReadonlyDictionaryProperty(object obj, PropertyInfo property, DefaultTypes defaultTypes, ValueConverters valueConverters)
            {
                if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                {
                    var dictionary = property.GetValue(obj);
                    if (dictionary == null) return;
                    var tValueType = property.PropertyType.GetTypeInfo().GetGenericArguments()[1];
                    var addMethod = GetDictionaryAddMethod(tValueType);
                    var clearMethod = GetDictionaryClearMethod(tValueType);
                    var propertyValue = section.Create(property.PropertyType, Type, property.Name, valueConverters, defaultTypes, Resolver);
                    clearMethod.Invoke(dictionary, null);
                    foreach (var item in (IEnumerable)propertyValue)
                        addMethod.Invoke(dictionary, new[] { item });
                }
            }

            private ConstructorInfo GetConstructor()
            {
                var constructors = Type.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                if (constructors.Length == 0) throw Exceptions.NoPublicConstructorsFound;
                var orderedConstructors = constructors
                    .Select(ctor => new ConstructorOrderInfo(ctor, _members, Resolver))
                    .OrderBy(x => x)
                    .ToList();
                if (orderedConstructors.Count > 1 && orderedConstructors[0].CompareTo(orderedConstructors[1]) == 0)
                    throw Exceptions.AmbiguousConstructors(Type);
                if (!orderedConstructors[0].IsInvokableWithDefaultParameters)
                    throw Exceptions.MissingRequiredConstructorParameters(Configuration, orderedConstructors[0]);
                return orderedConstructors[0].Constructor;
            }

            private object[] GetArgs(ConstructorInfo constructor, ValueConverters valueConverters, DefaultTypes defaultTypes)
            {
                var parameters = constructor.GetParameters();
                var args = new object[parameters.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    if (_members.TryGetValue(parameters[i].Name, out IConfigurationSection section))
                    {
                        var arg = section.Create(parameters[i].ParameterType, Type, parameters[i].Name, valueConverters, defaultTypes, Resolver);
                        if (parameters[i].ParameterType.GetTypeInfo().IsInstanceOfType(arg))
                        {
                            args[i] = arg;
                            _members.Remove(parameters[i].Name);
                        }
                    }
                    else if (Resolver.TryResolve(parameters[i], out object arg))
                        args[i] = arg;
                    else if (parameters[i].HasDefaultValue)
                        args[i] = parameters[i].DefaultValue;
                }
                return args;
            }

            private IEnumerable<PropertyInfo> WritableProperties =>
                Type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);

            private IEnumerable<PropertyInfo> ReadonlyListProperties =>
                Type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.IsReadonlyList());

            private IEnumerable<PropertyInfo> ReadonlyDictionaryProperties =>
                Type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.IsReadonlyDictionary());

            private class IdentifierComparer : IEqualityComparer<string>
            {
                private static readonly StringComparer _comparer = StringComparer.OrdinalIgnoreCase;

                public bool Equals(string x, string y)
                {
                    if (_comparer.Equals(x, y))
                        return true;

                    var xWords = SplitIntoWords(x);
                    var yWords = SplitIntoWords(y);

                    if (xWords.Count == yWords.Count)
                    {
                        for (int i = 0; i < xWords.Count; i++)
                            if (!_comparer.Equals(xWords[i], yWords[i]))
                                return false;

                        return true;
                    }
                    else if (xWords.Count == 1 && _comparer.Equals(xWords[0], string.Join("", yWords)))
                        return true;
                    else if (yWords.Count == 1 && _comparer.Equals(yWords[0], string.Join("", xWords)))
                        return true;

                    return false;
                }

                /// <summary>
                /// It is impossible to calculate a hash that identifies a both case-insensitive overall
                /// match and a word-separated case-insensitive match. For example "Foo-Bar" needs to have
                /// the same hash as "foobar", but there is no way of knowing how to split a value with
                /// no separators and no variation in casing. Returning a constant hash ensures that a
                /// dictionary will handle all identifier casing variations correctly, albeit without
                /// the optimization of a well-partitioned hash.
                /// </summary>
                public int GetHashCode(string obj) => 0;

                private static IReadOnlyList<string> SplitIntoWords(string identifier)
                {
                    if (identifier.Contains('_'))
                        return identifier.Split('_');
                    else if (identifier.Contains('-'))
                        return identifier.Split('-');
                    else
                    {
                        var words = new List<string>();

                        for (int i = 0; i < identifier.Length; i++)
                        {
                            if (IsUpperCaseWord(i))
                                words.Add(GetUpperCaseWord(ref i));
                            else
                                words.Add(GetWord(ref i));
                        }

                        bool IsUpperCaseWord(int index) =>
                                index <= identifier.Length - 2
                                && char.IsUpper(identifier[index])
                                && char.IsUpper(identifier[index + 1]);

                        string GetUpperCaseWord(ref int index)
                        {
                            var sb = new StringBuilder();
                            while (index < identifier.Length)
                            {
                                if (char.IsUpper(identifier[index]))
                                    sb.Append(identifier[index]);
                                else
                                {
                                    sb.Remove(sb.Length - 1, 1);
                                    index -= 2;
                                    break;
                                }
                                index++;
                            }
                            return sb.ToString();
                        }

                        string GetWord(ref int index)
                        {
                            var sb = new StringBuilder();
                            sb.Append(identifier[index]);
                            while (++index < identifier.Length && !char.IsUpper(identifier[index]))
                                sb.Append(identifier[index]);
                            index--;
                            return sb.ToString();
                        }

                        return words;
                    }
                }
            }
        }
    }
}
