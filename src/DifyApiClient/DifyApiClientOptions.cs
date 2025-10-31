namespace DifyApiClient;

/// <summary>
/// Options for configuring the Dify API client
/// </summary>
public class DifyApiClientOptions
{
    /// <summary>
    /// Base URL of the Dify API (e.g., "http://osl4243/v1")
    /// </summary>
    public required string BaseUrl { get; set; }

    /// <summary>
    /// API Key for authentication
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Timeout for HTTP requests (default: 100 seconds)
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
}
