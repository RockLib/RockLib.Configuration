using System;
using System.Collections.Generic;

namespace RockLib.Configuration
{
    public sealed class AppSettings
    {
        private AppSettings() { }

        internal static AppSettings Instance { get; } = new AppSettings();

        public string this[string key] => Config.Root["AppSettings:" + key] ?? throw GetKeyNotFoundExeption(key);

        private static Exception GetKeyNotFoundExeption(string key) =>
            new KeyNotFoundException($"Unable to locate {nameof(Config.AppSettings)} key '{key}' in {typeof(Config).FullName}.{nameof(Config.Root)}.");
    }
}
