
using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration
{
    public static class RockLibConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddRockLib(this IConfigurationBuilder builder)
        {
            return builder.AddRockLib("rocklib.config.json");
        }

        public static IConfigurationBuilder AddRockLib(this IConfigurationBuilder builder, string rockLibConfigJson)
        {
            return builder
                .AddJsonFile(rockLibConfigJson, optional: true)
                .AddEnvironmentVariables("RockLib");
        }
    }
}
