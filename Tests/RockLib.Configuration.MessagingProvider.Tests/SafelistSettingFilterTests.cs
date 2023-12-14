using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace RockLib.Configuration.MessagingProvider.Tests
{
    public static class SafelistSettingFilterTests
    {
        private static readonly string[] safeSettings = new[] { "foo" };

        [Fact]
        public static void ConstructorThrowsIfSafeSettingsIsNull()
        {
            var action = () => new SafelistSettingFilter(null!);
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public static void ConstructorSetsSafeSettings()
        {
            var safeSettings = new[] { "foo" };

            var filter = new SafelistSettingFilter(safeSettings);

            filter.SafeSettings.Should().BeEquivalentTo(safeSettings);
        }

        [Fact]
        public static void ConstructorSetsInnerFilter()
        {
            var innerFilter = Mock.Of<ISettingFilter>();

            var filter = new SafelistSettingFilter(safeSettings, innerFilter);

            filter.InnerFilter.Should().BeSameAs(innerFilter);
        }

        [Fact]
        public static void ReturnsWhatTheInnerFilterReturnsWhenTheSettingIsInTheSafelist()
        {
            var mockInnerFilter = new Mock<ISettingFilter>();
            mockInnerFilter
                .Setup(m => m.ShouldProcessSettingChange(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>()))
                .Returns(false);

            var filter = new SafelistSettingFilter(safeSettings, mockInnerFilter.Object);

            var receivedMessageHeaders = new Dictionary<string, object>();

            filter.ShouldProcessSettingChange("foo", receivedMessageHeaders)
                .Should().Be(false);

            mockInnerFilter.Verify(m => m.ShouldProcessSettingChange(
                It.Is<string>(s => s == "foo"), It.Is<IReadOnlyDictionary<string, object>>(headers => headers == receivedMessageHeaders)));
        }

        [Fact]
        public static void ReturnsWhatTheInnerFilterReturnsWhenTheSettingIsAChildOfAnItemInTheAllowlist()
        {
            var mockInnerFilter = new Mock<ISettingFilter>();
            mockInnerFilter
                .Setup(m => m.ShouldProcessSettingChange(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>()))
                .Returns(false);

            var filter = new SafelistSettingFilter(safeSettings, mockInnerFilter.Object);

            var receivedMessageHeaders = new Dictionary<string, object>();

            filter.ShouldProcessSettingChange("foo:bar", receivedMessageHeaders)
                .Should().Be(false);

            mockInnerFilter.Verify(m => m.ShouldProcessSettingChange(
                It.Is<string>(s => s == "foo:bar"), It.Is<IReadOnlyDictionary<string, object>>(headers => headers == receivedMessageHeaders)));
        }

        [Fact]
        public static void ReturnsFalseWhenTheSettingIsNotInTheSafelist()
        {
            var mockInnerFilter = new Mock<ISettingFilter>();

            var filter = new SafelistSettingFilter(safeSettings, mockInnerFilter.Object);

            filter.ShouldProcessSettingChange("bar", new Dictionary<string, object>())
                .Should().Be(false);

            mockInnerFilter.Verify(m => m.ShouldProcessSettingChange(
                It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Never);
        }
    }
}
