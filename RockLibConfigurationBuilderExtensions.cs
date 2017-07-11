
using System.IO;
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
            var fullFilePath = Path.Combine(Directory.GetCurrentDirectory(), rockLibConfigJson);
            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException(rockLibConfigJson, $"Unable to use the configuration file at location {rockLibConfigJson} as it was not found. Please make sure you have included the configuration file in your project and that is being deployed at runtime.");
            }

            return builder
                .AddJsonFile(rockLibConfigJson, optional: true)
                .AddEnvironmentVariables("RockLib");
        }
    }
}
