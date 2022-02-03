using System;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Defines which method should be used to convert a configuration string value to a target type. The method
    /// must be static, have a single parameter of type string, and return a type (other than <see cref="object"/>)
    /// that is assignable to the member that this attribute decorates.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class ConvertMethodAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertMethodAttribute"/> class.
        /// </summary>
        /// <param name="convertMethodName">
        /// The name of the method that does the conversion for the member that this attribute decorates.
        /// </param>
        public ConvertMethodAttribute(string convertMethodName) => ConvertMethodName = convertMethodName ?? throw new ArgumentNullException(nameof(convertMethodName));

        /// <summary>
        /// Get the name of the method that does the conversion for the member that this attribute decorates.
        /// </summary>
        public string ConvertMethodName { get; }
    }
}
