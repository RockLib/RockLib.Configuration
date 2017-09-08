using Microsoft.Extensions.Configuration;
using RockLib.Immutable;
using System;
using System.Collections.Generic;

namespace RockLib.Configuration
{
    /// <summary>
    /// Defines a central location to store an instance of <see cref="IConfigurationRoot"/>.
    /// </summary>
    public static class Config
    {
        private static readonly Semimutable<IConfigurationRoot> _root = new Semimutable<IConfigurationRoot>(() => GetDefaultRoot(null));

        /// <summary>
        /// Gets an object that retrieves settings from the "AppSettings" section of the
        /// <see cref="Root"/> property.
        /// </summary>
        public static AppSettings AppSettings => AppSettings.Instance;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Root"/> property is the default
        /// instance of <see cref="IConfigurationRoot"/>.
        /// </summary>
        public static bool IsDefault => _root.HasDefaultValue;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Root"/> property has been locked.
        /// <para>Tha value of this property is <c>false</c> before the <see cref="Root"/> property
        /// has been accessed and true after it has been accessed. When this property is true, any calls to the
        /// <see cref="SetRoot(IConfigurationRoot)"/>,
        /// <see cref="SetRoot(Func{IConfigurationRoot})"/>, or <see cref="ResetRoot"/>
        /// methods will result in an <see cref="InvalidOperationException"/>.</para>
        /// </summary>
        public static bool IsLocked => _root.IsLocked;

        /// <summary>
        /// Gets the <see cref="IConfigurationRoot"/> associated with the <see cref="Config"/> class.
        /// This property is guaranteed not to change.
        /// </summary>
        public static IConfigurationRoot Root => _root.Value;

        /// <summary>
        /// Sets the value of the <see cref="Root"/> property to the specified
        /// <see cref="IConfigurationRoot"/> instance.
        /// <para>NOTE: This method should only be called at the beginning of an application. Any calls to this method after
        /// the <see cref="Root"/> property has been accessed (i.e. when <see cref="IsLocked"/> is true) will
        /// result in an <see cref="InvalidOperationException"/> being thrown.</para>
        /// </summary>
        /// <param name="configurationRoot">
        /// The instance of <see cref="IConfigurationRoot"/> to be used as the <see cref="Root"/> property.
        /// </param>
        /// <exception cref="ArgumentNullException">If the <paramref name="configurationRoot"/> parameter is null.</exception>
        /// <exception cref="InvalidOperationException">If the <see cref="IsLocked"/> property is true.</exception>
        public static void SetRoot(IConfigurationRoot configurationRoot)
        {
            if (configurationRoot == null) throw new ArgumentNullException(nameof(configurationRoot));
            SetRoot(() => configurationRoot);
        }

        /// <summary>
        /// Sets the value of the <see cref="Root"/> property to the instance returned by the specified
        /// callback function. The callback function MUST NOT return null.
        /// <para>NOTE: This method should only be called at the beginning of an application. Any calls to this method after
        /// the <see cref="Root"/> property has been accessed (i.e. when <see cref="IsLocked"/> is true) will
        /// result in an <see cref="InvalidOperationException"/> being thrown.</para>
        /// </summary>
        /// <param name="getRoot">
        /// A function that returns the instance of <see cref="IConfigurationRoot"/> to be used as the <see cref="Root"/>
        /// property. This function MUST NOT return null.
        /// </param>
        /// <exception cref="ArgumentNullException">If the <paramref name="getRoot"/> parameter is null.</exception>
        /// <exception cref="InvalidOperationException">If the <see cref="IsLocked"/> property is true.</exception>
        public static void SetRoot(Func<IConfigurationRoot> getRoot)
        {
            if (getRoot == null) throw new ArgumentNullException(nameof(getRoot));
            _root.SetValue(getRoot);
        }

        /// <summary>
        /// Resets the value of the <see cref="Root"/> property to the default instance.
        /// <para>NOTE: This method should only be called at the beginning of an application. Any calls to this method after
        /// the <see cref="Root"/> property has been accessed (i.e. when <see cref="IsLocked"/> is true) will
        /// result in an <see cref="InvalidOperationException"/> being thrown.</para>
        /// </summary>
        /// <param name="additionalValues">When specified, these key/value pairs are applied to the resulting
        /// instance of <see cref="IConfigurationRoot"/>.</param>
        /// <exception cref="InvalidOperationException">If the <see cref="IsLocked"/> property is true.</exception>
        public static void ResetRoot(IEnumerable<KeyValuePair<string, string>> additionalValues = null)
        {
            if (additionalValues == null)
                _root.ResetValue();
            else
                SetRoot(() => GetDefaultRoot(additionalValues));
        }

        private static IConfigurationRoot GetDefaultRoot(IEnumerable<KeyValuePair<string, string>> additionalValues)
        {
            var builder = new ConfigurationBuilder();

#if NET451

            builder
                .AddConfigurationManager();
#endif
            builder
                .AddRockLib()
                .AddEnvironmentVariables()
                .AddInMemoryCollection(additionalValues ?? new List<KeyValuePair<string, string>>());
            
            var configurationRoot = builder.Build();
           
            return configurationRoot;
        }
    }
}
