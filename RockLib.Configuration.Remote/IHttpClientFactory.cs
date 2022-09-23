using System.Net.Http;

namespace RockLib.Configuration.Remote;

/// <summary>
/// A factory to create HttpClient instances.
/// </summary>
public interface IHttpClientFactory
{
    /// <summary>
    /// Create a new HttpClient instance.
    /// </summary>
    /// <returns>A new HttpClient instance.</returns>
    HttpClient Create();
}
