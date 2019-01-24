using FluentAssertions;
using Microsoft.Extensions.Primitives;
using RockLib.Dynamic;
using RockLib.Messaging.Testing;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Configuration.MessagingProvider.Tests
{
    public class MessagingConfigurationProviderTests
    {
        [Fact]
        public void ConstructorSetsReceiverProperty()
        {
            var receiver = new FakeReceiver("fake");

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null);

            provider.Receiver.Should().BeSameAs(receiver);
        }

        [Fact]
        public void ConstructorSetsSettingFilterProperty()
        {
            var receiver = new FakeReceiver("fake");
            var filter = new FakeSettingFilter();

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, filter);

            provider.SettingFilter.Should().BeSameAs(filter);
        }

        [Fact]
        public void ConstructorStartsTheReceiver()
        {
            var receiver = new FakeReceiver("fake");

            receiver.MessageHandler.Should().BeNull();

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null);

            receiver.MessageHandler.Should().NotBeNull();
        }

        [Fact]
        public void ConstructorLeavesDataEmpty()
        {
            var receiver = new FakeReceiver("fake");

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null);

            GetData(provider).Should().BeEmpty();
        }

        [Fact]
        public async Task HappyPathNewSetting()
        {
            var receiver = new FakeReceiver("fake");

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null);

            var newSettings = @"{
  ""foo"": ""abc""
}";
            var message = new FakeReceiverMessage(newSettings);

            var reloaded = false;
            ChangeToken.OnChange(provider.GetReloadToken, () => reloaded = true);

            var dataBefore = GetData(provider);

            // Simulate the FakeReceiver receiving a message.
            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, message);

            // The protected Data property should have been replaced.
            GetData(provider).Should().NotBeSameAs(dataBefore);

            // Data should contain the new settings.
            GetData(provider).Should().ContainKey("foo");
            GetData(provider)["foo"].Should().Be("abc");

            // It should report that it has been reloaded.
            reloaded.Should().BeTrue();

            // The received message should have been handled by acknowledging it.
            message.Handled.Should().BeTrue();
            message.HandledBy.Should().Be(nameof(message.AcknowledgeAsync));
        }

        [Fact]
        public async Task HappyPathChangedSetting()
        {
            var receiver = new FakeReceiver("fake");

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null);
            GetData(provider).Add("foo", "xyz");

            var newSettings = @"{
  ""foo"": ""abc""
}";
            var message = new FakeReceiverMessage(newSettings);

            var reloaded = false;
            ChangeToken.OnChange(provider.GetReloadToken, () => reloaded = true);

            var dataBefore = GetData(provider);

            // Simulate the FakeReceiver receiving a message.
            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, message);

            // The protected Data property should have been replaced.
            GetData(provider).Should().NotBeSameAs(dataBefore);

            // Data should contain the new settings.
            GetData(provider).Should().ContainKey("foo");
            GetData(provider)["foo"].Should().Be("abc");

            // It should report that it has been reloaded.
            reloaded.Should().BeTrue();

            // The received message should have been handled by acknowledging it.
            message.Handled.Should().BeTrue();
            message.HandledBy.Should().Be(nameof(message.AcknowledgeAsync));
        }

        [Fact]
        public async Task HappyPathRemovedSetting()
        {
            var receiver = new FakeReceiver("fake");

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null);
            GetData(provider).Add("foo", "abc");

            var newSettings = @"{}";
            var message = new FakeReceiverMessage(newSettings);

            var reloaded = false;
            ChangeToken.OnChange(provider.GetReloadToken, () => reloaded = true);

            var dataBefore = GetData(provider);

            // Simulate the FakeReceiver receiving a message.
            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, message);

            // The protected Data property should have been replaced.
            GetData(provider).Should().NotBeSameAs(dataBefore);

            // Data should contain the new settings (which is empty).
            GetData(provider).Should().BeEmpty();

            // It should report that it has been reloaded.
            reloaded.Should().BeTrue();

            // The received message should have been handled by acknowledging it.
            message.Handled.Should().BeTrue();
            message.HandledBy.Should().Be(nameof(message.AcknowledgeAsync));
        }

        [Fact]
        public async Task HappyPathNothingChanged()
        {
            var receiver = new FakeReceiver("fake");

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null);
            GetData(provider).Add("foo", "abc");

            var reloaded = false;
            ChangeToken.OnChange(provider.GetReloadToken, () => reloaded = true);

            var newSettings = @"{
  ""foo"": ""abc""
}";

            var message = new FakeReceiverMessage(newSettings);

            var dataBefore = GetData(provider);

            // Simulate the FakeReceiver receiving a message.
            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, message);

            // The protected Data property should not have been replaced.
            GetData(provider).Should().BeSameAs(dataBefore);

            // Data should (still) contain the new settings.
            GetData(provider).Should().ContainKey("foo");
            GetData(provider)["foo"].Should().Be("abc");

            // It should report that it has been reloaded.
            reloaded.Should().BeFalse();

            // The received message should have been handled by acknowledging it.
            message.Handled.Should().BeTrue();
            message.HandledBy.Should().Be(nameof(message.AcknowledgeAsync));
        }

        [Fact]
        public async Task InvalidMessage()
        {
            var receiver = new FakeReceiver("fake");

            MessagingConfigurationProvider provider = typeof(MessagingConfigurationProvider).New(receiver, null);

            var reloaded = false;
            ChangeToken.OnChange(provider.GetReloadToken, () => reloaded = true);

            var newSettings = "This is {not] a [valid} JSON string: \"";

            var message = new FakeReceiverMessage(newSettings);

            var dataBefore = GetData(provider);

            // Simulate the FakeReceiver receiving a message.
            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, message);

            // The protected Data property should not have been replaced.
            GetData(provider).Should().BeSameAs(dataBefore);

            // It should report that it has been reloaded.
            reloaded.Should().BeFalse();

            // The received message should have been handled by acknowledging it.
            message.Handled.Should().BeTrue();
            message.HandledBy.Should().Be(nameof(message.RejectAsync));
        }

        private static IDictionary<string, string> GetData(MessagingConfigurationProvider provider) => provider.Unlock().Data;
    }
}
