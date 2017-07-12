using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace System.Configuration
{
    /// <summary>
    /// Provides configuration system support for the AppSettings configuration section. This class cannot be inherited.
    /// </summary>
    internal sealed class AppSettingsSection
    {
        private readonly Func<IConfigurationRoot> _getConfigurationRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsSection"/> class.
        /// </summary>
        /// <param name="getConfigurationRoot">
        /// A function that returns the <see cref="IConfigurationRoot"/> used by this instance.
        /// </param>
        public AppSettingsSection(Func<IConfigurationRoot> getConfigurationRoot)
        {
            _getConfigurationRoot = getConfigurationRoot;
        }

        /// <summary>
        /// Gets a configuration value.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <value>The configuration value.</value>
        public string this[string key]
        {
            get
            {
                var value = _getConfigurationRoot().GetSection("AppSettings")[key];

                if (value == null)
                {
                    throw new KeyNotFoundException($"The given key, '{key}', was not present in the configuration's 'AppSettings' section.");
                }

                return value;
            }
        }
    }
}
