using FluentAssertions;
using RockLib.Messaging;
using System;
using Xunit;

namespace RockLib.Configuration.MessagingProvider.Tests
{
    public static class MessagingConfigurationSourceTests
    {
        [Fact]
        public static void ConstructorThrowsIfReceiverIsNull()
        {
            var action = () => new MessagingConfigurationSource(null!);
            action.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiver*");
        }

        [Fact]
        public static void ConstructorThrowsIfReceiverIsUsedByAnotherMessagingConfigurationSource()
        {
            using var receiver = new FakeReceiver("fake");

            // Create a source with the receiver and throw it away.
            _ = new MessagingConfigurationSource(receiver);

            // Passing the same receiver to another source causes it to throw.
            var action = () => new MessagingConfigurationSource(receiver);
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The same instance of IReceiver cannot be used to create multiple instances of MessagingConfigurationSource.*receiver*");
        }

        [Fact]
        public static void ConstructorThrowsIfReceiverIsAlreadyStarted()
        {
            using var receiver = new FakeReceiver("fake");
            receiver.Start(m => m.AcknowledgeAsync());

            var action = () => new MessagingConfigurationSource(receiver);
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The receiver is already started.*receiver*");
        }

        [Fact]
        public static void ConstructorSetsReceiverProperty()
        {
            using var receiver = new FakeReceiver("fake");

            var source = new MessagingConfigurationSource(receiver);

            source.Receiver.Should().BeSameAs(receiver);
        }

        [Fact]
        public static void ConstructorSetsSettingFilterProperty()
        {
            using var receiver = new FakeReceiver("fake");
            var filter = new FakeSettingFilter();

            var source = new MessagingConfigurationSource(receiver, filter);

            source.SettingFilter.Should().BeSameAs(filter);
        }

        [Fact]
        public static void BuildMethodReturnsMessagingConfigurationProvider()
        {
            using var receiver = new FakeReceiver("fake");
            var filter = new FakeSettingFilter();

            var source = new MessagingConfigurationSource(receiver, filter);

            var provider = source.Build(null!);

            provider.Should().BeOfType<MessagingConfigurationProvider>();

            var messagingProvider = (MessagingConfigurationProvider)provider;

            messagingProvider.Receiver.Should().BeSameAs(receiver);
            messagingProvider.SettingFilter.Should().BeSameAs(filter);
        }

        [Fact]
        public static void BuildMethodReturnsSameMessagingConfigurationProviderEachTime()
        {
            using var receiver = new FakeReceiver("fake");

            var source = new MessagingConfigurationSource(receiver);

            var provider1 = source.Build(null!);
            var provider2 = source.Build(null!);

            provider1.Should().BeSameAs(provider2);
        }
    }
}
