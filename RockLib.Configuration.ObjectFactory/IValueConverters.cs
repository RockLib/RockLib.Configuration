using System;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Defines an interface for retrieving convert functions by either declaring type and member name or by
    /// target type.
    /// </summary>
    internal interface IValueConverters
    {
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
        bool TryGet(Type declaringType, string memberName, out Func<string, object> converter);

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
        bool TryGet(Type targetType, out Func<string, object> converter);
    }
}
