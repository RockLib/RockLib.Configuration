using System;
using Microsoft.Extensions.Configuration;

#if NET451 || NET462
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
#endif

namespace RockLib.Configuration
{
    /// <summary>
    /// Extension methods for adding RockLib's standard JSON config file to a <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public static class RockLibConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds the ASP.NET Core appsettings.json configuration provider to the builder using the configuration file "appsettings.json",
        /// relative to the base path stored in <see cref="IConfigurationBuilder.Properties"/> of the builder.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> is null.</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppSettingsJson(this IConfigurationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            // we want the optional value to be true so that it will not throw a runtime exception if the file is not found
            builder = builder.AddJsonFile("appsettings.json", optional: true);

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrEmpty(environment))
                environment = Environment.GetEnvironmentVariable("ROCKLIB_ENVIRONMENT");

            if (!string.IsNullOrEmpty(environment))
                builder = builder.AddJsonFile($"appsettings.{environment.ToLower()}.json", optional: true);

            return builder;
        }

#if NET451 || NET462
        /// <summary>
        /// Adds support for .Net Framework applications to pull in App or Web.config AppSettings values.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder AddConfigurationManager(this IConfigurationBuilder builder)
        {
            var settings = new Dictionary<string, string>();

            try
            {
                foreach (var key in ConfigurationManager.AppSettings.AllKeys)
                    settings[$"AppSettings:{key}"] = ConfigurationManager.AppSettings[key];
            }
            catch
            {
            }

            try
            {
                var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (configuration != null && configuration.Sections != null)
                    foreach (var setting in configuration.Sections.OfType<RockLibConfigurationSection>().SelectMany(x => x.Settings))
                        settings[setting.Key] = setting.Value;
            }
            catch
            {
            }

            return builder.AddInMemoryCollection(settings);
        }
#endif
    }
}
