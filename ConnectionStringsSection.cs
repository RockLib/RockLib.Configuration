using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace System.Configuration
{
    /// <summary>
    /// Provides configuration system support for the ConnectionStrings configuration section. This class cannot be inherited.
    /// </summary>
    internal sealed class ConnectionStringsSection
    {
        private readonly Func<IConfigurationRoot> _getConfigurationRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStringsSection"/> class.
        /// </summary>
        /// <param name="getConfigurationRoot">
        /// A function that returns the <see cref="IConfigurationRoot"/> used by this instance.
        /// </param>
        public ConnectionStringsSection(Func<IConfigurationRoot> getConfigurationRoot)
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
                var value = _getConfigurationRoot().GetSection("ConnectionStrings")[key];

                if (value == null)
                {
                    throw new KeyNotFoundException();
                }

                return value;
            }
        }
    }
}
