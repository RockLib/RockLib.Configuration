#if NET451 || NET462
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web.Configuration;

namespace RockLib.Configuration
{
    /// <summary>
    /// A <see cref="ConfigurationProvider"/> backed by the <see cref="ConfigurationManager"/> class.
    /// </summary>
    public class ConfigurationManagerConfigurationProvider : ConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationManagerConfigurationProvider"/> class.
        /// </summary>
        /// <param name="source">The source settings.</param>
        public ConfigurationManagerConfigurationProvider(ConfigurationManagerConfigurationSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));

            if (Source.ReloadOnChange && Source.FileProvider != null)
            {
                ChangeToken.OnChange(() => Source.FileProvider.Watch("*.config"), delegate
                {
                    Thread.Sleep(Source.ReloadDelay);
                    OnChange(skipReload: false);
                });
            }

            OnChange(skipReload: true);
        }

        /// <summary>
        /// Gets the source settings for this provider.
        /// </summary>
        public ConfigurationManagerConfigurationSource Source { get; }

        private void OnChange(bool skipReload)
        {
            var newSettings = GetNewSettings(skipReload);

            if (HasAnyChanges(newSettings))
            {
                Data = newSettings;

                if (!skipReload)
                    OnReload();
            }
        }

        private IDictionary<string, string> GetNewSettings(bool skipReload)
        {
            IDictionary<string, string> newSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                foreach (var key in ConfigurationManager.AppSettings.AllKeys)
                    newSettings[$"AppSettings:{key}"] = ConfigurationManager.AppSettings[key];
            }
            catch
            {
            }

            foreach (var sectionName in SectionNames)
            {
                try
                {
                    if (!skipReload)
                        ConfigurationManager.RefreshSection(sectionName);
                    var section = (RockLibConfigurationSection)ConfigurationManager.GetSection(sectionName);
                    foreach (var setting in section.Settings)
                        newSettings.Add(setting);
                }
                catch
                {
                }
            }

            return newSettings;
        }

        private bool HasAnyChanges(IDictionary<string, string> newSettings)
        {
            if (newSettings.Count != Data.Count)
                return true;

            foreach (var newSetting in newSettings)
            {
                if (Data.TryGetValue(newSetting.Key, out var value))
                {
                    if (newSetting.Value != value)
                        return true; // same key, different value
                    continue; // same key, same value
                }

                return true; // new key/value pair
            }

            return false;
        }

        private IEnumerable<string> SectionNames =>
            Configuration?.Sections?.OfType<RockLibConfigurationSection>().Select(s => s.SectionInformation.Name)
            ?? Enumerable.Empty<string>();

        private static System.Configuration.Configuration Configuration
        {
            get
            {
                try
                {
                    return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                }
                catch
                {
                    try
                    {
                        return WebConfigurationManager.OpenWebConfiguration("~");
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }
    }
}
#endif
