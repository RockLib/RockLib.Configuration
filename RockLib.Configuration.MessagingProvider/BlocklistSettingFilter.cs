using System;
using System.Collections.Generic;

namespace RockLib.Configuration.MessagingProvider
{
    /// <summary>
    /// An implementation of <see cref="ISettingFilter"/> that does not allow changes to
    /// settings that are members of a blocklist.
    /// </summary>
    public sealed class BlocklistSettingFilter : ISettingFilter
    {
        private readonly HashSet<string> _blockedSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlocklistSettingFilter"/> class.
        /// </summary>
        /// <param name="blockedSettings">
        /// The collection of settings (and their child settings) that will be blocked.
        /// </param>
        /// <param name="innerFilter">
        /// An optional setting filter that is applied if a setting is not a
        /// member of <see cref="BlockedSettings"/>.
        /// </param>
        public BlocklistSettingFilter(IEnumerable<string> blockedSettings, ISettingFilter? innerFilter = null)
        {
            if (blockedSettings is null)
            {
                throw new ArgumentNullException(nameof(blockedSettings));
            }
            _blockedSettings = new HashSet<string>(blockedSettings, StringComparer.OrdinalIgnoreCase);
            InnerFilter = innerFilter ?? NullSettingFilter.Instance;
        }

        /// <summary>
        /// Gets the collection of settings (and their child settings) that will be blocked.
        /// </summary>
        public IEnumerable<string> BlockedSettings => _blockedSettings;

        /// <summary>
        /// Gets the inner setting filter that is applied if a setting is not a
        /// member of <see cref="BlockedSettings"/>.
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
            !_blockedSettings.HasSetting(setting) && InnerFilter.ShouldProcessSettingChange(setting, receivedMessageHeaders);
    }
}
