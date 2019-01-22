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
        internal MessagingConfigurationProvider(IReceiver receiver)
        {
            Receiver = receiver;
            Receiver.Start(OnMessageReceivedAsync);
        }

        /// <summary>
        /// Gets the <see cref="IReceiver"/> that receives messages for changing
        /// configuration values.
        /// </summary>
        public IReceiver Receiver { get; }

        private async Task OnMessageReceivedAsync(IReceiverMessage message)
        {
            var newSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                JsonConvert.PopulateObject(message.StringPayload, newSettings);
            }
            catch
            {
                await message.RejectAsync().ConfigureAwait(false);
                return;
            }

            var changed = Data.Count != newSettings.Count;

            if (!changed)
            {
                foreach (var newSetting in newSettings)
                {
                    if (Data.ContainsKey(newSetting.Key))
                    {
                        if (Data[newSetting.Key] != newSetting.Value)
                        {
                            changed = true;
                            break;
                        }
                    }
                    else
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
            {
                Data = newSettings;
                OnReload();
            }

            await message.AcknowledgeAsync().ConfigureAwait(false);
        }
    }
}
