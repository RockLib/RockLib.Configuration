using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Defines the default types used by <see cref="ConfigurationObjectFactory"/> when a type is not explicitly specified
    /// by a configuration section.
    /// </summary>
    public sealed class DefaultTypes : IDefaultTypes
    {
        private readonly Dictionary<string, Type> _dictionary = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        private DefaultTypes() {}

        internal static IDefaultTypes Empty { get; } = new DefaultTypes();

        /// <summary>
        /// Gets a new instance of <see cref="DefaultTypes"/> that is configured with a default type for the
        /// member (property or constructor parameter) specified by the provided declaring type and member name.
        /// </summary>
        /// <param name="declaringType">The declaring type of a member that needs a default type.</param>
        /// <param name="memberName">The name of a member that needs a default type.</param>
        /// <param name="defaultType">The default type for the specified member.</param>
        /// <returns>An instance of <see cref="DefaultTypes"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="declaringType"/>, <paramref name="memberName"/>, or <paramref name="defaultType"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If there are no members of <paramref name="declaringType"/> that match <paramref name="memberName"/>, or
        /// if <paramref name="defaultType"/> is not assignable to any of the matching members.
        /// </exception>
        public static DefaultTypes New(Type declaringType, string memberName, Type defaultType)
        {
            return new DefaultTypes().Add(declaringType, memberName, defaultType);
        }

        /// <summary>
        /// Gets a new instance of <see cref="DefaultTypes"/> that is configured with a default type for the
        /// specified target type.
        /// </summary>
        /// <param name="targetType">A type that needs a default type.</param>
        /// <param name="defaultType">The default type for the specified target type.</param>
        /// <returns>An instance of <see cref="DefaultTypes"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="targetType"/> or <paramref name="defaultType"/> is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="defaultType"/> is not assignable to <paramref name="targetType"/>.</exception>
        public static DefaultTypes New(Type targetType, Type defaultType)
        {
            return new DefaultTypes().Add(targetType, defaultType);
        }

        /// <summary>
        /// Configures a default type for the member (property or constructor parameter) specified by the
        /// provided declaring type and member name.
        /// </summary>
        /// <param name="declaringType">The declaring type of a member that needs a default type.</param>
        /// <param name="memberName">The name of a member that needs a default type.</param>
        /// <param name="defaultType">The default type for the specified member.</param>
        /// <returns>This instance of <see cref="DefaultTypes"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="declaringType"/>, <paramref name="memberName"/>, or <paramref name="defaultType"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If there are no members of <paramref name="declaringType"/> that match <paramref name="memberName"/>, or
        /// if <paramref name="defaultType"/> is not assignable to any of the matching members.
        /// </exception>
        public DefaultTypes Add(Type declaringType, string memberName, Type defaultType)
        {
            if (declaringType == null) throw new ArgumentNullException(nameof(declaringType));
            if (memberName == null) throw new ArgumentNullException(nameof(memberName));
            if (defaultType == null) throw new ArgumentNullException(nameof(defaultType));

            if (defaultType.GetTypeInfo().IsAbstract) throw Exceptions.DefaultTypeCannotBeAbstract(defaultType);

            var matchingMembers = Members.Find(declaringType, memberName).ToList();

            if (matchingMembers.Count == 0) throw Exceptions.DefaultTypeHasNoMatchingMembers(declaringType, memberName);
            var notAssignableMembers = matchingMembers.Where(m => !m.Type.GetTypeInfo().IsAssignableFrom(defaultType)).ToList();
            if (notAssignableMembers.Count > 0) throw Exceptions.DefaultTypeNotAssignableToMembers(declaringType, memberName, defaultType, notAssignableMembers);

            _dictionary.Add(GetKey(declaringType, memberName), defaultType);
            return this;
        }

        /// <summary>
        /// Configures a default type for the specified target type.
        /// </summary>
        /// <param name="targetType">A type that needs a default type.</param>
        /// <param name="defaultType">The default type for the specified target type.</param>
        /// <returns>This instance of <see cref="DefaultTypes"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="targetType"/> or <paramref name="defaultType"/> is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="defaultType"/> is not assignable to <paramref name="targetType"/>.</exception>
        public DefaultTypes Add(Type targetType, Type defaultType)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (defaultType == null) throw new ArgumentNullException(nameof(defaultType));

            if (!targetType.GetTypeInfo().IsAssignableFrom(defaultType))
                throw Exceptions.DefaultTypeIsNotAssignableToTargetType(targetType, defaultType);

            _dictionary.Add(GetKey(targetType), defaultType);
            return this;
        }

        /// <summary>
        /// Attempt to get the default type for a member (property or construtor parameter) specified by its declaring type and name.
        /// </summary>
        /// <param name="declaringType">A declaring type of the member to find a default type for.</param>
        /// <param name="memberName">The name of the member to find a default type for.</param>
        /// <param name="defaultType">When a match is found for the member, contains its default type.</param>
        /// <returns>True, if a default type was found for the member. Otherwise, false if a default type could not be found.</returns>
        public bool TryGet(Type declaringType, string memberName, out Type defaultType) =>
            _dictionary.TryGetValue(GetKey(declaringType, memberName), out defaultType);

        /// <summary>
        /// Attempt to get the default type for a specified target type.
        /// </summary>
        /// <param name="targetType">The type to find a default type for.</param>
        /// <param name="defaultType">When a match is found for the target type, contains its default type.</param>
        /// <returns>True, if a default type was found for the target type. Otherwise, false if a default type could not be found.</returns>
        public bool TryGet(Type targetType, out Type defaultType) =>
            _dictionary.TryGetValue(GetKey(targetType), out defaultType);

        private static string GetKey(Type declaringType, string memberName) => 
            (declaringType != null && memberName != null) ? declaringType.FullName + "::" + memberName : "";

        private static string GetKey(Type targetType) => targetType != null ? targetType.FullName: "";
    }
}
