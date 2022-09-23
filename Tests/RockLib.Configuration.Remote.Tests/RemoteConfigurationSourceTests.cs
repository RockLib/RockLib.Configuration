using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace RockLib.Configuration.Remote.Tests;

#pragma warning disable CA2000 // Dispose objects before losing scope
public class RemoteConfigurationSourceTests
{
    private const string OriginalApiEndpoint = "http://OriginalApiEndpoint.com";

    private readonly ConfigurationBuilder _configurationBuilder;
    public RemoteConfigurationSourceTests()
    {
        _configurationBuilder = new ConfigurationBuilder();
    }

    [Fact]
    public void RemoteConfigurationSourceShouldCallTheEndpointOnceWhenBuilt()
    {
        var remoteConfigurationSource = new RemoteConfigurationSource
        {
            Section = "Foo",
            ApiEndpoint = OriginalApiEndpoint,
            RefreshInterval = Timeout.InfiniteTimeSpan
        };

        var mockHttpMessageHandler = new Mock<IMockHttpMessageHandler>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(mockHttpMessageHandler.Object);

        var configurationSource = remoteConfigurationSource.DecorateHttpMessageHandler(handler => new FakeHttpMessageHandler(mockHttpMessageHandler.Object));
        mockHttpMessageHandler.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(GetHttpResponseMessageWithAuthResponse());
        var provider = remoteConfigurationSource.Build(_configurationBuilder);
        provider.Load();

        provider.Should().BeAssignableTo<RemoteConfigurationProvider>();
        mockHttpMessageHandler.Verify(mock => mock.SendAsync(RequestWithEndpoint(OriginalApiEndpoint), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static HttpRequestMessage RequestWithEndpoint(string endpoint)
    {
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        return It.Is<HttpRequestMessage>(arg => arg.RequestUri.OriginalString == endpoint);
        #pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    private static HttpResponseMessage GetHttpResponseMessageWithAuthResponse()
    {
        return new HttpResponseMessage { Content = new StringContent("{}") };
    }
}
#pragma warning restore CA2000 // Dispose objects before losing scope
