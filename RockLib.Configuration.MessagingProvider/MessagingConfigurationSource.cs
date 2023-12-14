﻿using Microsoft.Extensions.Configuration;
using RockLib.Messaging;
using System;
using System.Runtime.CompilerServices;

namespace RockLib.Configuration.MessagingProvider
{
    /// <summary>
    /// An <see cref="IConfigurationSource"/> that creates an <see cref="IConfigurationProvider"/>
    /// that reloads when it receives a message containing configuration changes.
    /// </summary>
    public sealed class MessagingConfigurationSource : IConfigurationSource
    {
        private static readonly ConditionalWeakTable<IReceiver, MessagingConfigurationSource> _validationCache = new ConditionalWeakTable<IReceiver, MessagingConfigurationSource>();

        private readonly Lazy<MessagingConfigurationProvider> _cachedProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingConfigurationSource"/> class.
        /// </summary>
        /// <param name="receiver">
        /// The <see cref="IReceiver"/> that will receive messages in order to change
        /// configuration values.
        /// </param>
        /// <param name="settingFilter">
        /// The <see cref="ISettingFilter"/> that is applied to each setting of each
        /// received message.
        /// </param>
        public MessagingConfigurationSource(IReceiver receiver, ISettingFilter? settingFilter = null)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(receiver);
#else
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }
#endif
            if (!ReferenceEquals(this, _validationCache.GetValue(receiver, r => this)))
            {
                throw new ArgumentException("The same instance of IReceiver cannot be used to create multiple instances of MessagingConfigurationSource.", nameof(receiver));
            }
            if (receiver.MessageHandler is not null)
            {
                throw new ArgumentException("The receiver is already started.", nameof(receiver));
            }

            Receiver = receiver;
            SettingFilter = settingFilter;
            _cachedProvider = new Lazy<MessagingConfigurationProvider>(() => new MessagingConfigurationProvider(Receiver, SettingFilter));
        }

        /// <summary>
        /// Gets the <see cref="IReceiver"/> that will receive messages in order to change
        /// configuration values.
        /// </summary>
        public IReceiver Receiver { get; }

        /// <summary>
        /// Gets the <see cref="ISettingFilter"/> that is applied to each setting of each
        /// received message.
        /// </summary>
        public ISettingFilter? SettingFilter { get; }

        /// <summary>
        /// Builds the <see cref="MessagingConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref=" MessagingConfigurationProvider"/>.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder) => _cachedProvider.Value;
    }
}
