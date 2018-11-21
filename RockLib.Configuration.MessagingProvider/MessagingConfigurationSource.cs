using Microsoft.Extensions.Configuration;
using RockLib.Messaging;
using System;

namespace RockLib.Configuration.MessagingProvider
{
    public class MessagingConfigurationSource : IConfigurationSource
    {
        public MessagingConfigurationSource(IReceiver receiver)
        {
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }

        public IReceiver Receiver { get; }

        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new MessagingConfigurationProvider(this);
    }
}
