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
            if (IsValueSection(configuration, out IConfigurationSection valueSection)) return ConvertToType(valueSection, targetType, declaringType, memberName, valueConverters, defaultTypes);
            if (IsTypeSpecifiedObject(configuration)) return BuildTypeSpecifiedObject(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes);
            if (targetType.IsArray) return BuildArray(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes);
            if (IsList(targetType)) return BuildList(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes);
            if (IsStringDictionary(targetType)) return BuildStringDictionary(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes);
            return BuildObject(configuration, targetType, declaringType, memberName, valueConverters, defaultTypes);
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
            if (valueConverters.TryGet(declaringType, memberName, out Func<string, object> convert)
                || valueConverters.TryGet(targetType, out convert))
            {
                var result = convert(valueSection.Value);
                if (result == null) throw Exceptions.ResultCannotBeNull(targetType, declaringType, memberName);
                return result;
            }

            if (targetType == typeof(Encoding)) return Encoding.GetEncoding(valueSection.Value);
            var typeConverter = TypeDescriptor.GetConverter(targetType);
            if (typeConverter.CanConvertFrom(typeof(string)))
                return typeConverter.ConvertFromInvariantString(valueSection.Value);
            if (valueSection.Value == "") return new ObjectBuilder(targetType).Build(valueConverters, defaultTypes);
            throw Exceptions.CannotConvertSectionValueToTargetType(valueSection, targetType);
        }

        private static bool IsTypeSpecifiedObject(IConfiguration configuration)
        {
            var typeFound = false;
            var i = 0;
            foreach (var child in configuration.GetChildren())
            {
                if (child.Key == _typeKey)
                {
                    if (string.IsNullOrEmpty(child.Value)) return false;
                    typeFound = true;
                }
                else if (child.Key != _valueKey) return false;
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
            if (!IsList(configuration)) throw Exceptions.ConfigurationIsNotAList(configuration, targetType);
            if (targetType.GetArrayRank() > 1) throw Exceptions.ArrayRankGreaterThanOneIsNotSupported(targetType);
            var elementType = targetType.GetElementType();
            var items = configuration.GetChildren().Select(child => child.Create(elementType, declaringType, memberName, valueConverters, defaultTypes)).ToList();
            var array = Array.CreateInstance(elementType, items.Count);
            for (int i = 0; i < array.Length; i++)
                array.SetValue(items[i], i);
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
            if (!IsList(configuration)) throw Exceptions.ConfigurationIsNotAList(configuration, targetType);
            var tType = targetType.GetTypeInfo().GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(tType);
            var addMethod = GetListAddMethod(listType, tType);
            var list = Activator.CreateInstance(listType);
            foreach (var item in configuration.GetChildren().Select(child => child.Create(tType, declaringType, memberName, valueConverters, defaultTypes)))
                addMethod.Invoke(list, new object[] { item });
            return list;
        }

        private static bool IsList(IConfiguration configuration, bool includeEmptyList = true)
        {
            int i = 0;
            foreach (var child in configuration.GetChildren())
                if (child.Key != i++.ToString()) return false;
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
            if (targetType.GetTypeInfo().IsAbstract) throw Exceptions.CannotCreateAbstractType(configuration, targetType);
            if (targetType == typeof(object)) throw Exceptions.CannotCreateObjectType;
            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(targetType)) throw Exceptions.UnsupportedCollectionType(targetType);
            if (IsList(configuration, includeEmptyList:false)) throw Exceptions.ConfigurationIsAList(configuration, targetType);
            var builder = new ObjectBuilder(targetType);
            foreach (var childSection in configuration.GetChildren())
                builder.AddMember(childSection.Key, childSection);
            return builder.Build(valueConverters, defaultTypes);
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
                if (targetType.GetTypeInfo().IsAssignableFrom(defaultType)) return true;
                throw Exceptions.DefaultTypeIsNotAssignableToTargetType(targetType, defaultType);
            }

            return false;
        }

        /// <summary>Gets the default type, or null if not found.</summary>
        private static Type GetDefaultTypeFromMemberCustomAttributes(Type declaringType, string memberName)
        {
            var set = new HashSet<Type>();

            foreach (var member in Members.Find(declaringType, memberName))
            {
                if (member.MemberType == MemberType.Property)
                {
                    var property = declaringType.GetTypeInfo().GetProperty(member.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    var defaultType = GetDefaultTypeFromCustomAttributes(property.CustomAttributes);
                    if (defaultType != null) set.Add(defaultType);
                }
                else
                {
                    foreach (var parameter in declaringType.GetTypeInfo().GetConstructors()
                        .SelectMany(c => c.GetParameters())
                        .Where(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, member.Name)))
                    {
                        var defaultType = GetDefaultTypeFromCustomAttributes(parameter.CustomAttributes);
                        if (defaultType != null) set.Add(defaultType);
                    }
                }
            }

            switch (set.Count)
            {
                case 0: return null;
                case 1: return set.First();
                default: throw Exceptions.InconsistentDefaultTypeAttributesForMultipleMembers(memberName);
            }
        }

        /// <summary>Gets the default type, or null if not found.</summary>
        private static Type GetDefaultTypeFromCustomAttributes(IEnumerable<CustomAttributeData> customAttributes) =>
            customAttributes.Where(attribute =>
                attribute.AttributeType.Name == nameof(DefaultTypeAttribute)
                    && attribute.ConstructorArguments.Count == 1
                    && attribute.ConstructorArguments[0].ArgumentType == typeof(Type))
                .Select(attribute => (Type)attribute.ConstructorArguments[0].Value)
                .FirstOrDefault();

        private static MethodInfo GetListAddMethod(Type listType, Type tType) =>
            typeof(ICollection<>).MakeGenericType(listType.GetTypeInfo().GetGenericArguments()[0])
                .GetTypeInfo().GetMethod("Add", new[] { tType });

        private class ObjectBuilder
        {
            private readonly Dictionary<string, IConfigurationSection> _members = new Dictionary<string, IConfigurationSection>(StringComparer.OrdinalIgnoreCase);

            public ObjectBuilder(Type type) => Type = type;

            public Type Type { get; }

            public void AddMember(string memberName, IConfigurationSection section) => _members.Add(memberName, section);

            public object Build(IValueConverters valueConverters, IDefaultTypes defaultTypes)
            {
                var constructor = GetConstructor();
                var args = GetArgs(constructor, valueConverters, defaultTypes);
                var obj = constructor.Invoke(args);
                foreach (var property in ReadWriteProperties)
                    if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                    {
                        var propertyValue = section.Create(property.PropertyType, Type, property.Name, valueConverters, defaultTypes);
                        property.SetValue(obj, propertyValue);
                    }
                foreach (var property in ReadonlyListProperties)
                {
                    if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                    {
                        var tType = property.PropertyType.GetTypeInfo().GetGenericArguments()[0];
                        var addMethod = GetListAddMethod(property.PropertyType, tType);
                        var list = property.GetValue(obj);
                        var propertyValue = section.Create(property.PropertyType, Type, property.Name, valueConverters, defaultTypes);
                        foreach (var item in (IEnumerable)propertyValue)
                            addMethod.Invoke(list, new[] { item });
                    }
                }
                foreach (var property in ReadonlyDictionaryProperties)
                {
                    if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                    {
                        var tValueType = property.PropertyType.GetTypeInfo().GetGenericArguments()[1];
                        var addMethod = typeof(ICollection<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), tValueType)).GetTypeInfo().GetMethod("Add");
                        var dictionary = property.GetValue(obj);
                        var keysProperty = property.PropertyType.GetTypeInfo().GetProperty("Keys");
                        var propertyValue = section.Create(property.PropertyType, Type, property.Name, valueConverters, defaultTypes);
                        var enumerator = ((IEnumerable)propertyValue).GetEnumerator();
                        while (enumerator.MoveNext())
                            addMethod.Invoke(dictionary, new object[] { enumerator.Current });
                    }
                }
                return obj;
            }

            private ConstructorInfo GetConstructor()
            {
                var constructors = Constructors;
                if (constructors.Length == 1) return constructors[0];
                if (constructors.Length == 0) throw Exceptions.NoPublicConstructorsFound;
                var orderedConstructors = constructors
                    .Select(ctor => new ConstructorOrderInfo(ctor, _members))
                    .OrderByDescending(x => x.MatchedParametersRatio)
                    .ThenByDescending(x => x.MatchedOrDefaultParametersRatio)
                    .ThenByDescending(x => x.TotalParameters)
                    .ToList();
                if (orderedConstructors[0].HasSameSortOrderAs(orderedConstructors[1]))
                    throw Exceptions.AmbiguousConstructors(Type);
                return orderedConstructors[0].Constructor;
            }

            private class ConstructorOrderInfo
            {
                public ConstructorOrderInfo(ConstructorInfo constructor, Dictionary<string, IConfigurationSection> members)
                {
                    Constructor = constructor;
                    var parameters = constructor.GetParameters();
                    TotalParameters = parameters.Length;
                    MatchedParametersRatio = parameters.Count(p => members.ContainsKey(p.Name)) / (double)TotalParameters;
                    MatchedOrDefaultParametersRatio = parameters.Count(p => members.ContainsKey(p.Name) || p.HasDefaultValue) / (double)TotalParameters;
                }

                public ConstructorInfo Constructor { get; }
                public double MatchedParametersRatio { get; }
                public double MatchedOrDefaultParametersRatio { get; }
                public int TotalParameters { get; }

                public bool HasSameSortOrderAs(ConstructorOrderInfo other) =>
                    MatchedParametersRatio == other.MatchedParametersRatio
                        && MatchedOrDefaultParametersRatio == other.MatchedOrDefaultParametersRatio
                        && TotalParameters == other.TotalParameters;
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
                    
                    else if (parameters[i].HasDefaultValue) args[i] = parameters[i].DefaultValue;
                }
                return args;
            }

            private IEnumerable<PropertyInfo> ReadWriteProperties =>
                Type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.IsReadWrite());

            private ConstructorInfo[] Constructors =>
                Type.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            private IEnumerable<PropertyInfo> ReadonlyListProperties =>
                Type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.IsReadonlyList());

            private IEnumerable<PropertyInfo> ReadonlyDictionaryProperties =>
                Type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.IsReadonlyDictionary());
        }
    }
}
