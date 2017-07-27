using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace RockLib.Configuration
{
    /// <summary>
    /// Defines a central location to store an instance of <see cref="IConfigurationRoot"/>.
    /// </summary>
    public static class ConfigurationManager
    {
        private static readonly object _locker = new object();

        private static Func<IConfigurationRoot> _getConfigurationRoot;
        private static IConfigurationRoot _configurationRoot;

        static ConfigurationManager()
        {
            ResetConfigurationRoot();
        }

        /// <summary>
        /// Gets an object that retrieves settings from the "AppSettings" section of the
        /// <see cref="ConfigurationRoot"/> property.
        /// </summary>
        public static AppSettings AppSettings => AppSettings.Instance;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ConfigurationRoot"/> property is the default
        /// instance of <see cref="IConfigurationRoot"/>.
        /// </summary>
        public static bool IsDefault { get; private set; } = true;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ConfigurationRoot"/> property has been locked.
        /// <para>Tha value of this property is <c>false</c> before the <see cref="ConfigurationRoot"/> property
        /// has been accessed and true after it has been accessed. When this property is true, any calls to the
        /// <see cref="SetConfigurationRoot(IConfigurationRoot)"/>,
        /// <see cref="SetConfigurationRoot(Func{IConfigurationRoot})"/>, or <see cref="ResetConfigurationRoot"/>
        /// methods will result in an <see cref="InvalidOperationException"/>.</para>
        /// </summary>
        public static bool IsLocked => _configurationRoot != null;

        /// <summary>
        /// Gets the <see cref="IConfigurationRoot"/> associated with the <see cref="ConfigurationManager"/> class.
        /// This property is guaranteed not to change.
        /// </summary>
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

        /// <summary>
        /// Sets the value of the <see cref="ConfigurationRoot"/> property to the specified
        /// <see cref="IConfigurationRoot"/> instance.
        /// <para>NOTE: This method should only be called at the beginning of an application. Any calls to this method after
        /// the <see cref="ConfigurationRoot"/> property has been accessed (i.e. when <see cref="IsLocked"/> is true) will
        /// result in an <see cref="InvalidOperationException"/> being thrown.</para>
        /// </summary>
        /// <param name="configurationRoot">
        /// The instance of <see cref="IConfigurationRoot"/> to be used as the <see cref="ConfigurationRoot"/> property.
        /// </param>
        /// <exception cref="ArgumentNullException">If the <paramref name="configurationRoot"/> parameter is null.</exception>
        /// <exception cref="InvalidOperationException">If the <see cref="IsLocked"/> property is true.</exception>
        public static void SetConfigurationRoot(IConfigurationRoot configurationRoot)
        {
            if (configurationRoot == null) throw new ArgumentNullException(nameof(configurationRoot));
            SetConfigurationRoot(() => configurationRoot);
        }

        /// <summary>
        /// Sets the value of the <see cref="ConfigurationRoot"/> property to the instance returned by the specified
        /// callback function. The callback function MUST NOT return null.
        /// <para>NOTE: This method should only be called at the beginning of an application. Any calls to this method after
        /// the <see cref="ConfigurationRoot"/> property has been accessed (i.e. when <see cref="IsLocked"/> is true) will
        /// result in an <see cref="InvalidOperationException"/> being thrown.</para>
        /// </summary>
        /// <param name="getConfigurationRoot">
        /// A function that returns the instance of <see cref="IConfigurationRoot"/> to be used as the <see cref="ConfigurationRoot"/>
        /// property. This function MUST NOT return null.
        /// </param>
        /// <exception cref="ArgumentNullException">If the <paramref name="getConfigurationRoot"/> parameter is null.</exception>
        /// <exception cref="InvalidOperationException">If the <see cref="IsLocked"/> property is true.</exception>
        public static void SetConfigurationRoot(Func<IConfigurationRoot> getConfigurationRoot)
        {
            if (getConfigurationRoot == null) throw new ArgumentNullException(nameof(getConfigurationRoot));
            SetConfigurationRoot(getConfigurationRoot, false);
        }

        /// <summary>
        /// Resets the value of the <see cref="ConfigurationRoot"/> property to the default instance.
        /// <para>NOTE: This method should only be called at the beginning of an application. Any calls to this method after
        /// the <see cref="ConfigurationRoot"/> property has been accessed (i.e. when <see cref="IsLocked"/> is true) will
        /// result in an <see cref="InvalidOperationException"/> being thrown.</para>
        /// </summary>
        /// <param name="additionalValues">When specified, these key/value pairs are applied to the resulting
        /// instance of <see cref="IConfigurationRoot"/>.</param>
        /// <exception cref="InvalidOperationException">If the <see cref="IsLocked"/> property is true.</exception>
        public static void ResetConfigurationRoot(IEnumerable<KeyValuePair<string, string>> additionalValues = null)
        {
            SetConfigurationRoot(() => GetDefaultConfigurationRoot(additionalValues), additionalValues == null);
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

        private static IConfigurationRoot GetDefaultConfigurationRoot(IEnumerable<KeyValuePair<string, string>> additionalValues)
        {
            var builder = new ConfigurationBuilder().AddRockLib();

            if (additionalValues != null)
                builder = builder.AddInMemoryCollection(additionalValues);

            builder = builder.AddEnvironmentVariables();

            var configurationRoot = builder.Build();
            return configurationRoot;
        }
    }
}
