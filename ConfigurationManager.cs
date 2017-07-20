using Microsoft.Extensions.Configuration;
using System;

namespace RockLib.Configuration
{
    public static class ConfigurationManager
    {
        private static readonly object _locker = new object();

        private static Func<IConfigurationRoot> _getConfigurationRoot;
        private static IConfigurationRoot _configurationRoot;

        static ConfigurationManager()
        {
            ResetConfigurationRoot();
        }

        public static AppSettings AppSettings => AppSettings.Instance;

        public static bool IsDefault { get; private set; } = true;

        public static IConfigurationRoot ConfigurationRoot
        {
            get
            {
                if (_configurationRoot == null)
                {
                    lock (_locker)
                    {
                        if (_configurationRoot == null)
                        {
                            _configurationRoot = _getConfigurationRoot();
                            if (_configurationRoot == null)
                                throw new InvalidOperationException("A null value was returned from the Func<IConfigurationRoot> factory method.");
                            _getConfigurationRoot = null;
                        }
                    }
                }
                return _configurationRoot;
            }
        }

        public static void SetConfigurationRoot(IConfigurationRoot configurationRoot)
        {
            if (configurationRoot == null) throw new ArgumentNullException(nameof(configurationRoot));
            SetConfigurationRoot(() => configurationRoot);
        }

        public static void SetConfigurationRoot(Func<IConfigurationRoot> getConfigurationRoot)
        {
            if (getConfigurationRoot == null) throw new ArgumentNullException(nameof(getConfigurationRoot));
            SetConfigurationRoot(getConfigurationRoot, false);
        }

        public static void ResetConfigurationRoot()
        {
            SetConfigurationRoot(GetDefaultConfigurationRoot, true);
        }

        private static void SetConfigurationRoot(Func<IConfigurationRoot> getConfigurationRoot, bool isDefault)
        {
            if (getConfigurationRoot == null) throw new ArgumentNullException(nameof(getConfigurationRoot));

            if (_configurationRoot == null)
            {
                lock (_locker)
                {
                    if (_configurationRoot == null)
                    {
                        _getConfigurationRoot = getConfigurationRoot;
                        IsDefault = isDefault;
                        return;
                    }
                }
            }

            throw new InvalidOperationException($"{nameof(ConfigurationManager)}.{nameof(ConfigurationRoot)} has been locked. Its value cannot be changed after its value has been read.");
        }

        private static IConfigurationRoot GetDefaultConfigurationRoot()
        {
            var configurationRoot = new ConfigurationBuilder()
                .AddRockLib()
                .AddEnvironmentVariables()
                .Build();

            return configurationRoot;
        }
    }
}
