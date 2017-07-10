using System;
using System.Collections.Generic;
using System.Text;

namespace RockLib.Configuration
{

    public class AppSettings
    {
        private readonly IConfigurationRoot _configuration;

        public AppSettings(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public string this[string key]
        {
            get
            {
                return _configuration[key];
            }
        }
    }
}
