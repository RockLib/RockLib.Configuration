using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace RockLib.Configuration.Remote.Tests;

public class ConfigurationBuilderExtensionsTests
{
    private const string OriginalApiEndpoint = "OriginalApiEndpoint";
    private const string ChangedApiEndpoint = "ChangedApiEndpoint";
    private const string ConfigSection = "configSection";
    private readonly ConfigurationBuilder _configurationBuilder;

    public ConfigurationBuilderExtensionsTests()
    {
        _configurationBuilder = new ConfigurationBuilder();
    }
    [Fact]
    public void AddRemoteShouldAddConfigurationSourceToSourcesWhenConfigurationSourceParameterProvided()
    {
        // Arrange
        var remoteConfigurationSource = new RemoteConfigurationSource
        {
            ApiEndpoint = OriginalApiEndpoint
        };

        // Act
        var response = _configurationBuilder.AddRemote(remoteConfigurationSource);

        // Assert
        response.Sources.Should().Contain(remoteConfigurationSource);
    }

    [Fact]
    public void AddRemoteShouldCreateConfigurationSourceWithConfigSectionWhenCalledWithConfigSection()
    {
         // Arrange
        var inMemoryCollection = new Dictionary<string, string>
        {
            {$"{ConfigSection}:ApiEndpoint", OriginalApiEndpoint}
        };
        _configurationBuilder.AddInMemoryCollection(inMemoryCollection);

        // Act
        var response = _configurationBuilder.AddRemote(ConfigSection);

        // Assert
        response.Sources.Should().ContainSingle(source => source is RemoteConfigurationSource)
            .Which.Should().BeAssignableTo<RemoteConfigurationSource>()
            .Which.ApiEndpoint.Should().Be(OriginalApiEndpoint);
    }

    [Fact]
    public void AddRemoteShouldModifyConfigurationSourceWithThatActionWhenCalledWithConfigSectionAndAction()
    {
         // Arrange
        var inMemoryCollection = new Dictionary<string, string>
        {
            {$"{ConfigSection}:ApiEndpoint", OriginalApiEndpoint}
        };

        // Act
        _configurationBuilder.AddInMemoryCollection(inMemoryCollection);

        var response = _configurationBuilder.AddRemote(ConfigSection, remote =>
        {
            remote.Options.ApiEndpoint = ChangedApiEndpoint;
        });

        // Assert
        response.Sources.Should().ContainSingle(source => source is RemoteConfigurationSource)
            .Which.Should().BeAssignableTo<RemoteConfigurationSource>()
            .Which.ApiEndpoint.Should().Be(ChangedApiEndpoint);
    }

    [Fact]
    public void AddRemoteShouldAddConfigurationSourceWithApiEndpointToTheBuilderWhenActionSetsApiEndPoint()
    {
        // Arrange and Act
        var response = _configurationBuilder.AddRemote(remote =>
        {
            remote.Options.ApiEndpoint = ChangedApiEndpoint;
        });

        // Assert
        response.Sources.Should().ContainSingle(source => source is RemoteConfigurationSource)
            .Which.Should().BeAssignableTo<RemoteConfigurationSource>()
            .Which.ApiEndpoint.Should().Be(ChangedApiEndpoint);
    }

    [Fact]
    public void AddRemoteThePriorConfigurationIsLoadedWhenPriorConfigurationIsAccessed()
    {
         // Arrange
        var inMemoryCollection = new Dictionary<string, string>
        {
            {"NewConfigSection:ApiEndpoint", OriginalApiEndpoint}
        };

        _configurationBuilder.AddInMemoryCollection(inMemoryCollection);

        // Act
        var response = _configurationBuilder.AddRemote(remote =>
        {
            var apiEndpoint = remote.PriorConfiguration.GetValue<string>("NewConfigSection:ApiEndpoint");
            remote.Options.ApiEndpoint = apiEndpoint;
        });

        // Assert
        response.Sources.Should().ContainSingle(source => source is RemoteConfigurationSource)
            .Which.Should().BeAssignableTo<RemoteConfigurationSource>()
            .Which.ApiEndpoint.Should().Be(OriginalApiEndpoint);
    }

    [Fact]
    public void AddRemoteBuilderBuildShouldBeCalledOnlyOnceWhenPriorConfigurationIsCalledTwice()
    {
        // Arrange
        var mockConfigurationBuilder = new Mock<IConfigurationBuilder>();

        var response1 = mockConfigurationBuilder.Object.AddRemote(remote =>
        {
            var configuration1 = remote.PriorConfiguration;
            var configuration2 = remote.PriorConfiguration;
        });

        // Assert

        mockConfigurationBuilder.Verify(x => x.Build(), Times.Once);
    }
}
