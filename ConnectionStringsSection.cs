using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace System.Configuration
{
    internal sealed class ConnectionStringsSection
    {
        private readonly Func<IConfigurationRoot> _getConfigurationRoot;

        public ConnectionStringsSection(Func<IConfigurationRoot> getConfigurationRoot)
        {
            _getConfigurationRoot = getConfigurationRoot;
        }

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
