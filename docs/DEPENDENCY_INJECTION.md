# Dependency Injection Guide

DifyApiClient provides comprehensive support for dependency injection using `IHttpClientFactory` and built-in extension methods for easy setup.

## Table of Contents

- [Quick Start](#quick-start)
- [Basic Setup](#basic-setup)
- [With Resilience Policies](#with-resilience-policies)
- [Advanced Configuration](#advanced-configuration)
- [ASP.NET Core Integration](#aspnet-core-integration)
- [Best Practices](#best-practices)

## Quick Start

```csharp
using DifyApiClient.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add DifyApiClient with configuration
builder.Services.AddDifyApiClient(options =>
{
    options.BaseUrl = builder.Configuration["Dify:BaseUrl"]!;
    options.ApiKey = builder.Configuration["Dify:ApiKey"]!;
    options.Timeout = TimeSpan.FromSeconds(100);
});

var app = builder.Build();

// Inject and use
app.MapGet("/chat", async (IDifyApiClient difyClient) =>
{
    var response = await difyClient.SendChatMessageAsync(new ChatMessageRequest
    {
        Query = "Hello!",
        User = "user-123"
    });
    
    return Results.Ok(response);
});
```

## Basic Setup

### Console Application

```csharp
using Microsoft.Extensions.DependencyInjection;
using DifyApiClient.Extensions;

var services = new ServiceCollection();

// Add DifyApiClient
services.AddDifyApiClient(options =>
{
    options.BaseUrl = "https://api.dify.ai/v1";
    options.ApiKey = Environment.GetEnvironmentVariable("DIFY_API_KEY")!;
});

// Add logging (optional but recommended)
services.AddLogging(builder => builder.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Resolve and use
var difyClient = serviceProvider.GetRequiredService<IDifyApiClient>();
var response = await difyClient.SendChatMessageAsync(new ChatMessageRequest
{
    Query = "Hello!",
    User = "user-123"
});
```

### Using Pre-configured Options

```csharp
var options = new DifyApiClientOptions
{
    BaseUrl = "https://api.dify.ai/v1",
    ApiKey = "your-api-key",
    Timeout = TimeSpan.FromSeconds(120)
};

services.AddDifyApiClient(options);
```

## With Resilience Policies

Add automatic retry, circuit breaker, and timeout policies:

```csharp
services.AddDifyApiClientWithResilience(options =>
{
    options.BaseUrl = configuration["Dify:BaseUrl"]!;
    options.ApiKey = configuration["Dify:ApiKey"]!;
});
```

This adds:
- **Retry Policy**: 3 retries with exponential backoff
- **Circuit Breaker**: Opens after 5 consecutive failures for 30 seconds
- **Handles**: 5xx errors, network failures, 429 Too Many Requests

### Custom Resilience Configuration

```csharp
services.AddDifyApiClientWithResilience(
    options =>
    {
        options.BaseUrl = configuration["Dify:BaseUrl"]!;
        options.ApiKey = configuration["Dify:ApiKey"]!;
    },
    httpClientBuilder =>
    {
        // Add custom policies
        httpClientBuilder.AddTimeoutPolicy(timeoutSeconds: 60);
    });
```

## Advanced Configuration

### Custom HttpClient Configuration

```csharp
var httpClientBuilder = services.AddDifyApiClient(options =>
{
    options.BaseUrl = "https://api.dify.ai/v1";
    options.ApiKey = "your-api-key";
});

// Add custom message handler
httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
});

// Add custom delegating handler for logging
httpClientBuilder.AddHttpMessageHandler<LoggingDelegatingHandler>();
```

### Multiple DifyApiClient Instances

If you need multiple clients (e.g., different API keys or environments):

```csharp
// Production client
services.AddSingleton<DifyApiClientOptions>(sp => new DifyApiClientOptions
{
    BaseUrl = configuration["Dify:Production:BaseUrl"]!,
    ApiKey = configuration["Dify:Production:ApiKey"]!
});

services.AddHttpClient<IDifyApiClient, DifyApiClient>();

// Staging client (named)
services.AddHttpClient<DifyApiClient>("DifyStaging")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());

services.AddSingleton<IDifyApiClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient("DifyStaging");
    var options = new DifyApiClientOptions
    {
        BaseUrl = configuration["Dify:Staging:BaseUrl"]!,
        ApiKey = configuration["Dify:Staging:ApiKey"]!
    };
    var logger = sp.GetService<ILogger<DifyApiClient>>();
    return new DifyApiClient(options, httpClient, disposeHttpClient: false, logger);
});
```

## ASP.NET Core Integration

### appsettings.json

```json
{
  "Dify": {
    "BaseUrl": "https://api.dify.ai/v1",
    "ApiKey": "your-api-key-here",
    "Timeout": "00:01:40"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "DifyApiClient": "Debug"
    }
  }
}
```

### Program.cs

```csharp
using DifyApiClient.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add DifyApiClient
builder.Services.AddDifyApiClientWithResilience(options =>
{
    var config = builder.Configuration.GetSection("Dify");
    options.BaseUrl = config["BaseUrl"]!;
    options.ApiKey = config["ApiKey"]!;
    
    if (TimeSpan.TryParse(config["Timeout"], out var timeout))
    {
        options.Timeout = timeout;
    }
});

var app = builder.Build();
```

### Controller Usage

```csharp
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IDifyApiClient _difyClient;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IDifyApiClient difyClient, ILogger<ChatController> logger)
    {
        _difyClient = difyClient;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            var response = await _difyClient.SendChatMessageAsync(new ChatMessageRequest
            {
                Query = request.Message,
                User = request.UserId,
                ConversationId = request.ConversationId
            });

            return Ok(response);
        }
        catch (DifyApiException ex)
        {
            _logger.LogError(ex, "Dify API error: {StatusCode}", ex.StatusCode);
            return StatusCode(ex.StatusCode ?? 500, new { error = ex.Message });
        }
    }

    [HttpGet("stream")]
    public async IAsyncEnumerable<string> StreamMessages([FromQuery] string message, [FromQuery] string userId)
    {
        await foreach (var chunk in _difyClient.SendChatMessageStreamAsync(new ChatMessageRequest
        {
            Query = message,
            User = userId
        }))
        {
            if (chunk.Event == "message" && chunk.Answer != null)
            {
                yield return chunk.Answer;
            }
        }
    }
}
```

### Minimal API Usage

```csharp
app.MapPost("/api/chat", async (
    ChatRequest request,
    IDifyApiClient difyClient,
    CancellationToken cancellationToken) =>
{
    var response = await difyClient.SendChatMessageAsync(new ChatMessageRequest
    {
        Query = request.Message,
        User = request.UserId
    }, cancellationToken);

    return Results.Ok(response);
});
```

## Best Practices

### 1. Always Use IHttpClientFactory

✅ **Good**: Use dependency injection with `AddDifyApiClient()`
```csharp
services.AddDifyApiClient(options => { ... });
```

❌ **Bad**: Create new HttpClient instances manually
```csharp
var client = new DifyApiClient(options, new HttpClient(), disposeHttpClient: true);
```

### 2. Configure Resilience for Production

```csharp
// Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDifyApiClient(options => { ... });
}
// Production
else
{
    builder.Services.AddDifyApiClientWithResilience(options => { ... });
}
```

### 3. Use Configuration Instead of Hardcoded Values

```csharp
// Use configuration
services.AddDifyApiClient(options =>
{
    options.BaseUrl = configuration["Dify:BaseUrl"]!;
    options.ApiKey = configuration["Dify:ApiKey"]!;
});
```

### 4. Enable Logging

```csharp
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.AddFilter("DifyApiClient", LogLevel.Information);
});
```

### 5. Use Scoped or Transient Lifetime

The client is registered as `Transient` by default, which is appropriate for most scenarios:

```csharp
// Default: Transient (new instance per request)
services.AddDifyApiClient(options => { ... });

// If you need singleton (use with caution)
services.AddSingleton<IDifyApiClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient(typeof(DifyApiClient).FullName!);
    var options = sp.GetRequiredService<DifyApiClientOptions>();
    var logger = sp.GetService<ILogger<DifyApiClient>>();
    return new DifyApiClient(options, httpClient, disposeHttpClient: false, logger);
});
```

### 6. Handle Exceptions Properly

```csharp
try
{
    var response = await difyClient.SendChatMessageAsync(request);
    return response;
}
catch (DifyApiException ex) when (ex.StatusCode == 429)
{
    // Rate limited - wait and retry
    logger.LogWarning("Rate limited, retrying after delay");
    await Task.Delay(TimeSpan.FromSeconds(5));
    // Retry logic
}
catch (DifyApiException ex)
{
    logger.LogError(ex, "Dify API error: {Message}", ex.Message);
    throw;
}
```

## Testing

### Mocking for Unit Tests

```csharp
[Fact]
public async Task SendMessage_ReturnsResponse()
{
    // Arrange
    var mockClient = new Mock<IDifyApiClient>();
    mockClient
        .Setup(x => x.SendChatMessageAsync(It.IsAny<ChatMessageRequest>(), default))
        .ReturnsAsync(new ChatCompletionResponse
        {
            Answer = "Test response",
            MessageId = "msg-123"
        });

    var controller = new ChatController(mockClient.Object, Mock.Of<ILogger<ChatController>>());

    // Act
    var result = await controller.SendMessage(new ChatRequest { Message = "Test" });

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    var response = Assert.IsType<ChatCompletionResponse>(okResult.Value);
    Assert.Equal("Test response", response.Answer);
}
```

### Integration Testing

```csharp
public class DifyClientIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient;

    public DifyClientIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ChatEndpoint_ReturnsValidResponse()
    {
        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/chat", new
        {
            Message = "Hello",
            UserId = "test-user"
        });

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();
        Assert.NotNull(content);
    }
}
```

## Troubleshooting

### Issue: "No service for type 'IDifyApiClient' has been registered"

**Solution**: Ensure you've called `AddDifyApiClient()` in your service configuration:
```csharp
builder.Services.AddDifyApiClient(options => { ... });
```

### Issue: Socket exhaustion

**Solution**: Always use the DI extension methods instead of creating `new HttpClient()` instances.

### Issue: Configuration values are null

**Solution**: Check your appsettings.json and ensure the configuration section exists and is properly bound:
```csharp
var config = builder.Configuration.GetSection("Dify");
if (!config.Exists())
{
    throw new InvalidOperationException("Dify configuration section is missing");
}
```

## Migration from Direct Instantiation

**Before**:
```csharp
var options = new DifyApiClientOptions { BaseUrl = "...", ApiKey = "..." };
using var client = new DifyApiClient(options);
var response = await client.SendChatMessageAsync(request);
```

**After**:
```csharp
// In Startup/Program.cs
services.AddDifyApiClient(options =>
{
    options.BaseUrl = configuration["Dify:BaseUrl"]!;
    options.ApiKey = configuration["Dify:ApiKey"]!;
});

// In your service/controller
public class MyService
{
    private readonly IDifyApiClient _difyClient;
    
    public MyService(IDifyApiClient difyClient)
    {
        _difyClient = difyClient;
    }
    
    public async Task DoSomething()
    {
        var response = await _difyClient.SendChatMessageAsync(request);
    }
}
```
