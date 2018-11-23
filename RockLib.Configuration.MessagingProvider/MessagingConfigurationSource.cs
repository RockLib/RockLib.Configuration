using Microsoft.Extensions.Configuration;
using RockLib.Messaging;
using System;
using System.Runtime.CompilerServices;

namespace RockLib.Configuration.MessagingProvider
{
    public sealed class MessagingConfigurationSource : IConfigurationSource
    {
        private static readonly ConditionalWeakTable<IReceiver, MessagingConfigurationSource> _validationCache = new ConditionalWeakTable<IReceiver, MessagingConfigurationSource>();

        private readonly Lazy<MessagingConfigurationProvider> _cachedProvider;

        public MessagingConfigurationSource(IReceiver receiver)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));
            if (!ReferenceEquals(this, _validationCache.GetValue(receiver, r => this)))
                throw new ArgumentException("The same instance of IReceiver cannot be used to create multiple instances of MessagingConfigurationSource.", nameof(receiver));
            if (receiver.MessageHandler != null)
                throw new ArgumentException("The receiver is already started.", nameof(receiver));

            Receiver = receiver;
            _cachedProvider = new Lazy<MessagingConfigurationProvider>(() => new MessagingConfigurationProvider(Receiver));
        }

        public IReceiver Receiver { get; }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => _cachedProvider.Value;
    }
}
