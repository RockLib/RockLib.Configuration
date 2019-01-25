using System.Collections.Generic;

namespace RockLib.Configuration.MessagingProvider
{
    internal static class ListSettingFilterExtensions
    {
        internal static bool HasSetting(this HashSet<string> settings, string setting)
        {
            foreach (var key in SelfAndAncestors(setting))
                if (settings.Contains(key))
                    return true;
            return false;
        }

        private static IEnumerable<string> SelfAndAncestors(string setting)
        {
            yield return setting;
            var index = setting.LastIndexOf(':');
            if (index != -1)
                foreach (var ancestor in SelfAndAncestors(setting.Substring(0, index)))
                    yield return ancestor;
        }
    }
}
