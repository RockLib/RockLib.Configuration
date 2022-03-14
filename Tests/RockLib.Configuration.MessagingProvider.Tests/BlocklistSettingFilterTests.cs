﻿using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace RockLib.Configuration.MessagingProvider.Tests
{
    public static class BlocklistSettingFilterTests
    {
        [Fact]
        public static void ConstructorThrowsIfBlockedSettingsIsNull()
        {
            var action = () => new BlocklistSettingFilter(null!);
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public static void ConstructorSetsBlockedSettings()
        {
            var blockedSettings = new[] { "foo" };

            var filter = new BlocklistSettingFilter(blockedSettings);

            filter.BlockedSettings.Should().BeEquivalentTo(blockedSettings);
        }

        [Fact]
        public static void ConstructorSetsInnerFilter()
        {
            var innerFilter = Mock.Of<ISettingFilter>();// new FakeSettingFilter();

            var filter = new BlocklistSettingFilter(new[] { "foo" }, innerFilter);

            filter.InnerFilter.Should().BeSameAs(innerFilter);
        }

        [Fact]
        public static void ReturnsWhatTheInnerFilterReturnsWhenTheSettingIsNotInTheBlocklist()
        {
            var mockInnerFilter = new Mock<ISettingFilter>();
            mockInnerFilter
                .Setup(m => m.ShouldProcessSettingChange(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>()))
                .Returns(false);

            var filter = new BlocklistSettingFilter(new[] { "foo" }, mockInnerFilter.Object);

            var receivedMessageHeaders = new Dictionary<string, object>();

            filter.ShouldProcessSettingChange("bar", receivedMessageHeaders)
                .Should().Be(false);

            mockInnerFilter.Verify(m => m.ShouldProcessSettingChange(
                It.Is<string>(s => s == "bar"), It.Is<IReadOnlyDictionary<string, object>>(headers => headers == receivedMessageHeaders)));
        }

        [Fact]
        public static void ReturnsFalseWhenTheSettingIsInTheBlocklist()
        {
            var mockInnerFilter = new Mock<ISettingFilter>();

            var filter = new BlocklistSettingFilter(new[] { "foo" }, mockInnerFilter.Object);

            filter.ShouldProcessSettingChange("foo", new Dictionary<string, object>())
                .Should().Be(false);

            mockInnerFilter.Verify(m => m.ShouldProcessSettingChange(
                It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Never);
        }

        [Fact]
        public static void ReturnsFalseWhenTheSettingIsAChildOfAnItemInTheBlocklist()
        {
            var mockInnerFilter = new Mock<ISettingFilter>();

            var filter = new BlocklistSettingFilter(new[] { "foo" }, mockInnerFilter.Object);

            filter.ShouldProcessSettingChange("foo:bar", new Dictionary<string, object>())
                .Should().Be(false);

            mockInnerFilter.Verify(m => m.ShouldProcessSettingChange(
                It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Never);
        }
    }
}
