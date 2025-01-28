using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;

#if NET8_0_OR_GREATER
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#endif
namespace RockLib.Configuration.CustomConfigurationManagerTests;

public class ConfigTests
{
    private readonly IConfiguration _configuration;

    public ConfigTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    { "AppSettings:Environment", "Test" },
                    { "AppSettings:ApplicationId", "200001" }
                })
            .Build();

        Config.SetRoot(_configuration);
    }

    [Fact(DisplayName = "CustomConfigurationManagerTests: IsDefault is false when SetConfigurationRoot has been called.")]
    public void IsDefaultIsFalseWhenCustomized()
    {
        Assert.False(Config.IsDefault);
    }

    [Fact(DisplayName = "CustomConfigurationManagerTests: The ConfigurationRoot property is the same instance passed to SetConfigurationRoot.")]
    public void TheCustomConfigurationRootIsUsed()
    {
        Assert.Same(_configuration, Config.Root);
        Assert.Equal("Test", Config.AppSettings["Environment"]);
        Assert.Equal("200001", Config.AppSettings["ApplicationId"]);
    }
}
#if NET8_0_OR_GREATER
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#endif