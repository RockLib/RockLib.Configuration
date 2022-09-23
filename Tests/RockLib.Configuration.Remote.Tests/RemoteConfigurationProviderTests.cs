using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using FluentAssertions;
using Moq;
using Xunit;

namespace RockLib.Configuration.Remote.Tests;

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public class RemoteConfigurationProviderTests
{
    private readonly Mock<IConfigurationParser> _mockConfigurationParser;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Func<HttpClient> _httpClient;
    private readonly Mock<IMockHttpMessageHandler> _mockHttpMessageHandler;

    public RemoteConfigurationProviderTests()
    {
        _mockConfigurationParser = new Mock<IConfigurationParser>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpMessageHandler = new Mock<IMockHttpMessageHandler>();
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(_mockHttpMessageHandler.Object);
        _httpClient = () => new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://test@test.com") };
    }

    [Fact]
    public void LoadShouldCreateHttpClientAndParseTheResponseWhenInitialized()
    {
        // Arrange
        const string ResponseString = "Test";
        const string ApiEndpoint = "http://Endpoint";
        var refreshInterval = Timeout.InfiniteTimeSpan;

        _mockHttpClientFactory.Setup(x => x.Create()).Returns(_httpClient);
        _mockHttpMessageHandler.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage { Content = new StringContent(ResponseString) });
        _mockConfigurationParser.Setup(x => x.Parse(ResponseString)).Returns(new Dictionary<string, string> { { ResponseString, ResponseString } });

        // Act
        var remoteConfigurationProvider = new RemoteConfigurationProvider(ApiEndpoint, refreshInterval, _mockConfigurationParser.Object, _mockHttpClientFactory.Object);
        remoteConfigurationProvider.Load();

        // Assert
        _mockHttpClientFactory.Verify(x => x.Create(), Times.Once);
        _mockConfigurationParser.Verify(x => x.Parse(ResponseString), Times.Once);
    }

    [Fact]
    public void LoadItShouldThrowErrorWhenCallToEndPointFailsForSomeReasonForTheFirstTime()
    {
        // Arrange
        const string ApiEndpoint = "http://Endpoint";
        var refreshInterval = Timeout.InfiniteTimeSpan;

        _mockHttpClientFactory.Setup(x => x.Create()).Returns(_httpClient);
        _mockHttpMessageHandler.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RemoteConfigurationException("Something went wrong"));

        // Act
        var remoteConfigurationProvider = new RemoteConfigurationProvider(ApiEndpoint, refreshInterval, _mockConfigurationParser.Object, _mockHttpClientFactory.Object);

        Action action = () =>  remoteConfigurationProvider.Load();

        // Assert
        action.Should().Throw<Exception>().WithMessage("Something went wrong");
        _mockHttpClientFactory.Verify(x => x.Create(), Times.Once);
        _mockConfigurationParser.Verify(x => x.Parse(It.IsAny<string>()), Times.Never);

    }

    [Fact]
    public void LoadShouldThrowExceptionWhenCallToEndPointIsNotSuccessful()
    {
        // Arrange
        const string ResponseString = "Test";
        const string ApiEndpoint = "https://FakeEndpoint";
        var refreshInterval = Timeout.InfiniteTimeSpan;

        _mockHttpClientFactory.Setup(x => x.Create()).Returns(_httpClient);
        _mockHttpMessageHandler.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(ResponseString),
                    StatusCode = HttpStatusCode.InternalServerError
                });

        // Act
        var remoteConfigurationProvider = new RemoteConfigurationProvider(ApiEndpoint, refreshInterval, _mockConfigurationParser.Object, _mockHttpClientFactory.Object);
        Action action = () => remoteConfigurationProvider.Load();

        // Assert
        action.Should().Throw<Exception>().WithMessage("The call to the endpoint https://FakeEndpoint was not successful");
        _mockHttpClientFactory.Verify(x => x.Create(), Times.Once);
        _mockConfigurationParser.Verify(x => x.Parse(It.IsAny<string>()), Times.Never);
    }

    [Fact]

    public void LoadShouldNotThrowExceptionWhenCallToEndPointIsSuccessfulForTheFirstTimeButFailsSecondTime()
    {
        // Arrange
        const string ResponseString = "Test";
        const string ApiEndpoint = "http://Endpoint";
        var refreshInterval = Timeout.InfiniteTimeSpan;

        _mockHttpClientFactory.Setup(x => x.Create()).Returns(_httpClient);
        _mockHttpMessageHandler.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage { Content = new StringContent(ResponseString) });
        _mockConfigurationParser.Setup(x => x.Parse(ResponseString)).Returns(new Dictionary<string, string> { { ResponseString, ResponseString } });

        // Act
        var remoteConfigurationProvider = new RemoteConfigurationProvider(ApiEndpoint, refreshInterval, _mockConfigurationParser.Object, _mockHttpClientFactory.Object);
        remoteConfigurationProvider.Load();

        // second call gets error

        _mockHttpMessageHandler.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RemoteConfigurationException("Something went wrong second time"));

        Action action = () => remoteConfigurationProvider.Load();

        // Assert
        action.Should().NotThrow<Exception>();
        _mockHttpClientFactory.Verify(x => x.Create(), Times.Exactly(2));
        _mockConfigurationParser.Verify(x => x.Parse(ResponseString), Times.Once);
    }
}
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
#pragma warning restore CA2000 // Dispose objects before losing scope
