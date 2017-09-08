using System.Collections.Generic;
using Xunit;

namespace RockLib.Configuration.AppConfigTests
{
    public class ConfigTests
    {
        [Fact]
        public void AppSetting_WillPullValueFromAppConfigFile()
        {
            var value = Config.AppSettings["Key100"];

            Assert.Equal(value, "Key100_Value");
        }

        [Fact]
        public void AppSetting_WhenValueNotInAppSetting_WillThrowException()
        {
            Assert.Throws<KeyNotFoundException>(() => Config.AppSettings["KeyNotFound"]);
        }
    }
}
