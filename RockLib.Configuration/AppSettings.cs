using System;
using System.Collections.Generic;

namespace RockLib.Configuration
{
    /// <summary>
    /// Defines an indexer property that retrieves settings from the "AppSettings" section from
    /// <see cref="Config.Root"/>.
    /// </summary>
    public sealed class AppSettings
    {
        private AppSettings() { }

        internal static AppSettings Instance { get; } = new AppSettings();

        /// <summary>
        /// Gets the setting associated with the <paramref name="key"/> parameter.
        /// </summary>
        /// <param name="key">The key of the setting to look up.</param>
        /// <returns>The setting associated with the <paramref name="key"/> parameter.</returns>
        /// <exception cref="KeyNotFoundException">
        /// If the given key is not found in the "AppSettings" section of <see cref="Config.Root"/>.
        /// </exception>
        public string this[string key] => Config.Root[$"AppSettings:{key}"] ?? throw GetKeyNotFoundExeption(key);

        private static Exception GetKeyNotFoundExeption(string key) =>
            new KeyNotFoundException($"Unable to locate {nameof(Config.AppSettings)} key '{key}' in {typeof(Config).FullName}.{nameof(Config.Root)}.");
    }
}
