using Microsoft.Extensions.Configuration;
using System;

namespace RockLib.Configuration
{
    public static class Config
    {
        private static readonly object _locker = new object();

        private static Func<IConfigurationRoot> _getRoot;
        private static IConfigurationRoot _root;

        static Config()
        {
            ResetRootToDefault();
        }

        public static AppSettings AppSettings => AppSettings.Instance;

        public static bool IsDefault { get; private set; } = true;

        public static IConfigurationRoot Root
        {
            get
            {
                if (_root == null)
                {
                    lock (_locker)
                    {
                        if (_root == null)
                        {
                            _root = _getRoot();
                        }
                    }
                }
                return _root;
            }
        }

        public static void SetRoot(IConfigurationRoot root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            SetRoot(() => root);
        }

        public static void SetRoot(Func<IConfigurationRoot> getRoot)
        {
            if (getRoot == null) throw new ArgumentNullException(nameof(getRoot));
            SetRoot(getRoot, false);
        }

        public static void ResetRootToDefault()
        {
            SetRoot(GetDefaultConfigurationRoot, true);
        }

        private static void SetRoot(Func<IConfigurationRoot> getRoot, bool isDefault)
        {
            if (getRoot == null) throw new ArgumentNullException(nameof(getRoot));

            if (_root == null)
            {
                lock (_locker)
                {
                    if (_root == null)
                    {
                        _getRoot = getRoot;
                        IsDefault = isDefault;
                        return;
                    }
                }
            }

            throw new InvalidOperationException($"{nameof(Config)}.{nameof(Root)} has been locked. Its value cannot be changed after its value has been read.");
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
