using System;
using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration
{
    public static class RockLibConfigurationBuilderExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddRockLib(this IConfigurationBuilder builder)
        {
            return builder.AddRockLib("rocklib.config.json");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder">Non-null instance of an IConfigurationBuilder</param>
        /// <param name="rockLibConfigJson">Required value which provides the name of the file to pull the configuration values from</param>
        /// <exception cref="NullReferenceException">Will be thrown if the value for rockLibConfigJson is null or empty</exception>
        /// <exception cref="FileNotFoundException">Will be thrown if the provided file name is not found in the runtime folder</exception>
        /// <returns>A built instance of IConfigurationbuilder</returns>
        public static IConfigurationBuilder AddRockLib(this IConfigurationBuilder builder, string rockLibConfigJson)
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
