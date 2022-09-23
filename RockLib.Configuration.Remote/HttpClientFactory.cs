using System;
using System.Net.Http;

namespace RockLib.Configuration.Remote;

/// <summary>
/// A factory class to create HttpClient instances using an HttpMessageHandler
/// factory.
/// </summary>
public class HttpClientFactory : IHttpClientFactory
{
    private readonly Func<HttpMessageHandler> _httpMessageHandlerFactory;

    /// <summary>
    /// Create an HttpClientFactory instance.
    /// </summary>
    /// <param name="innerHandlerFactory">A factory to create an HttpMessageHandler</param>
    public HttpClientFactory(Func<HttpMessageHandler> innerHandlerFactory)
    {
        _httpMessageHandlerFactory = innerHandlerFactory;
    }

    /// <summary>
    /// Create a new HttpClient using the provided HttpMessageHandler factory.
    /// </summary>
    // <returns>A new HttpClient</returns>
    public HttpClient Create()
    {
        return new HttpClient(_httpMessageHandlerFactory.Invoke());
    }
}
