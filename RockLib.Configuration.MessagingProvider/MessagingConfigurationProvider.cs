﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RockLib.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RockLib.Configuration.MessagingProvider
{
    public sealed class MessagingConfigurationProvider : ConfigurationProvider
    {
        internal MessagingConfigurationProvider(IReceiver receiver)
        {
            Receiver = receiver;
            Receiver.Start(OnMessageReceived);
        }

        public IReceiver Receiver { get; }

        private void OnMessageReceived(IReceiverMessage message)
        {
            var newSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                JsonConvert.PopulateObject(message.StringPayload, newSettings);
            }
            catch
            {
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
                IDictionary<string, string> previousData = null;

                if (message.Headers.TryGetValue("RevertAfterMilliseconds", out int milliseconds) && milliseconds > 0)
                    previousData = Data;

                Data = newSettings;
                OnReload();

                if (previousData != null)
                    RevertDataAfterDelay(previousData, milliseconds);
            }
        }

        private async void RevertDataAfterDelay(IDictionary<string, string> previousData, int milliseconds)
        {
            await Task.Delay(milliseconds).ConfigureAwait(false);
            Data = previousData;
            OnReload();
        }
    }
}