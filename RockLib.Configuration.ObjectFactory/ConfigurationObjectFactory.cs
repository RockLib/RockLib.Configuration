using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Static class that allows the creation of objects based on configuration values.
    /// </summary>
    public static class ConfigurationObjectFactory
    {
        private const string _typeKey = "type";
        private const string _valueKey = "value";

        private static readonly IDefaultTypes _emptyDefaultTypes = new DefaultTypes();
        private static readonly IValueConverters _emptyValueConverters = new ValueConverters();

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
        public static T Create<T>(this IConfiguration configuration, DefaultTypes defaultTypes = null, ValueConverters valueConverters = null) =>
            (T)configuration.Create(typeof(T), defaultTypes, valueConverters);

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
        public static object Create(this IConfiguration configuration, Type type, DefaultTypes defaultTypes = null, ValueConverters valueConverters = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (type == null) throw new ArgumentNullException(nameof(type));
            return configuration.Create(type, null, null, valueConverters ?? _emptyValueConverters, defaultTypes ?? _emptyDefaultTypes);
        }

        private static object Create(this IConfiguration configuration, Type targetType, Type declaringType, string memberName, IValueConverters valueConverters, IDefaultTypes defaultTypes)
        {
            if (targetType.IsArray)
                return BuildArray(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes);
            if (IsList(targetType))
                return BuildList(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes);
            if (IsValueSection(configuration, out IConfigurationSection valueSection))
                return ConvertToType(valueSection, targetType, declaringType, memberName, valueConverters, defaultTypes);
            if (IsTypeSpecifiedObject(configuration))
                return BuildTypeSpecifiedObject(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes);
            if (IsStringDictionary(targetType))
                return BuildStringDictionary(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes);
            return BuildObject(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes);
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
            IConfigurationSection valueSection, Type targetType, Type declaringType, string memberName, IValueConverters valueConverters, IDefaultTypes defaultTypes)
        {
            var convert = GetConvertFunc(targetType, declaringType, memberName, valueConverters);
            if (convert != null)
                return convert(valueSection.Value) ?? throw Exceptions.ResultCannotBeNull(targetType, declaringType, memberName);
            if (targetType == typeof(Encoding))
                return Encoding.GetEncoding(valueSection.Value);
            var typeConverter = TypeDescriptor.GetConverter(targetType);
            if (typeConverter.CanConvertFrom(typeof(string)))
                return typeConverter.ConvertFromInvariantString(valueSection.Value);
            if (valueSection.Value == "")
                return new ObjectBuilder(targetType, valueSection).Build(valueConverters, defaultTypes);
            throw Exceptions.CannotConvertSectionValueToTargetType(valueSection, targetType);
        }

        private static Func<string, object> GetConvertFunc(Type targetType, Type declaringType, string memberName, IValueConverters valueConverters)
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

        private static bool IsTypeSpecifiedObject(IConfiguration configuration)
        {
            var typeFound = false;
            var i = 0;
            foreach (var child in configuration.GetChildren())
            {
                if (child.Key.Equals(_typeKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(child.Value)) return false;
                    typeFound = true;
                }
                else if (!child.Key.Equals(_valueKey, StringComparison.OrdinalIgnoreCase)) return false;
                i++;
            }
            if (i == 1) return typeFound;
            return i == 2;
        }

        private static object BuildTypeSpecifiedObject(IConfiguration configuration, Type targetType, Type declaringType, string memberName, IValueConverters valueConverters, IDefaultTypes defaultTypes)
        {
            var typeSection = configuration.GetSection(_typeKey);
            var specifiedType = Type.GetType(typeSection.Value, throwOnError: true);
            if (!targetType.GetTypeInfo().IsAssignableFrom(specifiedType))
                throw Exceptions.ConfigurationSpecifiedTypeIsNotAssignableToTargetType(targetType, specifiedType);
            return BuildObject(configuration.GetSection(_valueKey), specifiedType, declaringType, memberName, valueConverters, defaultTypes, true);
        }

        private static object BuildArray(IConfiguration configuration, Type targetType, Type declaringType, string memberName, IValueConverters valueConverters, IDefaultTypes defaultTypes)
        {
            if (targetType.GetArrayRank() > 1) throw Exceptions.ArrayRankGreaterThanOneIsNotSupported(targetType);

            var elementType = targetType.GetElementType();
            var isValueSection = IsValueSection(configuration);

            Array array;
            if (isValueSection && elementType == typeof(byte))
            {
                var item = (string)configuration.Create(typeof(string), declaringType, memberName, valueConverters, defaultTypes);
                array = Convert.FromBase64String(item);
            }
            else if (isValueSection || !IsList(configuration))
            {
                var item = configuration.Create(elementType, declaringType, memberName, valueConverters, defaultTypes);
                array = Array.CreateInstance(elementType, 1);
                array.SetValue(item, 0);
            }
            else
            {
                var items = configuration.GetChildren().Select(child =>
                    child.Create(elementType, declaringType, memberName, valueConverters, defaultTypes)).ToList();
                array = Array.CreateInstance(elementType, items.Count);
                for (int i = 0; i < array.Length; i++)
                    array.SetValue(items[i], i);
            }
            return array;
        }

        private static bool IsList(Type type) =>
            type.GetTypeInfo().IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(List<>)
                    || type.GetGenericTypeDefinition() == typeof(IList<>)
                    || type.GetGenericTypeDefinition() == typeof(ICollection<>)
                    || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    || type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>)
                    || type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));

        private static object BuildList(IConfiguration configuration, Type targetType, Type declaringType, string memberName, IValueConverters valueConverters, IDefaultTypes defaultTypes)
        {
            var tType = targetType.GetTypeInfo().GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(tType);
            var addMethod = GetListAddMethod(tType);
            var list = Activator.CreateInstance(listType);
            if (IsValueSection(configuration) || !IsList(configuration))
                addMethod.Invoke(list, new[] {configuration.Create(tType, declaringType, memberName, valueConverters, defaultTypes)});
            else
                foreach (var item in configuration.GetChildren().Select(child => child.Create(tType, declaringType, memberName, valueConverters, defaultTypes)))
                    addMethod.Invoke(list, new[] {item});
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

        private static bool IsStringDictionary(Type type) =>
            type.GetTypeInfo().IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                && type.GetTypeInfo().GetGenericArguments()[0] == typeof(string);

        private static object BuildStringDictionary(IConfiguration configuration, Type targetType, Type declaringType, string memberName, IValueConverters valueConverters, IDefaultTypes defaultTypes)
        {
            var tValueType = targetType.GetTypeInfo().GetGenericArguments()[1];
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), tValueType);
            var addMethod = dictionaryType.GetTypeInfo().GetMethod("Add", new[] { typeof(string), tValueType });
            var dictionary = Activator.CreateInstance(dictionaryType);
            foreach (var x in configuration.GetChildren().Select(c => new { c.Key, Value = c.Create(tValueType, declaringType, memberName, valueConverters, defaultTypes) }))
                addMethod.Invoke(dictionary, new object[] { x.Key, x.Value });
            return dictionary;
        }

        private static object BuildObject(IConfiguration configuration, Type targetType, Type declaringType, string memberName, IValueConverters valueConverters, IDefaultTypes defaultTypes, bool skipDefaultTypes = false)
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
            return new ObjectBuilder(targetType, configuration).Build(valueConverters, defaultTypes);
        }

        private static bool IsSimpleType(Type targetType, Type declaringType, string memberName, IValueConverters valueConverters)
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
                || GetConvertFunc(targetType, declaringType, memberName, valueConverters) != null;
        }

        private static bool TryGetDefaultType(IDefaultTypes defaultTypes, Type targetType, Type declaringType, string memberName, out Type defaultType)
        {
            defaultType = null;
            if (declaringType == null || memberName == null) return false;

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
            typeof(ICollection<>).MakeGenericType(tType)
                .GetTypeInfo().GetMethod("Add");

        private static MethodInfo GetDictionaryAddMethod(Type tValueType) =>
            typeof(ICollection<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), tValueType))
                .GetTypeInfo().GetMethod("Add");

        private class ObjectBuilder
        {
            private readonly Dictionary<string, IConfigurationSection> _members = new Dictionary<string, IConfigurationSection>(StringComparer.OrdinalIgnoreCase);

            public ObjectBuilder(Type type, IConfiguration configuration)
            {
                Type = type;
                Configuration = configuration;
                foreach (var childSection in configuration.GetChildren())
                    _members.Add(childSection.Key, childSection);
            }

            public Type Type { get; }
            public IConfiguration Configuration { get; }

            public object Build(IValueConverters valueConverters, IDefaultTypes defaultTypes)
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

            private void SetWritableProperty(object obj, PropertyInfo property, IDefaultTypes defaultTypes, IValueConverters valueConverters)
            {
                if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                {
                    var propertyValue = section.Create(property.PropertyType, Type, property.Name, valueConverters, defaultTypes);
                    property.SetValue(obj, propertyValue);
                }
            }

            private void SetReadonlyListProperty(object obj, PropertyInfo property, IDefaultTypes defaultTypes, IValueConverters valueConverters)
            {
                if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                {
                    var list = property.GetValue(obj);
                    if (list == null) return;
                    var tType = property.PropertyType.GetTypeInfo().GetGenericArguments()[0];
                    var addMethod = GetListAddMethod(tType);
                    var propertyValue = section.Create(property.PropertyType, Type, property.Name, valueConverters, defaultTypes);
                    foreach (var item in (IEnumerable)propertyValue)
                        addMethod.Invoke(list, new[] { item });
                }
            }

            private void SetReadonlyDictionaryProperty(object obj, PropertyInfo property, IDefaultTypes defaultTypes, IValueConverters valueConverters)
            {
                if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                {
                    var dictionary = property.GetValue(obj);
                    if (dictionary == null) return;
                    var tValueType = property.PropertyType.GetTypeInfo().GetGenericArguments()[1];
                    var addMethod = GetDictionaryAddMethod(tValueType);
                    var propertyValue = section.Create(property.PropertyType, Type, property.Name, valueConverters, defaultTypes);
                    foreach (var item in (IEnumerable)propertyValue)
                        addMethod.Invoke(dictionary, new object[] { item });
                }
            }

            private ConstructorInfo GetConstructor()
            {
                var constructors = Type.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                if (constructors.Length == 0) throw Exceptions.NoPublicConstructorsFound;
                var orderedConstructors = constructors
                    .Select(ctor => new ConstructorOrderInfo(ctor, _members))
                    .OrderBy(x => x)
                    .ToList();
                if (orderedConstructors.Count > 1 && orderedConstructors[0].CompareTo(orderedConstructors[1]) == 0)
                    throw Exceptions.AmbiguousConstructors(Type);
                if (orderedConstructors[0].MatchedOrDefaultParametersRatio < 1)
                    throw Exceptions.MissingRequiredConstructorParameters(Configuration, orderedConstructors[0]);
                return orderedConstructors[0].Constructor;
            }

            private object[] GetArgs(ConstructorInfo constructor, IValueConverters valueConverters, IDefaultTypes defaultTypes)
            {
                var parameters = constructor.GetParameters();
                var args = new object[parameters.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    if (_members.TryGetValue(parameters[i].Name, out IConfigurationSection section))
                    {
                        var arg = section.Create(parameters[i].ParameterType, Type, parameters[i].Name, valueConverters, defaultTypes);
                        if (parameters[i].ParameterType.GetTypeInfo().IsInstanceOfType(arg))
                        {
                            args[i] = arg;
                            _members.Remove(parameters[i].Name);
                        }
                    }
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
        }
    }
}
