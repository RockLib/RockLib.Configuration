using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RockLib.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RockLib.Configuration.MessagingProvider
{
    /// <summary>
    /// An <see cref="IConfigurationProvider"/> that reloads when it receives
    /// a message containing configuration changes.
    /// </summary>
    public sealed class MessagingConfigurationProvider : ConfigurationProvider
    {
        internal MessagingConfigurationProvider(IReceiver receiver, ISettingFilter? settingFilter)
        {
            Receiver = receiver;
            SettingFilter = settingFilter ?? NullSettingFilter.Instance;
            Receiver.Start(OnMessageReceivedAsync);
        }

        /// <summary>
        /// Gets the <see cref="IReceiver"/> that receives messages for changing
        /// configuration values.
        /// </summary>
        public IReceiver Receiver { get; }

        /// <summary>
        /// Gets the <see cref="ISettingFilter"/> that is applied to each setting of each
        /// received message.
        /// </summary>
        public ISettingFilter SettingFilter { get; }

        private async Task OnMessageReceivedAsync(IReceiverMessage message)
        {
            var newSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                JsonConvert.PopulateObject(message.StringPayload, newSettings);
            }
            catch(JsonSerializationException)
            {
                await message.RejectAsync().ConfigureAwait(false);
                return;
            }

            if (IsChanged(newSettings, message.Headers))
            {
                Data = newSettings;
                OnReload();
            }

            await message.AcknowledgeAsync().ConfigureAwait(false);
        }

        private bool IsChanged(Dictionary<string, string> newSettings, HeaderDictionary headers)
        {
            foreach (var newSetting in newSettings)
            {
                if (Data.ContainsKey(newSetting.Key))
                {
                    if (Data[newSetting.Key] != newSetting.Value)
                    {
                        return true;
                    }
                }
                else if (SettingFilter.ShouldProcessSettingChange(newSetting.Key, headers))
                {
                    return true;
                }
            }

            foreach (var oldSetting in Data)
            {
                if (!newSettings.ContainsKey(oldSetting.Key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
