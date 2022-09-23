using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration.Remote;

/// <summary>
/// A configuration source that gets data from a remote endpoint.
/// </summary>
public class RemoteConfigurationSource : IConfigurationSource
{
    private Func<HttpMessageHandler> _httpMessageHandlerFactory = () => new HttpClientHandler();

    /// <summary>
    /// The remote API endpoint that holds a configuration response.
    /// </summary>
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// The interval to refresh configuration by calling the remote endpoint.
    /// </summary>
    public TimeSpan RefreshInterval { get; set; }

    /// <summary>
    /// The name of the configuration section to inject remote configuration into.
    /// </summary>
    public string? Section { get; set; }

    /// <summary>
    /// Decorate the HttpMessageHandler used to retrieve configuration from the
    /// remote endpoint.
    /// </summary>
    /// <param name="decorator">A decorator function to wrap the existing handler</param>
    /// <returns>This RemoteConfigurationSource with a decorated HttpMessageHandler</returns>
    public RemoteConfigurationSource DecorateHttpMessageHandler(Func<HttpMessageHandler, HttpMessageHandler> decorator)
    {
        var originalFactory = _httpMessageHandlerFactory;
        _httpMessageHandlerFactory = () => decorator.Invoke(originalFactory.Invoke());
        return this;
    }

    /// <summary>
    /// Build the RemoteConfigurationProvider for this source.
    /// </summary>
    /// <param name="builder">The IConfigurationBuilder where this source was added</param>
    /// <returns>A new RemoteConfigurationProvider</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (Section is null)
        {
            throw new RemoteConfigurationException($"{nameof(Section)} cannot be null.");
        }

        if (ApiEndpoint is null)
        {
            throw new RemoteConfigurationException($"{nameof(ApiEndpoint)} cannot be null.");
        }

        var configurationParser = new JsonConfigurationParser(Section);
        var httpClientFactory = new HttpClientFactory(_httpMessageHandlerFactory);
        return new RemoteConfigurationProvider(ApiEndpoint, RefreshInterval, configurationParser, httpClientFactory);
    }
}
