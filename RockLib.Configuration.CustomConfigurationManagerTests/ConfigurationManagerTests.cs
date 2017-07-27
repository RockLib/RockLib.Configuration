using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;

namespace RockLib.Configuration.CustomConfigurationManagerTests
{
    public class ConfigurationManagerTests
    {
        private static readonly IConfigurationRoot _configurationRoot;

        static ConfigurationManagerTests()
        {
            _configurationRoot = new ConfigurationBuilder()
                    .AddInMemoryCollection(
                        new Dictionary<string, string>
                        {
                            { "AppSettings:Environment", "Test" },
                            { "AppSettings:ApplicationId", "200001" }
                        })
                    .Build();

            ConfigurationManager.SetConfigurationRoot(_configurationRoot);
        }

        [Fact(DisplayName = "CustomConfigurationManagerTests: IsDefault is false when SetConfigurationRoot has been called.")]
        public void IsDefaultIsFalseWhenCustomized()
        {
            Assert.False(ConfigurationManager.IsDefault);
        }

        [Fact(DisplayName = "CustomConfigurationManagerTests: The ConfigurationRoot property is the same instance passed to SetConfigurationRoot.")]
        public void TheCustomConfigurationRootIsUsed()
        {
            Assert.Same(_configurationRoot, ConfigurationManager.ConfigurationRoot);
            Assert.Equal("Test", ConfigurationManager.AppSettings["Environment"]);
            Assert.Equal("200001", ConfigurationManager.AppSettings["ApplicationId"]);
        }
    }
}
