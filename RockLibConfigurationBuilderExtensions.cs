using System;
using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration
{
    public static class RockLibConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds the RockLib configuration provider to the builder using the configuration file "rocklib.config.json",
        /// relative to the base path stored in <see cref="IConfigurationBuilder.Properties"/> of the builder.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="jsonConfigPath"/> is null or empty.</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddRockLib(this IConfigurationBuilder builder)
        {
            return builder.AddRockLib("rocklib.config.json");
        }

        /// <summary>
        /// Adds the RockLib configuration provider to the builder using the specified configuration file,
        /// relative to the base path stored in <see cref="IConfigurationBuilder.Properties"/> of the builder.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="jsonConfigPath">Required value which provides the name of the file to pull the configuration values from</param>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="jsonConfigPath"/> is null or empty.</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddRockLib(this IConfigurationBuilder builder, string jsonConfigPath)
        {
            if (string.IsNullOrEmpty(jsonConfigPath)) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrEmpty(jsonConfigPath)) throw new ArgumentException($"'{nameof(jsonConfigPath)}' must be a non-empty string.", nameof(jsonConfigPath));

            // we want the optional value to be false so that it will throw a runtime exception if the file is not found
            // if this is set to true no exception is throw and no config values are found/returned.
            var builtBuilder = builder
                .AddJsonFile(jsonConfigPath, optional: false);

            return builtBuilder;
        }
    }
}
