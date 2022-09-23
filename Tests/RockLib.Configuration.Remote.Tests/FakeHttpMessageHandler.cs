using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Configuration.Remote.Tests;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly IMockHttpMessageHandler _mockHandler;

    public FakeHttpMessageHandler(IMockHttpMessageHandler mockHttpMessageHandler)
    {
        _mockHandler = mockHttpMessageHandler;
    }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _mockHandler.SendAsync(request, cancellationToken);
    }
}
