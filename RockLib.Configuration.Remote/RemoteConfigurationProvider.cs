using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration.Remote;

/// <summary>
/// A ConfigurationProvider that gets configuration from a remote endpoint.
/// </summary>
public class RemoteConfigurationProvider : ConfigurationProvider, IDisposable
{
    private bool _disposed = false;

    private bool hasLoadedOnce = false;
    private readonly string _apiEndpoint;
    private readonly IConfigurationParser _configurationParser;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Timer _timer;

    /// <summary>
    /// Create a RemoteConfigurationProvider instance.
    /// </summary>
    /// <param name="apiEndpoint">The API endpoint to fetch configuration from</param>
    /// <param name="refreshInterval">The interval to call the endpoint and reload configuration</param>
    /// <param name="configurationParser">A parser to convert the endpoint response into a configuration Dictionary</param>
    /// <param name="httpClientFactory">A factory to create an HttpClient to call the endpoint</param>
    public RemoteConfigurationProvider(string apiEndpoint, TimeSpan refreshInterval, IConfigurationParser configurationParser, IHttpClientFactory httpClientFactory)
    {
        _apiEndpoint = apiEndpoint;
        _configurationParser = configurationParser;
        _httpClientFactory = httpClientFactory;

        _timer = new Timer(_ => Load(), null, refreshInterval, refreshInterval);
    }

    /// <summary>
    /// Load the configuration data for this provider from a remote endpoint.
    /// </summary>
    public override void Load()
    {
        try
        {
            using var httpClient = _httpClientFactory.Create();
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, _apiEndpoint);
            var response = httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                Data = _configurationParser.Parse(content);
                OnReload();
                hasLoadedOnce = true;
            }
            else
                throw new RemoteConfigurationException($"The call to the endpoint {_apiEndpoint} was not successful");
        }
        catch (Exception ex)
        {
            // Do nothing for now
            Console.WriteLine($"There was an error calling endpoint {_apiEndpoint}", ex.StackTrace);
            if (hasLoadedOnce == false)
                throw;
        }
    }

    /// <summary>
    /// Stop the refresh timer and release resources
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _timer?.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Stop the refresh timer and release resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
