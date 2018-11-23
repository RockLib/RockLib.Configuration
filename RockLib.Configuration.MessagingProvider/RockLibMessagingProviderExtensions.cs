using Microsoft.Extensions.Configuration;
using RockLib.Messaging;

namespace RockLib.Configuration.MessagingProvider
{
    /// <summary>
    /// Extension methods for registering <see cref="MessagingConfigurationProvider"/>
    /// with <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public static class RockLibMessagingProviderExtensions
    {
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reloads with changes
        /// specified in messages received from a new <see cref="IReceiver"/> with the
        /// specified name created from the built builder.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="receiverName">The name of the receiver.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// <remarks>
        /// This is how the <see cref="IReceiver"/> is created:
        /// <code>
        /// builder.Build().GetSection("RockLib.Messaging").CreateReceiver(receiverName)
        /// </code>
        /// </remarks>
        public static IConfigurationBuilder AddRockLibMessagingProvider(this IConfigurationBuilder builder, string receiverName) =>
            builder.AddRockLibMessagingProvider(builder.Build().GetSection("RockLib.Messaging").CreateReceiver(receiverName));

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reloads with changes
        /// specified in messages received from the <see cref="IReceiver"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="receiver">
        /// The object that listens for messages that update configuration values.
        /// </param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddRockLibMessagingProvider(this IConfigurationBuilder builder, IReceiver receiver) =>
            builder.Add(new MessagingConfigurationSource(receiver));
    }
}
