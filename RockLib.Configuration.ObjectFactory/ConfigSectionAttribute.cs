using System;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Associates the path to a configuration section with a target type. The contents
    /// of such a configuration section should declare an object of the target type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ConfigSectionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSectionAttribute"/> class,
        /// associating the specified path to a configuration section with the specified type.
        /// The contents of such a configuration section should declare an object of the target type.
        /// </summary>
        /// <param name="path">
        /// The path to a target configuration section. The contents of such a configuration
        /// section should declare an object of the type of the <paramref name="type"/> parameter.
        /// </param>
        /// <param name="type">
        /// The type of object that should be able to be created using a configuration section
        /// specified by the <paramref name="path"/> parameter.
        /// </param>
        public ConfigSectionAttribute(string path, Type type)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Gets the path to a target configuration section. The contents of such a configuration
        /// section should declare an object of the type of the <see cref="Type"/> property.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the type of object that should be able to be created using a configuration section
        /// specified by the <see cref="Path"/> property.
        /// </summary>
        public Type Type { get; }
    }
}
