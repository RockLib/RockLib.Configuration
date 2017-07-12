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

        public static IConfigurationRoot ConfigurationRoot
        {
            get { return _configurationRoot.Value; }
            set
            {
                if (value == null) _configurationRoot = new Lazy<IConfigurationRoot>(GetDefaultConfigurationRoot);
                else _configurationRoot = new Lazy<IConfigurationRoot>(() => value);
            }
        }

        public static AppSettingsSection AppSettings { get; } = new AppSettingsSection(() => ConfigurationRoot);
        public static ConnectionStringsSection ConnectionStrings { get; } = new ConnectionStringsSection(() => ConfigurationRoot);

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
