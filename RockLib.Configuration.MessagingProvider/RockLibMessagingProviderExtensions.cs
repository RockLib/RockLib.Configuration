using Microsoft.Extensions.Configuration;
using RockLib.Messaging;

namespace RockLib.Configuration.MessagingProvider
{
    public static class RockLibMessagingProviderExtensions
    {
        public static IConfigurationBuilder AddRockLibMessagingProvider(this IConfigurationBuilder builder, string scenarioName) =>
            builder.AddRockLibMessagingProvider(MessagingScenarioFactory.CreateReceiver(scenarioName));

        public static IConfigurationBuilder AddRockLibMessagingProvider(this IConfigurationBuilder builder, IReceiver receiver) =>
            builder.Add(new MessagingConfigurationSource(receiver));
    }
}
