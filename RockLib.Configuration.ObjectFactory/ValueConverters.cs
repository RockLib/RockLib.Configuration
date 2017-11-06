using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// A container for functions that are used to convert a configuration string value to a
    /// target type.
    /// </summary>
    public class ValueConverters : IValueConverters, IEnumerable<KeyValuePair<string, Type>>
    {
        private readonly Dictionary<string, ValueConverter> _converters = new Dictionary<string, ValueConverter>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Configures a converter for the member (property or constructor parameter) specified by the
        /// provided declaring type and member name. Use this method when you need different members of
        /// a target type to each use a different converter. If you want all members of a target type to
        /// use the same converter, use one of the other <see cref="Add(Type, Type)"/> or
        /// <see cref="Add{T}(Type, Func{string, T})"/> methods.
        /// </summary>
        /// <param name="declaringType">The declaring type of a member that needs a converter.</param>
        /// <param name="memberName">The name of a member that needs a converter.</param>
        /// <param name="convertFunc">A function that does the conversion from string to <typeparamref name="T"/>.</param>
        /// <returns>This instance of <see cref="ValueConverters"/>.</returns>
        /// <typeparam name="T">The concrete type of the object returned by the <paramref name="convertFunc"/> function.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="declaringType"/>, <paramref name="memberName"/>, or <paramref name="convertFunc"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If there are no members of <paramref name="declaringType"/> that match <paramref name="memberName"/>, or
        /// if the <typeparamref name="T"/> type is not assignable to any of the matching members.
        /// </exception>
        public ValueConverters Add<T>(Type declaringType, string memberName, Func<string, T> convertFunc)
        {
            if (convertFunc == null) throw new ArgumentNullException(nameof(convertFunc));
            return Add(declaringType, memberName, typeof(T), value => convertFunc(value));
        }

        /// <summary>
        /// Configures a converter for the specified target type. Use this method when you want all
        /// members (properties or constructor parameters) of the target type to use the same converter.
        /// If you need different members of a target type to each use a different converter, use one of
        /// the other <see cref="Add{T}(Type, string, Func{string, T})"/> or
        /// <see cref="Add(Type, string, Type)"/> methods.
        /// </summary>
        /// <param name="targetType">A type that needs a default type.</param>
        /// <param name="defaultType">The default type for the specified target type.</param>
        /// <returns>This instance of <see cref="DefaultTypes"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="targetType"/> or <paramref name="convertFunc"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the <typeparamref name="T"/> type is not assignable to <paramref name="targetType"/>.
        /// </exception>
        public ValueConverters Add<T>(Type targetType, Func<string, T> convertFunc)
        {
            if (convertFunc == null) throw new ArgumentNullException(nameof(convertFunc));
            return Add(targetType, typeof(T), value => convertFunc(value));
        }

        /// <summary>
        /// Attempt to get a convert function for a member (property or construtor parameter) specified by
        /// its declaring type and name.
        /// </summary>
        /// <param name="declaringType">A declaring type of the member to find a converter for.</param>
        /// <param name="memberName">The name of the member to find a converter for.</param>
        /// <param name="convertFunc">
        /// When a match is found for the member, contains the convert function that is used to convert a
        /// configuration value to the member's type.
        /// </param>
        /// <returns>
        /// True, if a converter was found for the member. Otherwise, false if a converter could not be found.
        /// </returns>
        public bool TryGet(Type declaringType, string memberName, out Func<string, object> convertFunc) =>
            _converters.TryGetValue(GetKey(declaringType, memberName), out ValueConverter converter)
                ? (convertFunc = converter.ConvertFunc) != null
                : (convertFunc = null) != null;

        /// <summary>
        /// Attempt to get a convert function for a specified target type.
        /// </summary>
        /// <param name="targetType">The type to find a converter for.</param>
        /// <param name="convertFunc">
        /// When a match is found for the member, contains the convert function that is used to convert a
        /// configuration value to the target type.
        /// </param>
        /// <returns>
        /// True, if a converter was found for the member. Otherwise, false if a converter could not be found.
        /// </returns>
        public bool TryGet(Type targetType, out Func<string, object> convertFunc) =>
            _converters.TryGetValue(GetKey(targetType), out ValueConverter converter)
                ? (convertFunc = converter.ConvertFunc) != null
                : (convertFunc = null) != null;

        private ValueConverters Add(Type declaringType, string memberName, Type returnType, Func<string, object> convertFunc)
        {
            if (declaringType == null) throw new ArgumentNullException(nameof(declaringType));
            if (memberName == null) throw new ArgumentNullException(nameof(memberName));

            var matchingMembers = Members.Find(declaringType, memberName).ToList();

            if (matchingMembers.Count == 0) throw Exceptions.NoMatchingMembers(declaringType, memberName);
            var notAssignableMembers = matchingMembers.Where(m => !m.Type.GetTypeInfo().IsAssignableFrom(returnType)).ToList();
            if (notAssignableMembers.Count > 0) throw Exceptions.ReturnTypeOfConvertFuncNotAssignableToMembers(declaringType, memberName, returnType, notAssignableMembers);

            _converters.Add(GetKey(declaringType, memberName), new ValueConverter(returnType, convertFunc));
            return this;
        }

        private ValueConverters Add(Type targetType, Type returnType, Func<string, object> convertFunc)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (!targetType.GetTypeInfo().IsAssignableFrom(returnType)) throw Exceptions.ReturnTypeOfConvertFuncIsNotAssignableToTargetType(targetType, returnType);
            _converters.Add(GetKey(targetType), new ValueConverter(returnType, convertFunc));
            return this;
        }

        private static string GetKey(Type declaringType, string memberName) =>
                (declaringType != null && memberName != null) ? declaringType.FullName + "::" + memberName : "";

        private static string GetKey(Type targetType) => targetType != null ? targetType.FullName : "";

        private class ValueConverter
        {
            public readonly Type ReturnType;
            public readonly Func<string, object> ConvertFunc;

            public ValueConverter(Type returnType, Func<string, object> convertFunc)
            {
                ReturnType = returnType;
                ConvertFunc = convertFunc;
            }
        }

        IEnumerator<KeyValuePair<string, Type>> IEnumerable<KeyValuePair<string, Type>>.GetEnumerator() => Enumerable.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Enumerable).GetEnumerator();
        private IEnumerable<KeyValuePair<string, Type>> Enumerable =>
            _converters.Select(x => new KeyValuePair<string, Type>(x.Key, x.Value.ReturnType));
    }
}
