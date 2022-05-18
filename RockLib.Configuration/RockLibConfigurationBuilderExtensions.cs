﻿using System;
using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration
{
    /// <summary>
    /// Extension methods for adding configuration providers to an instance of <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public static class RockLibConfigurationBuilderExtensions
    {
        /// <summary>
        /// The default value for whether a configuration should be reloaded when its source changes.
        /// </summary>
        public const bool DefaultReloadOnChange = false;

        /// <summary>
        /// Sets the value of the <see cref="Config.Root"/> property by building the specified
        /// <see cref="IConfigurationBuilder"/>.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IConfigurationBuilder"/> that will be the source of
        /// the <see cref="Config.Root"/> property.
        /// </param>
        /// <returns>The <see cref="IConfigurationBuilder"/></returns>
        public static IConfigurationBuilder SetConfigRoot(this IConfigurationBuilder builder)
        {
            Config.SetRoot(() => builder.Build());
            return builder;
        }

        /// <summary>
        /// Adds the ASP.NET Core appsettings.json configuration provider to the builder using the configuration file "appsettings.json",
        /// relative to the base path stored in <see cref="IConfigurationBuilder.Properties"/> of the builder.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> is null.</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppSettingsJson(this IConfigurationBuilder builder) =>
            builder.AddAppSettingsJson(DefaultReloadOnChange);

        /// <summary>
        /// Adds the ASP.NET Core appsettings.json configuration provider to the builder using the configuration file "appsettings.json",
        /// relative to the base path stored in <see cref="IConfigurationBuilder.Properties"/> of the builder.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the appsettings.json file changes.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> is null.</exception>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAppSettingsJson(this IConfigurationBuilder builder, bool reloadOnChange)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            // we want the optional value to be true so that it will not throw a runtime exception if the file is not found
            builder = builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: reloadOnChange);

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrEmpty(environment))
                environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            if (string.IsNullOrEmpty(environment))
                environment = Environment.GetEnvironmentVariable("ROCKLIB_ENVIRONMENT");

            if (!string.IsNullOrEmpty(environment))
                builder = builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: reloadOnChange);

            return builder;
        }
    }
}
