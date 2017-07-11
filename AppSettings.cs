
using Microsoft.Extensions.Configuration;
using System;

namespace RockLib.Configuration
{

    public class AppSettings
    {
        private readonly Func<IConfigurationRoot> _getConfigurationRoot;

        public AppSettings(Func<IConfigurationRoot> getConfigurationRoot)
        {
            _getConfigurationRoot = getConfigurationRoot;
        }

        public string this[string key] => _getConfigurationRoot()[key];
    }
}
