using Microsoft.Extensions.Configuration;
using RockLib.Messaging;

namespace RockLib.Configuration.MessagingProvider
{
    public static class RockLibMessagingProviderExtensions
    {
        public static IConfigurationBuilder AddRockLibMessagingProvider(this IConfigurationBuilder builder, string scenarioName) =>
            builder.AddRockLibMessagingProvider(builder.Build().GetSection("RockLib.Messaging").CreateReceiver(scenarioName));

        public static IConfigurationBuilder AddRockLibMessagingProvider(this IConfigurationBuilder builder, IReceiver receiver) =>
            builder.Add(new MessagingConfigurationSource(receiver));
    }
}
