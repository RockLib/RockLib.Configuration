using System.Collections.Generic;

namespace RockLib.Configuration.MessagingProvider.Tests
{
    internal sealed class FakeSettingFilter : ISettingFilter
    {
        public bool ShouldProcessSettingChange(string setting, IReadOnlyDictionary<string, object> receivedMessageHeaders) => false;
    }
}
