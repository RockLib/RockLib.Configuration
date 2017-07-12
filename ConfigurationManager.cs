using Microsoft.Extensions.Configuration;
using RockLib.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace System.Configuration
{
    /// <summary>
    /// Provides access to configuration files for client applications. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// The intent of our copy of ConfigurationManager is to mimic that of the existing/legacy configuration manager in .net 45+
    ///     The idea is that someone consuming our library should be able to drop this in and have it 'just work'
    /// 
    /// Api for legacy system
    /// https://msdn.microsoft.com/en-us/library/system.configuration.configurationmanager(v=vs.110).aspx
    /// </remarks>
    internal static class ConfigurationManager
    {
        private static Lazy<IConfigurationRoot> _configurationRoot = new Lazy<IConfigurationRoot>(GetDefaultConfigurationRoot);

        /// <summary>
        /// Gets or sets the <see cref="IConfigurationRoot"/> that is the backing store for the other public
        /// members of the <see cref="ConfigurationManager"/> class.
        /// <para>NOTE: If the value is set to null, then the default <see cref="IConfigurationRoot"/>
        /// of the <see cref="ConfigurationManager"/> class is used as the value instead.</para>
        /// </summary>
        public static IConfigurationRoot ConfigurationRoot
        {
            get { return _configurationRoot.Value; }
            set
            {
                if (value == null) _configurationRoot = new Lazy<IConfigurationRoot>(GetDefaultConfigurationRoot);
                else _configurationRoot = new Lazy<IConfigurationRoot>(() => value);
            }
        }

        /// <summary>
        /// Gets the <see cref="AppSettingsSection"/> data for the current application's default configuration.
        /// </summary>
        public static AppSettingsSection AppSettings { get; } = new AppSettingsSection(() => ConfigurationRoot);

        /// <summary>
        /// Gets the <see cref="ConnectionStringsSection"/> data for the current application's default configuration.
        /// </summary>
        public static ConnectionStringsSection ConnectionStrings { get; } = new ConnectionStringsSection(() => ConfigurationRoot);

        /// <summary>
        /// Retrieves a specified configuration section for the current application's default configuration.
        /// </summary>
        /// <param name="sectionName">The configuration section path and name.</param>
        /// <returns>The specified ConfigurationSection object.</returns>
        /// <exception cref="KeyNotFoundException">If the section does not exist.</exception>
        public static dynamic GetSection(string sectionName)
        {
            var section = ConfigurationRoot.GetSection(sectionName);
            if (section.Value == null && !section.GetChildren().Any()) throw new KeyNotFoundException();
            return new ConvertibleConfigurationSection(section);
        }

        private static IConfigurationRoot GetDefaultConfigurationRoot()
        {
            var configurationRoot = new ConfigurationBuilder()
                .AddRockLib()
                .AddEnvironmentVariables()
                .Build();

            return configurationRoot;
        }
    }
}
