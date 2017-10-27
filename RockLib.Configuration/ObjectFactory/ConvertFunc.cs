using System;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Defines a function that is used to convert a string value to a target type. If the function cannot
    /// convert the value, it should return null.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="declaringType">
    /// The type that declares the member (a property or constructor parameter) whose value will be set
    /// by the return value of this function. This value is null for top-level configuration values.
    /// </param>
    /// <param name="memberName">
    /// The name of the member (property or constructor parameter) whose value will be set by the return
    /// value of this function. This value is null for top-level configuration values.
    /// </param>
    /// <returns>The converted value, or null if the function cannot convert to the target type.</returns>
    public delegate object ConvertFunc(string value, Type targetType, Type declaringType, string memberName);
}
