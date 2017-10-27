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

        /// <summary>
        /// Create an objecgt of type <typeparamref name="T"/> based on the specified configuration.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="configuration">The configuration to create the object from.</param>
        /// <param name="convertFunc">
        /// A function that overrides the default conversion mechanism by taking a configuration string value
        /// and converting it to a new instance of target type. If the function cannot or should not convert a
        /// value to the target type, it should return null so the default conversion mechanism can attempt to
        /// convert the value.
        /// </param>
        /// <returns>An object of type <typeparamref name="T"/> with values set from the configuration.</returns>
        public static T Create<T>(this IConfiguration configuration, ConvertFunc convertFunc = null, DefaultTypes defaultTypes = null)
        {
            return (T)configuration.Create(typeof(T), convertFunc, defaultTypes);
        }

        /// <summary>
        /// Create an object of the specified type based on the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration to create the object from.</param>
        /// <param name="type">The type of object to create.</param>
        /// <param name="convertFunc">
        /// A function that overrides the default conversion mechanism by taking a configuration string value
        /// and converting it to a new instance of target type. If the function cannot or should not convert a
        /// value to the target type, it should return null so the default conversion mechanism can attempt to
        /// convert the value.
        /// </param>
        /// <returns>An object with values set from the configuration.</returns>
        public static object Create(this IConfiguration configuration, Type type, ConvertFunc convertFunc = null, DefaultTypes defaultTypes = null)
        {
            return configuration.Create(type, null, null, convertFunc, defaultTypes ?? DefaultTypes.Empty);
        }

        private static object Create(this IConfiguration configuration, Type targetType, Type declaringType, string memberName, ConvertFunc convertFunc, IDefaultTypes defaultTypes)
        {
            if (IsConfigurationValue(configuration, out IConfigurationSection section)) return ConvertToType(section, targetType, declaringType, memberName, convertFunc, defaultTypes);
            if (IsTypeSpecifiedObject(configuration)) return BuildTypeSpecifiedObject(configuration, targetType, declaringType, memberName, convertFunc, defaultTypes);
            if (targetType.IsArray) return BuildArray(configuration, targetType, declaringType, memberName, convertFunc, defaultTypes);
            if (IsList(targetType)) return BuildList(configuration, targetType, declaringType, memberName, convertFunc, defaultTypes);
            if (IsDictionary(targetType)) return BuildDictionary(configuration, targetType, declaringType, memberName, convertFunc, defaultTypes);
            if (targetType.GetTypeInfo().IsAbstract) throw GetCannotCreateAbstractTypeException(configuration, targetType);
            return BuildObject(configuration, targetType, declaringType, memberName, convertFunc, defaultTypes);
        }

        private static bool IsConfigurationValue(IConfiguration configuration, out IConfigurationSection section)
        {
            section = configuration as IConfigurationSection;
            return section != null && section.Value != null;
        }

        private static object ConvertToType(
            IConfigurationSection section, Type targetType, Type declaringType, string memberName, ConvertFunc convertFunc, IDefaultTypes defaultTypes)
        {
            if (convertFunc != null)
            {
                var result = convertFunc(section.Value, targetType, declaringType, memberName);
                if (result != null)
                {
                    if (!targetType.GetTypeInfo().IsInstanceOfType(result))
                        throw new InvalidOperationException($"The {result.GetType()} object that was returned by the ConvertFunc "
                            + $"callback was not assignable to the target type {targetType} from value '{section.Value}'.");
                    return result;
                }
            }
            if (targetType == typeof(Encoding)) return Encoding.GetEncoding(section.Value);
            var typeConverter = TypeDescriptor.GetConverter(targetType);
            if (typeConverter.CanConvertFrom(typeof(string)))
                return typeConverter.ConvertFromInvariantString(section.Value);
            if (section.Value == "") return new ObjectBuilder(targetType).Build(convertFunc, defaultTypes);
            throw new InvalidOperationException($"Unable to convert section '{section.Key}' to type '{targetType}'.");
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

        private static object BuildTypeSpecifiedObject(IConfiguration configuration, Type targetType, Type declaringType, string memberName, ConvertFunc convertFunc, IDefaultTypes defaultTypes)
        {
            var typeSection = configuration.GetSection(_typeKey);
            var specifiedType = Type.GetType(typeSection.Value, throwOnError: true);
            if (!targetType.GetTypeInfo().IsAssignableFrom(specifiedType)) throw GetNotAssignableException(targetType, specifiedType);
            return configuration.GetSection(_valueKey).Create(specifiedType, declaringType, memberName, convertFunc, defaultTypes);
        }

        private static object BuildArray(IConfiguration configuration, Type targetType, Type declaringType, string memberName, ConvertFunc convertFunc, IDefaultTypes defaultTypes)
        {
            if (!IsList(configuration)) throw GetConfigurationIsNotAListException(configuration, targetType);
            if (targetType.GetArrayRank() > 1) throw GetArrayRankGreaterThanOneIsNotSupportedException(targetType);
            var elementType = targetType.GetElementType();
            var items = configuration.GetChildren().Select(child => child.Create(elementType, declaringType, memberName, convertFunc, defaultTypes)).ToList();
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

        private static object BuildList(IConfiguration configuration, Type targetType, Type declaringType, string memberName, ConvertFunc convertFunc, IDefaultTypes defaultTypes)
        {
            if (!IsList(configuration)) throw GetConfigurationIsNotAListException(configuration, targetType);
            var tType = targetType.GetTypeInfo().GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(tType);
            var addMethod = GetListAddMethod(listType, tType);
            var list = Activator.CreateInstance(listType);
            foreach (var item in configuration.GetChildren().Select(child => child.Create(tType, declaringType, memberName, convertFunc, defaultTypes)))
                addMethod.Invoke(list, new object[] { item });
            return list;
        }

        private static bool IsList(IConfiguration configuration)
        {
            int i = 0;
            foreach (var child in configuration.GetChildren())
                if (child.Key != i++.ToString()) return false;
            return true;
        }

        private static bool IsDictionary(Type type) =>
            type.GetTypeInfo().IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                && type.GetTypeInfo().GetGenericArguments()[0] == typeof(string);

        private static object BuildDictionary(IConfiguration configuration, Type targetType, Type declaringType, string memberName, ConvertFunc convertFunc, IDefaultTypes defaultTypes)
        {
            var tValueType = targetType.GetTypeInfo().GetGenericArguments()[1];
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), tValueType);
            var addMethod = dictionaryType.GetTypeInfo().GetMethod("Add", new[] { typeof(string), tValueType });
            var dictionary = Activator.CreateInstance(dictionaryType);
            foreach (var x in configuration.GetChildren().Select(c => new { c.Key, Value = c.Create(tValueType, declaringType, memberName, convertFunc, defaultTypes) }))
                addMethod.Invoke(dictionary, new object[] { x.Key, x.Value });
            return dictionary;
        }

        private static object BuildObject(IConfiguration configuration, Type targetType, Type declaringType, string memberName, ConvertFunc convertFunc, IDefaultTypes defaultTypes)
        {
            if (defaultTypes.TryGet(declaringType, memberName, out Type defaultType))
                targetType = defaultType;
            var builder = new ObjectBuilder(targetType);
            foreach (var childSection in configuration.GetChildren())
                builder.AddMember(childSection.Key, childSection);
            return builder.Build(convertFunc, defaultTypes);
        }

        private static MethodInfo GetListAddMethod(Type listType, Type tType) =>
            typeof(ICollection<>).MakeGenericType(listType.GetTypeInfo().GetGenericArguments()[0])
                .GetTypeInfo().GetMethod("Add", new[] { tType });

        private static Exception GetNotAssignableException(Type type, Type specifiedType) =>
            new InvalidOperationException($"The configuration-specified type, '{specifiedType}' is not assignable to the target type, {type}.");

        private static Exception GetCannotCreateAbstractTypeException(IConfiguration configuration, Type type)
        {
            if (configuration is IConfigurationSection section)
                return new InvalidOperationException($"Cannot create instance of abstract target type, '{type}', from section '{section.Key}' at path '{section.Path}'.");
            return new InvalidOperationException($"Cannot create instance of abstract target type, '{type}' from configuration.");
        }

        private static Exception GetNoConstructorsException() =>
            new InvalidOperationException("No public constructors found.");

        private static Exception GetAmbiguousConstructorsException(Type type) =>
            new InvalidOperationException($"Ambiguous best-match constructors in '{type}' type. Constructors are ordered as following: 1) from "
                + "highest-to-lowest ratio of matched parameters to total parameters, 2) then from highest-to-lowest ratio of matched parameters "
                + "or parameters with a default value to total parameters, 3) then from the highest-to-lowest number of total parameters.");

        private static Exception GetConfigurationIsNotAListException(IConfiguration configuration, Type type)
        {
            if (configuration is IConfigurationSection section)
                return new InvalidOperationException($"Configuration from section '{section.Key}' at path '{section.Path}' does not represent a list but should be convertible to type '{type}'.");
            return new InvalidOperationException($"Configuration does not represent a list but should be convertible to type '{type}'.");
        }

        private static Exception GetArrayRankGreaterThanOneIsNotSupportedException(Type type) =>
            new InvalidOperationException($"Arrays with rank greater than zero are not supported. The type specified was '{type}'.");

        private class ObjectBuilder
        {
            private readonly Dictionary<string, IConfigurationSection> _members = new Dictionary<string, IConfigurationSection>(StringComparer.OrdinalIgnoreCase);

            public ObjectBuilder(Type type)
            {
                Type = type;
            }

            public Type Type { get; }

            public void AddMember(string memberName, IConfigurationSection section) => _members.Add(memberName, section);

            public object Build(ConvertFunc convertFunc, IDefaultTypes defaultTypes)
            {
                var constructor = GetConstructor();
                var args = GetArgs(constructor, convertFunc, defaultTypes);
                var obj = constructor.Invoke(args);
                foreach (var property in GetReadWriteProperties())
                    if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                    {
                        var propertyValue = section.Create(property.PropertyType, Type, property.Name, convertFunc, defaultTypes);
                        property.SetValue(obj, propertyValue);
                    }
                foreach (var property in GetReadonlyListProperties())
                {
                    if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                    {
                        var tType = property.PropertyType.GetTypeInfo().GetGenericArguments()[0];
                        var addMethod = GetListAddMethod(property.PropertyType, tType);
                        var list = property.GetValue(obj);
                        var propertyValue = section.Create(property.PropertyType, Type, property.Name, convertFunc, defaultTypes);
                        foreach (var item in (IEnumerable)propertyValue)
                            addMethod.Invoke(list, new[] { item });
                    }
                }
                foreach (var property in GetReadonlyDictionaryProperties())
                {
                    if (_members.TryGetValue(property.Name, out IConfigurationSection section))
                    {
                        var tValueType = property.PropertyType.GetTypeInfo().GetGenericArguments()[1];
                        var addMethod = typeof(ICollection<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), tValueType)).GetTypeInfo().GetMethod("Add");
                        var dictionary = property.GetValue(obj);
                        var keysProperty = property.PropertyType.GetTypeInfo().GetProperty("Keys");
                        var propertyValue = section.Create(property.PropertyType, Type, property.Name, convertFunc, defaultTypes);
                        var enumerator = ((IEnumerable)propertyValue).GetEnumerator();
                        while (enumerator.MoveNext())
                            addMethod.Invoke(dictionary, new object[] { enumerator.Current });
                    }
                }
                return obj;
            }

            private ConstructorInfo GetConstructor()
            {
                var constructors = GetConstructors();
                if (constructors.Length == 1) return constructors[0];
                if (constructors.Length == 0) throw GetNoConstructorsException();
                var orderedConstructors = constructors
                    .Select(ctor => new ConstructorOrderInfo(ctor, _members))
                    .OrderByDescending(x => x.MatchedParametersRatio)
                    .ThenByDescending(x => x.MatchedOrDefaultParametersRatio)
                    .ThenByDescending(x => x.TotalParameters)
                    .ToList();
                if (orderedConstructors[0].HasSameSortOrderAs(orderedConstructors[1]))
                    throw GetAmbiguousConstructorsException(Type);
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

            private object[] GetArgs(ConstructorInfo constructor, ConvertFunc convertFunc, IDefaultTypes defaultTypes)
            {
                var parameters = constructor.GetParameters();
                var args = new object[parameters.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    if (_members.TryGetValue(parameters[i].Name, out IConfigurationSection section))
                    {
                        var arg = section.Create(parameters[i].ParameterType, Type, parameters[i].Name, convertFunc, defaultTypes);
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

            private IEnumerable<PropertyInfo> GetReadWriteProperties() =>
                Type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite);

            private ConstructorInfo[] GetConstructors() =>
                Type.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            private IEnumerable<PropertyInfo> GetReadonlyListProperties() =>
                Type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p =>
                    p.CanRead
                    && !p.CanWrite
                    && p.PropertyType.GetTypeInfo().IsGenericType
                    && (p.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                        || p.PropertyType.GetGenericTypeDefinition() == typeof(IList<>)
                        || p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)));

            private IEnumerable<PropertyInfo> GetReadonlyDictionaryProperties() =>
                Type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p =>
                    p.CanRead
                    && !p.CanWrite
                    && p.PropertyType.GetTypeInfo().IsGenericType
                    && (p.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                        || p.PropertyType.GetGenericTypeDefinition() == typeof(IDictionary<,>)));
        }
    }
}
