using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace RockLib.Configuration.Remote.Tests;

public class HttpClientFactoryTests
{
    [Fact]
    public void CreateBaseAddressShouldBeNullWhenInitializedWithDefaults()
    {
        // Arrange
        var httpClientFactory = new HttpClientFactory(() => new HttpClientHandler());

        // Act
        var httpClient = httpClientFactory.Create();

        // Assert
        httpClient.Should().BeAssignableTo<HttpClient>()
            .Which.BaseAddress.Should().BeNull();
    }

    [Fact]
    public async Task CreateThenExpectedResponseShouldMatchWhenInitializedWithBaseAddressAndCallSendAsync()
    {
        // Arrange
        var expectedResponseString = "Response";
        using var requestMessage = new HttpRequestMessage();
        var mockHttpMessageHandler = new Mock<IMockHttpMessageHandler>();
        using var fakeHttpMessageHandler = new FakeHttpMessageHandler(mockHttpMessageHandler.Object);
        var httpClientFactory = new HttpClientFactory(() => fakeHttpMessageHandler);
        using var responseMessage = new HttpResponseMessage { Content = new StringContent(expectedResponseString) };
        mockHttpMessageHandler.Setup(x => x.SendAsync(requestMessage, It.IsAny<CancellationToken>())).ReturnsAsync(responseMessage);

        // Act
        using var httpClient = httpClientFactory.Create();
        httpClient.BaseAddress = new Uri("http://test@test.com");
        var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        // Assert
        responseString.Should().Be(expectedResponseString);
    }
}
