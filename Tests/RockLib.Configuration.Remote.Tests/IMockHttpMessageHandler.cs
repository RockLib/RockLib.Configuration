using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Configuration.Remote.Tests;

public interface IMockHttpMessageHandler
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken);
}
