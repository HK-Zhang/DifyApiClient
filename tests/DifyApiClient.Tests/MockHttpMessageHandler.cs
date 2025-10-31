using System.Net;
using System.Text;
using System.Text.Json;

namespace DifyApiClient.Tests;

/// <summary>
/// Mock HTTP message handler for testing
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    private object? _responseContent;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpRequestMessage? LastRequest { get; private set; }

    public MockHttpMessageHandler()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public void SetupResponse(HttpStatusCode statusCode, object? content)
    {
        _statusCode = statusCode;
        _responseContent = content;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;

        var response = new HttpResponseMessage(_statusCode);

        if (_responseContent != null)
        {
            var json = JsonSerializer.Serialize(_responseContent, _jsonOptions);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await Task.FromResult(response);
    }
}
