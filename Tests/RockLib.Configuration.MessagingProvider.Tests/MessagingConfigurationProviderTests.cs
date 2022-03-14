using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Moq;
using RockLib.Dynamic;
using RockLib.Messaging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Configuration.MessagingProvider.Tests
{
    public static class MessagingConfigurationProviderTests
    {
        [Fact]
        public static void ConstructorSetsReceiverProperty()
        {
            using var receiver = new Mock<Receiver>("fake").Object;

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null!);

            provider.Receiver.Should().BeSameAs(receiver);
        }

        [Fact]
        public static void ConstructorSetsSettingFilterProperty()
        {
            using var receiver = new Mock<Receiver>("fake").Object;
            var filter = Mock.Of<ISettingFilter>();

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, filter);

            provider.SettingFilter.Should().BeSameAs(filter);
        }

        [Fact]
        public static void ConstructorStartsTheReceiver()
        {
            using var receiver = new Mock<Receiver>("fake").Object;

            receiver.MessageHandler.Should().BeNull();

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null!);

            receiver.MessageHandler.Should().NotBeNull();
        }

        [Fact]
        public static void ConstructorLeavesDataEmpty()
        {
            using var receiver = new Mock<Receiver>("fake").Object;

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null!);

            GetData(provider).Should().BeEmpty();
        }

        [Fact]
        public static async Task HappyPathNewSetting()
        {
            using var receiver = new Mock<Receiver>("fake").Object;

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null!);

            var newSettings = @"{
  ""foo"": ""abc""
}";

            var isHandled = false;
            var messageMock = new Mock<IReceiverMessage>();
            messageMock.Setup(_ => _.StringPayload).Returns(newSettings);
            messageMock.Setup(_ => _.AcknowledgeAsync(It.IsAny<CancellationToken>())).Callback(() => isHandled = true);
            var message = messageMock.Object;

            var reloaded = false;
            ChangeToken.OnChange(provider.GetReloadToken, () => reloaded = true);

            var dataBefore = GetData(provider);

            // Simulate the FakeReceiver receiving a message.
            await receiver.MessageHandler!.OnMessageReceivedAsync(receiver, message).ConfigureAwait(false);

            // The protected Data property should have been replaced.
            GetData(provider).Should().NotBeSameAs(dataBefore);

            // Data should contain the new settings.
            GetData(provider).Should().ContainKey("foo");
            GetData(provider)["foo"].Should().Be("abc");

            // It should report that it has been reloaded.
            reloaded.Should().BeTrue();

            // The received message should have been handled by acknowledging it.
            isHandled.Should().BeTrue();
        }

        [Fact]
        public static async Task ReceiveMessageWithChangedSetting()
        {
            using var receiver = new Mock<Receiver>("fake").Object;

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null!);
            GetData(provider).Add("foo", "xyz");

            var newSettings = @"{
  ""foo"": ""abc""
}";

            var isHandled = false;
            var messageMock = new Mock<IReceiverMessage>();
            messageMock.Setup(_ => _.StringPayload).Returns(newSettings);
            messageMock.Setup(_ => _.AcknowledgeAsync(It.IsAny<CancellationToken>())).Callback(() => isHandled = true);
            var message = messageMock.Object;

            var reloaded = false;
            ChangeToken.OnChange(provider.GetReloadToken, () => reloaded = true);

            var dataBefore = GetData(provider);

            // Simulate the FakeReceiver receiving a message.
            await receiver.MessageHandler!.OnMessageReceivedAsync(receiver, message).ConfigureAwait(false);

            // The protected Data property should have been replaced.
            GetData(provider).Should().NotBeSameAs(dataBefore);

            // Data should contain the new settings.
            GetData(provider).Should().ContainKey("foo");
            GetData(provider)["foo"].Should().Be("abc");

            // It should report that it has been reloaded.
            reloaded.Should().BeTrue();

            // The received message should have been handled by acknowledging it.
            isHandled.Should().BeTrue();
        }

        [Fact]
        public static async Task ReceiveMessageWithRemovedSetting()
        {
            using var receiver = new Mock<Receiver>("fake").Object;

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null!);
            GetData(provider).Add("foo", "abc");

            var newSettings = @"{}";

            var isHandled = false;
            var messageMock = new Mock<IReceiverMessage>();
            messageMock.Setup(_ => _.StringPayload).Returns(newSettings);
            messageMock.Setup(_ => _.AcknowledgeAsync(It.IsAny<CancellationToken>())).Callback(() => isHandled = true);
            var message = messageMock.Object;

            var reloaded = false;
            ChangeToken.OnChange(provider.GetReloadToken, () => reloaded = true);

            var dataBefore = GetData(provider);

            // Simulate the FakeReceiver receiving a message.
            await receiver.MessageHandler!.OnMessageReceivedAsync(receiver, message).ConfigureAwait(false);

            // The protected Data property should have been replaced.
            GetData(provider).Should().NotBeSameAs(dataBefore);

            // Data should contain the new settings (which is empty).
            GetData(provider).Should().BeEmpty();

            // It should report that it has been reloaded.
            reloaded.Should().BeTrue();

            // The received message should have been handled by acknowledging it.
            isHandled.Should().BeTrue();
        }

        [Fact]
        public static async Task ReceiveMessageWhereNothingChanged()
        {
            using var receiver = new Mock<Receiver>("fake").Object;

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null!);
            GetData(provider).Add("foo", "abc");

            var reloaded = false;
            ChangeToken.OnChange(provider.GetReloadToken, () => reloaded = true);

            var newSettings = @"{
  ""foo"": ""abc""
}";

            var isHandled = false;
            var messageMock = new Mock<IReceiverMessage>();
            messageMock.Setup(_ => _.StringPayload).Returns(newSettings);
            messageMock.Setup(_ => _.AcknowledgeAsync(It.IsAny<CancellationToken>())).Callback(() => isHandled = true);
            var message = messageMock.Object;

            var dataBefore = GetData(provider);

            // Simulate the FakeReceiver receiving a message.
            await receiver.MessageHandler!.OnMessageReceivedAsync(receiver, message).ConfigureAwait(false);

            // The protected Data property should not have been replaced.
            GetData(provider).Should().BeSameAs(dataBefore);

            // Data should (still) contain the new settings.
            GetData(provider).Should().ContainKey("foo");
            GetData(provider)["foo"].Should().Be("abc");

            // It should report that it has been reloaded.
            reloaded.Should().BeFalse();

            // The received message should have been handled by acknowledging it.
            isHandled.Should().BeTrue();

            messageMock.VerifyAll();
        }

        [Fact]
        public static async Task ReceiveMessageWithInvalidMessage()
        {
            using var receiver = new Mock<Receiver>("fake").Object;

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null!);

            var reloaded = false;
            ChangeToken.OnChange(provider.GetReloadToken, () => reloaded = true);

            var newSettings = "This is {not] a [valid} JSON string: \"";

            var isHandled = false;
            var messageMock = new Mock<IReceiverMessage>();
            messageMock.Setup(_ => _.StringPayload).Returns(newSettings);
            messageMock.Setup(_ => _.RejectAsync(It.IsAny<CancellationToken>())).Callback(() => isHandled = true);
            var message = messageMock.Object;

            var dataBefore = GetData(provider);

            // Simulate the FakeReceiver receiving a message.
            await receiver.MessageHandler!.OnMessageReceivedAsync(receiver, message).ConfigureAwait(false);

            // The protected Data property should not have been replaced.
            GetData(provider).Should().BeSameAs(dataBefore);

            // It should report that it has been reloaded.
            reloaded.Should().BeFalse();

            // The received message should have been handled by acknowledging it.
            isHandled.Should().BeTrue();

            messageMock.VerifyAll();
        }

        private static IDictionary<string, string> GetData(MessagingConfigurationProvider provider) => provider.Unlock().Data;
    }
}
