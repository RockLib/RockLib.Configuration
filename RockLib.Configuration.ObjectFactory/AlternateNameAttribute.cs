using System;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Defines an alternate name for a constructor parameter or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = true)]
    public class AlternateNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlternateNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The alternate name.</param>
        public AlternateNameAttribute(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

        /// <summary>
        /// Gets the alternate name.
        /// </summary>
        public string Name { get; }
    }
}
