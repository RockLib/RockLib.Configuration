#if NET451 || NET462
using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration
{
    public class ConfigurationManagerConfigurationSource : IConfigurationSource
    {
        public ConfigurationManagerConfigurationSource(bool reloadOnChange) =>
            ReloadOnChange = reloadOnChange;

        public bool ReloadOnChange { get; }

        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new ConfigurationManagerConfigurationProvider(ReloadOnChange);
    }
}
#endif
