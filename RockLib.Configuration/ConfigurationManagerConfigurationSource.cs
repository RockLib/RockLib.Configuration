#if NET451 || NET462
using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration
{
    public class ConfigurationManagerConfigurationSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new ConfigurationManagerConfigurationProvider(this);
        }
    }
}
#endif
