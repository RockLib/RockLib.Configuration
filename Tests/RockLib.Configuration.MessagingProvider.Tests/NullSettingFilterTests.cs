using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace RockLib.Configuration.MessagingProvider.Tests
{
    public static class NullSettingFilterTests
    {
        [Fact]
        public static void AlwaysReturnsTrue()
        {
            NullSettingFilter.Instance.ShouldProcessSettingChange(null!, new Dictionary<string, object>()).Should().BeTrue();
        }
    }
}
