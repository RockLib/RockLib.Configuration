#if NET451 || NET462
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace RockLib.Configuration
{
    /// <summary>
    /// Represents the values of the <see cref="ConfigurationManager"/> class as an
    /// <see cref="IConfigurationProvider"/>.
    /// </summary>
    public class ConfigurationManagerConfigurationSource : FileConfigurationSource
    {
        /// <summary>
        /// Builds the <see cref="ConfigurationManagerConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="ConfigurationManagerConfigurationProvider"/>.</returns>
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new ConfigurationManagerConfigurationProvider(this);
        }
    }
}
#endif
