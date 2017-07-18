using System;
using System.Collections.Generic;

namespace RockLib.Configuration
{
    public sealed class AppSettings
    {
        private AppSettings() { }

        internal static AppSettings Instance { get; } = new AppSettings();

        public string this[string key] => ConfigurationManager.ConfigurationRoot["AppSettings:" + key] ?? throw GetKeyNotFoundExeption(key);

        private static Exception GetKeyNotFoundExeption(string key) =>
            new KeyNotFoundException($"Unable to locate {nameof(ConfigurationManager.AppSettings)} key '{key}' in {typeof(ConfigurationManager).FullName}.{nameof(ConfigurationManager.ConfigurationRoot)}.");
    }
}
