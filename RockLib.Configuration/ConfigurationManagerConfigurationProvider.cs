#if NET451 || NET462
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Configuration;

namespace RockLib.Configuration
{
    public class ConfigurationManagerConfigurationProvider : ConfigurationProvider
    {
        private readonly FileSystemWatcher _watcher;

        public ConfigurationManagerConfigurationProvider(bool reloadOnChange)
        {
            OnChange(skipReload: true);

            if (reloadOnChange && Configuration?.FilePath != null)
            {
                _watcher = new FileSystemWatcher(Path.GetDirectoryName(Configuration.FilePath))
                {
                    NotifyFilter = NotifyFilters.LastWrite
                };

                _watcher.Changed += (s, e) => OnChange(skipReload: false);
                _watcher.EnableRaisingEvents = true;
            }
        }

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
            IDictionary<string, string> newSettings = new Dictionary<string, string>();

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
