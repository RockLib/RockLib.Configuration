using System;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Defines the default type of a property, constructor parameter, class, or interface. The value of this attribute
    /// must be assignable to the member it decorates.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class DefaultTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTypeAttribute"/> type.
        /// </summary>
        /// <param name="value">The default type of the member that this attribute decorates.</param>
        public DefaultTypeAttribute(Type value) => Value = value ?? throw new ArgumentNullException(nameof(value));

        /// <summary>
        /// Gets the default type of the member that this attribute decorates.
        /// </summary>
        public Type Value { get; }
    }
}
