using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace RockLib.Configuration.MessagingProvider.Tests
{
    public class NullSettingFilterTests
    {
        [Fact]
        public void AlwaysReturnsTrue()
        {
            NullSettingFilter.Instance.ShouldProcessSettingChange(null, new Dictionary<string, object>()).Should().BeTrue();
        }
    }
}
