using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RockLib.Configuration
{
    /// <summary>
    /// Defines extension methods for obtaining composite configuration sections.
    /// That is, a configuration section composed of the settings contained with
    /// multiple sections at different keys within a single configuration root.
    /// </summary>
    public static class CompositeConfigurationExtensions
    {
        /// <summary>
        /// Gets a composite configuration sub-section with the specified keys.
        /// </summary>
        /// <param name="configuration">The source of the composite section.</param>
        /// <param name="keys">The keys of the configuration sections.</param>
        /// <returns>The <see cref="IConfigurationSection"/>.</returns>
        public static IConfigurationSection GetCompositeSection(this IConfiguration configuration, IEnumerable<string> keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            return configuration.GetCompositeSection(keys.ToArray());
        }

        /// <summary>
        /// Gets a composite configuration sub-section with the specified keys.
        /// </summary>
        /// <param name="configuration">The source of the composite section.</param>
        /// <param name="keys">The keys of the configuration sections.</param>
        /// <returns>The <see cref="IConfigurationSection"/>.</returns>
        public static IConfigurationSection GetCompositeSection(this IConfiguration configuration, params string[] keys)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (keys.Length == 0)
            {
                throw new ArgumentException("Must contain at least one key.", nameof(keys));
            }

            if (keys.Any(key => string.IsNullOrEmpty(key)))
            {
                throw new ArgumentException("Cannot contain null or empty keys.", nameof(keys));
            }

            return new CompositeConfigurationSection(keys.Select(key => configuration.GetSection(key)));
        }
    }
}
