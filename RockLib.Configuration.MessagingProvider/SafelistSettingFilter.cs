using System;
using System.Collections.Generic;

namespace RockLib.Configuration.MessagingProvider
{
    /// <summary>
    /// An implementation of <see cref="ISettingFilter"/> that only allows changes to
    /// settings that are members of a safelist - all other settings are blocked.
    /// </summary>
    public sealed class SafelistSettingFilter : ISettingFilter
    {
        private readonly HashSet<string> _safeSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafelistSettingFilter"/> class.
        /// </summary>
        /// <param name="safeSettings">
        /// The collection of settings (and their child settings) that are safe - all
        /// other settings will be blocked.
        /// </param>
        /// <param name="innerFilter">
        /// An optional setting filter that is applied if a setting is a
        /// member of <see cref="SafeSettings"/>.
        /// </param>
        public SafelistSettingFilter(IEnumerable<string> safeSettings, ISettingFilter? innerFilter = null)
        {
            if (safeSettings is null)
            {
                throw new ArgumentNullException(nameof(safeSettings));
            }
            _safeSettings = new HashSet<string>(safeSettings, StringComparer.OrdinalIgnoreCase);
            InnerFilter = innerFilter ?? NullSettingFilter.Instance;
        }

        /// <summary>
        /// Gets the collection of settings (and their child settings) that are safe - all
        /// other settings will be blocked.
        /// </summary>
        public IEnumerable<string> SafeSettings => _safeSettings;

        /// <summary>
        /// Gets the inner setting filter that is applied if a setting is a
        /// member of <see cref="SafeSettings"/>.
        /// </summary>
        public ISettingFilter InnerFilter { get; }

        /// <summary>
        /// Returns whether the specified setting should be allowed to be changed.
        /// </summary>
        /// <param name="setting">The setting to potentially change.</param>
        /// <param name="receivedMessageHeaders">
        /// The headers of the message that was received that has initiated the change.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the setting is allowed to be changed; otherwise
        /// <see langword="false"/> if the setting is not allowed to be changed.
        /// </returns>
        public bool ShouldProcessSettingChange(string setting, IReadOnlyDictionary<string, object> receivedMessageHeaders) =>
            _safeSettings.HasSetting(setting) && InnerFilter.ShouldProcessSettingChange(setting, receivedMessageHeaders);
    }
}
