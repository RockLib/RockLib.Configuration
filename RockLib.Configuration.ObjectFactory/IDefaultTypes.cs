using System;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Defines an interface for retrieving default types by either declaring type and member name or by target type.
    /// </summary>
    internal interface IDefaultTypes
    {
        /// <summary>
        /// Attempt to get the default type for a member (property or construtor parameter) specified by its declaring type and name.
        /// </summary>
        /// <param name="declaringType">A declaring type of the member to find a default type for.</param>
        /// <param name="memberName">The name of the member to find a default type for.</param>
        /// <param name="defaultType">When a match is found for the member, contains its default type.</param>
        /// <returns>True, if a default type was found for the member. Otherwise, false if a default type could not be found.</returns>
        bool TryGet(Type declaringType, string memberName, out Type defaultType);

        /// <summary>
        /// Attempt to get the default type for a specified target type.
        /// </summary>
        /// <param name="targetType">The type to find a default type for.</param>
        /// <param name="defaultType">When a match is found for the target type, contains its default type.</param>
        /// <returns>True, if a default type was found for the target type. Otherwise, false if a default type could not be found.</returns>
        bool TryGet(Type targetType, out Type defaultType);
    }
}
