using Microsoft.Extensions.Configuration;
using RockLib.Immutable;
using System;
using System.Collections.Generic;
using System.IO;

namespace RockLib.Configuration
{
    /// <summary>
    /// Defines a central location to store an instance of <see cref="IConfiguration"/>.
    /// </summary>
    public static class Config
    {
        private static readonly Semimutable<IConfiguration> _root = new Semimutable<IConfiguration>(() => GetDefaultRoot(null));

        private static string? _basePath;

        /// <summary>
        /// Gets an object that retrieves settings from the "AppSettings" section of the
        /// <see cref="Root"/> property.
        /// </summary>
        public static AppSettings AppSettings => AppSettings.Instance;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Root"/> property is the default
        /// instance of <see cref="IConfiguration"/>.
        /// </summary>
        public static bool IsDefault => _root.HasDefaultValue;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Root"/> property has been locked.
        /// <para>Tha value of this property is <c>false</c> before the <see cref="Root"/> property
        /// has been accessed and true after it has been accessed. When this property is true, any calls to the
        /// <see cref="SetRoot(IConfiguration)"/>,
        /// <see cref="SetRoot(Func{IConfiguration})"/>, or <see cref="ResetRoot"/>
        /// methods will result in an <see cref="InvalidOperationException"/>.</para>
        /// </summary>
        public static bool IsLocked => _root.IsLocked;

        /// <summary>
        /// Gets the <see cref="IConfiguration"/> associated with the <see cref="Config"/> class.
        /// This property is guaranteed not to change.
        /// </summary>
        public static IConfiguration? Root => _root.Value;

        /// <summary>
        /// Sets the value of the <see cref="Root"/> property to the specified
        /// <see cref="IConfiguration"/> instance.
        /// <para>NOTE: This method should only be called at the beginning of an application. Any calls to this method after
        /// the <see cref="Root"/> property has been accessed (i.e. when <see cref="IsLocked"/> is true) will
        /// result in an <see cref="InvalidOperationException"/> being thrown.</para>
        /// </summary>
        /// <param name="configurationRoot">
        /// The instance of <see cref="IConfiguration"/> to be used as the <see cref="Root"/> property.
        /// </param>
        /// <exception cref="ArgumentNullException">If the <paramref name="configurationRoot"/> parameter is null.</exception>
        /// <exception cref="InvalidOperationException">If the <see cref="IsLocked"/> property is true.</exception>
        public static void SetRoot(IConfiguration configurationRoot)
        {
            if (configurationRoot is null) throw new ArgumentNullException(nameof(configurationRoot));
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
        /// A function that returns the instance of <see cref="IConfiguration"/> to be used as the <see cref="Root"/>
        /// property. This function MUST NOT return null.
        /// </param>
        /// <exception cref="ArgumentNullException">If the <paramref name="getRoot"/> parameter is null.</exception>
        /// <exception cref="InvalidOperationException">If the <see cref="IsLocked"/> property is true.</exception>
        public static void SetRoot(Func<IConfiguration> getRoot)
        {
            if (getRoot is null) throw new ArgumentNullException(nameof(getRoot));
            _root.SetValue(getRoot);
        }

        /// <summary>
        /// Resets the value of the <see cref="Root"/> property to the default instance.
        /// <para>NOTE: This method should only be called at the beginning of an application. Any calls to this method after
        /// the <see cref="Root"/> property has been accessed (i.e. when <see cref="IsLocked"/> is true) will
        /// result in an <see cref="InvalidOperationException"/> being thrown.</para>
        /// </summary>
        /// <param name="additionalValues">When specified, these key/value pairs are applied to the resulting
        /// instance of <see cref="IConfiguration"/>.</param>
        /// <exception cref="InvalidOperationException">If the <see cref="IsLocked"/> property is true.</exception>
        public static void ResetRoot(IEnumerable<KeyValuePair<string, string>>? additionalValues = null)
        {
            if (additionalValues is null)
            {
                _root.ResetValue();
            }
            else
            {
                SetRoot(() => GetDefaultRoot(additionalValues));
            }
        }

        /// <summary>
        /// When the <c>SetRoot</c> method is <em>not</em> called (i.e. the default root is used),
        /// sets the base path of the <see cref="ConfigurationBuilder"/> to the specified value.
        /// If <paramref name="basePath"/> is null, then the value returned by the <see cref=
        /// "Directory.GetCurrentDirectory"/> method is used as the base path.
        /// </summary>
        /// <param name="basePath">
        /// The base path for the <see cref="ConfigurationBuilder"/>, or null to use the value
        /// returned by the <see cref="Directory.GetCurrentDirectory"/> method.
        /// </param>
        public static void SetBasePath(string? basePath = null)
        {
            _basePath = basePath ?? Directory.GetCurrentDirectory();
        }

        private static IConfiguration GetDefaultRoot(IEnumerable<KeyValuePair<string, string>>? additionalValues)
        {
            var builder = new ConfigurationBuilder();

            if (_basePath is not null)
            {
                builder.SetBasePath(_basePath);
            }

            builder
                .AddAppSettingsJson(reloadOnChange: true)
                .AddEnvironmentVariables();

            if (additionalValues is not null)
            {
                builder.AddInMemoryCollection(additionalValues);
            }

            builder.AddRockLibSecrets();

            return builder.Build();
        }

        private static IConfigurationBuilder AddRockLibSecrets(this IConfigurationBuilder builder)
        {
            const string extensionTypeName = "RockLib.Secrets.ConfigurationBuilderExtensions, RockLib.Secrets";
            
            var extensionType = Type.GetType(extensionTypeName);

            if (extensionType is not null)
            {
                var addRockLibSecretsMethod = extensionType.GetMethod("AddRockLibSecrets",
                    new Type[] { typeof(IConfigurationBuilder) });

                if (addRockLibSecretsMethod is not null)
                {
                    addRockLibSecretsMethod.Invoke(null, new object[] { builder });
                }
            }

            return builder;
        }
    }
}
