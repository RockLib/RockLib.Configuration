using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Xunit;

namespace RockLib.Configuration.MessagingProvider.Tests
{
    public class RockLibMessagingProviderExtensionsTests
    {
        [Fact]
        public void AddRockLibMessagingProviderExtensionMethod1AddsAMessagingConfigurationSourceToTheConfigurationBuilder()
        {
            var filter = new FakeSettingFilter();

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddRockLibMessagingProvider("fake", filter);

            builder.Sources.Should().HaveCount(2);
            builder.Sources[0].Should().BeOfType<JsonConfigurationSource>();
            builder.Sources[1].Should().BeOfType<MessagingConfigurationSource>();

            var source = (MessagingConfigurationSource)builder.Sources[1];

            source.Receiver.Should().BeOfType<FakeReceiver>();
            source.Receiver.Name.Should().Be("fake");
            source.SettingFilter.Should().BeSameAs(filter);
        }

        [Fact]
        public void AddRockLibMessagingProviderExtensionMethod2AddsAMessagingConfigurationSourceToTheConfigurationBuilder()
        {
            var receiver = new FakeReceiver("fake");
            var filter = new FakeSettingFilter();

            var builder = new ConfigurationBuilder();

            builder.AddRockLibMessagingProvider(receiver, filter);

            builder.Sources.Should().HaveCount(1);
            builder.Sources[0].Should().BeOfType<MessagingConfigurationSource>();

            var source = (MessagingConfigurationSource)builder.Sources[0];

            source.Receiver.Should().BeSameAs(receiver);
            source.SettingFilter.Should().BeSameAs(filter);
        }
    }
}
