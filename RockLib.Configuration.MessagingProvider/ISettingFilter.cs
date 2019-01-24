using System.Collections.Generic;

namespace RockLib.Configuration.MessagingProvider
{
    /// <summary>
    /// Defines an object that can determine if a specific setting from a received message
    /// is allowed to be changed.
    /// </summary>
    public interface ISettingFilter
    {
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
        bool ShouldProcessSettingChange(string setting, IReadOnlyDictionary<string, object> receivedMessageHeaders);
    }
}
