# Per-Request Timeout Configuration

DifyApiClient supports both global and per-request timeout configuration, allowing you to fine-tune timeout behavior for different operations.

## Global Timeout

Set a default timeout for all requests:

```csharp
var options = new DifyApiClientOptions
{
    BaseUrl = "https://api.dify.ai/v1",
    ApiKey = "your-api-key",
    Timeout = TimeSpan.FromSeconds(100) // Default timeout for all requests
};

using var client = new DifyApiClient(options);
```

## Per-Request Timeout Override

### Why Per-Request Timeouts?

Different operations have different time requirements:
- **Quick operations**: Application info retrieval (< 5 seconds)
- **Standard operations**: Chat messages (30-60 seconds)
- **Long operations**: File uploads, large file processing (2-5 minutes)
- **Streaming**: May need longer timeouts for slow token generation

### Usage

The `timeout` parameter is available on all base HTTP methods in `BaseApiClient`. Services can expose this to their public APIs.

#### Example: Custom Service with Timeout

```csharp
public class CustomChatService : IChatService
{
    private readonly BaseApiClient _apiClient;
    
    public async Task<ChatCompletionResponse> SendChatMessageAsync(
        ChatMessageRequest request,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        // Use custom timeout if provided, otherwise use global HttpClient timeout
        return await _apiClient.PostAsync<ChatMessageRequest, ChatCompletionResponse>(
            "chat-messages",
            request,
            cancellationToken,
            timeout);
    }
}
```

## How It Works

When a timeout is specified:

1. A linked `CancellationTokenSource` is created
2. The timeout is applied via `CancelAfter()`
3. The request uses the combined cancellation token
4. If timeout expires, the request is cancelled with `OperationCanceledException`

```csharp
// Internal implementation in BaseApiClient
if (timeout.HasValue)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    cts.CancelAfter(timeout.Value);
    var response = await HttpClient.SendAsync(request, cts.Token);
    // ...
}
```

## Examples

### Different Timeouts for Different Operations

```csharp
public class ChatService
{
    private readonly IDifyApiClient _client;
    
    public async Task<string> QuickInfoAsync()
    {
        // Quick operation - 5 second timeout
        var info = await _client.GetApplicationInfoAsync();
        return info.Name;
    }
    
    public async Task<ChatCompletionResponse> StandardChatAsync(string message)
    {
        // Standard chat - 30 second timeout
        var response = await _client.SendChatMessageAsync(new ChatMessageRequest
        {
            Query = message,
            User = "user-123"
        });
        return response;
    }
    
    public async Task<FileUploadResponse> UploadLargeFileAsync(Stream file, string fileName)
    {
        // Large file upload - 5 minute timeout
        var response = await _client.UploadFileAsync(file, fileName, "user-123");
        return response;
    }
}
```

### Extending DifyApiClient with Timeout Support

If you want to add timeout parameters to the public API:

```csharp
// Create extension methods
public static class DifyApiClientExtensions
{
    public static async Task<ChatCompletionResponse> SendChatMessageWithTimeoutAsync(
        this IDifyApiClient client,
        ChatMessageRequest request,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        
        return await client.SendChatMessageAsync(request, cts.Token);
    }
}

// Usage
var response = await difyClient.SendChatMessageWithTimeoutAsync(
    request,
    TimeSpan.FromSeconds(15));
```

## Best Practices

### 1. Use Appropriate Timeouts

```csharp
// ❌ Too short - may fail unnecessarily
timeout: TimeSpan.FromSeconds(1)

// ✅ Reasonable for chat
timeout: TimeSpan.FromSeconds(30)

// ✅ Appropriate for file upload
timeout: TimeSpan.FromMinutes(3)
```

### 2. Combine with Retry Policies

```csharp
builder.Services.AddDifyApiClientWithResilience(options => { ... });

// Retry handles transient failures
// Timeout prevents hanging indefinitely
```

### 3. Handle Timeout Exceptions

```csharp
try
{
    var response = await client.SendChatMessageAsync(request);
}
catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
{
    // Timeout occurred (not user cancellation)
    logger.LogWarning("Request timed out");
    return Results.RequestTimeout("The request took too long to complete");
}
catch (OperationCanceledException)
{
    // User cancelled the operation
    logger.LogInformation("Request cancelled by user");
    return Results.NoContent();
}
```

### 4. Progressive Timeouts

```csharp
public async Task<ChatCompletionResponse> SendWithRetryAsync(
    ChatMessageRequest request,
    CancellationToken cancellationToken = default)
{
    var timeouts = new[] { 
        TimeSpan.FromSeconds(15), 
        TimeSpan.FromSeconds(30), 
        TimeSpan.FromSeconds(60) 
    };
    
    for (int i = 0; i < timeouts.Length; i++)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeouts[i]);
            
            return await _client.SendChatMessageAsync(request, cts.Token);
        }
        catch (OperationCanceledException) when (i < timeouts.Length - 1)
        {
            _logger.LogWarning("Attempt {Attempt} timed out after {Timeout}s, retrying...", 
                i + 1, timeouts[i].TotalSeconds);
        }
    }
    
    throw new TimeoutException("All retry attempts timed out");
}
```

### 5. Environment-Specific Timeouts

```csharp
// appsettings.json
{
  "Dify": {
    "Timeouts": {
      "Default": "00:01:40",
      "QuickOperations": "00:00:05",
      "FileUpload": "00:05:00",
      "Streaming": "00:10:00"
    }
  }
}

// Configuration
public class DifyTimeoutConfig
{
    public TimeSpan Default { get; set; } = TimeSpan.FromSeconds(100);
    public TimeSpan QuickOperations { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan FileUpload { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan Streaming { get; set; } = TimeSpan.FromMinutes(10);
}

// Usage
builder.Services.Configure<DifyTimeoutConfig>(
    builder.Configuration.GetSection("Dify:Timeouts"));

public class ChatService
{
    private readonly IDifyApiClient _client;
    private readonly DifyTimeoutConfig _timeouts;
    
    public ChatService(IDifyApiClient client, IOptions<DifyTimeoutConfig> timeouts)
    {
        _client = client;
        _timeouts = timeouts.Value;
    }
    
    public async Task<ChatCompletionResponse> QuickChatAsync(string message)
    {
        using var cts = new CancellationTokenSource(_timeouts.QuickOperations);
        return await _client.SendChatMessageAsync(new ChatMessageRequest
        {
            Query = message,
            User = "user-123"
        }, cts.Token);
    }
}
```

## Integration with OpenTelemetry

Timeout information is automatically included in traces:

```csharp
Activity.Tags:
    http.method: POST
    http.url: chat-messages
    http.timeout_ms: 30000  // Timeout is recorded
    http.status_code: 200
```

This helps identify if timeouts are too aggressive or too lenient.

## Troubleshooting

### Issue: Frequent timeouts

**Symptoms**: Regular `OperationCanceledException` errors

**Solutions**:
1. Increase timeout value
2. Check network latency
3. Verify API endpoint performance
4. Review retry policy configuration

### Issue: Requests hanging indefinitely

**Symptoms**: No timeout exceptions, requests never complete

**Solutions**:
1. Ensure global timeout is set in `DifyApiClientOptions`
2. Add per-request timeout
3. Check for deadlocks (ensure `ConfigureAwait(false)` is used)

### Issue: Timeout vs Cancellation confusion

```csharp
try
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    var response = await client.SendChatMessageAsync(request, cts.Token);
}
catch (OperationCanceledException ex) when (!cts.Token.IsCancellationRequested)
{
    // This should not happen - cancellation token was cancelled
    throw new InvalidOperationException("Unexpected cancellation", ex);
}
catch (OperationCanceledException)
{
    // Expected - timeout occurred
    throw new TimeoutException("Request timed out after 30 seconds");
}
```

## Comparison: Global vs Per-Request Timeout

| Aspect | Global Timeout | Per-Request Timeout |
|--------|---------------|---------------------|
| Configuration | `DifyApiClientOptions.Timeout` | Method parameter |
| Scope | All requests | Single request |
| Override | Cannot be changed after client creation | Can vary per call |
| Use Case | Default safety net | Operation-specific tuning |
| Recommendation | Set to longest reasonable time | Use for specific needs |

## Summary

- ✅ Global timeout provides a safety net for all requests
- ✅ Per-request timeout allows fine-grained control
- ✅ Combine with retry policies for robust error handling
- ✅ Use `OperationCanceledException` to detect timeouts
- ✅ Configure different timeouts for different operation types
- ✅ Monitor timeout metrics in OpenTelemetry

Per-request timeouts give you the flexibility to optimize performance for each type of API operation! 🚀
