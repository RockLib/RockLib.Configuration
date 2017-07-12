using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace System.Configuration
{
    internal sealed class AppSettingsSection
    {
        private readonly Func<IConfigurationRoot> _getConfigurationRoot;

        public AppSettingsSection(Func<IConfigurationRoot> getConfigurationRoot)
        {
            _getConfigurationRoot = getConfigurationRoot;
        }

        public string this[string key]
        {
            get
            {
                var value = _getConfigurationRoot().GetSection("AppSettings")[key];

                if (value == null)
                {
                    throw new KeyNotFoundException();
                }

                return value;
            }
        }
    }
}
