# Logging in DifyApiClient

The DifyApiClient library includes comprehensive logging support using `Microsoft.Extensions.Logging.Abstractions`, making it easy to integrate with any logging framework in the .NET ecosystem.

## Features

- **Structured logging** for all HTTP requests and responses
- **Operation-level logging** for key business operations
- **Error logging** with detailed error information
- **Debug-level logging** for HTTP request/response details
- **Optional** - logging is completely optional and backward compatible

## Quick Start

### Basic Usage (No Logging)

If you don't need logging, the client works exactly as before:

```csharp
var options = new DifyApiClientOptions
{
    BaseUrl = "https://api.dify.ai/v1",
    ApiKey = "your-api-key"
};

using var client = new DifyApiClient(options);
```

### With Console Logging

To enable console logging:

```csharp
using Microsoft.Extensions.Logging;

// Create a logger factory
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddConsole();
});

var logger = loggerFactory.CreateLogger<DifyApiClient>();

// Create client with logger
var options = new DifyApiClientOptions
{
    BaseUrl = "https://api.dify.ai/v1",
    ApiKey = "your-api-key"
};

using var client = new DifyApiClient(options, logger);
```

### With ASP.NET Core Dependency Injection

In ASP.NET Core applications, you can use built-in DI and logging:

```csharp
// In Program.cs or Startup.cs
services.AddSingleton<DifyApiClientOptions>(new DifyApiClientOptions
{
    BaseUrl = configuration["DifyApi:BaseUrl"],
    ApiKey = configuration["DifyApi:ApiKey"]
});

services.AddScoped(sp =>
{
    var options = sp.GetRequiredService<DifyApiClientOptions>();
    var logger = sp.GetRequiredService<ILogger<DifyApiClient>>();
    return new DifyApiClient(options, logger);
});
```

### With Serilog

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/dify-client-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSerilog();
});

var logger = loggerFactory.CreateLogger<DifyApiClient>();
var client = new DifyApiClient(options, logger);
```

## Log Levels

### Information Level
- Client initialization
- Successful operation completion
- High-level operation summaries

Example:
```
info: DifyApiClient[0]
      DifyApiClient initialized with base URL: https://api.dify.ai/v1
info: DifyApiClient[0]
      Sending chat message in blocking mode
info: DifyApiClient[0]
      Chat message completed successfully
```

### Debug Level
- HTTP request URLs
- Request/response details
- Streaming chunk counts

Example:
```
dbug: DifyApiClient[0]
      POST request to chat-messages
dbug: DifyApiClient[0]
      POST request to chat-messages completed successfully
```

### Error Level
- HTTP errors with status codes
- API errors with response bodies
- Exception details

Example:
```
fail: DifyApiClient[0]
      HTTP request failed with status 400 (Bad Request). Response: {"error":"Invalid input"}
```

## Filtering Logs

You can filter logs to show only DifyApiClient logs:

```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddConsole()
        .AddFilter("DifyApiClient", LogLevel.Debug); // Show debug logs for DifyApiClient
});
```

## Logged Operations

The following operations are logged:

### Chat Operations
- `SendChatMessageAsync` - Blocking chat message
- `SendChatMessageStreamAsync` - Streaming chat message (includes chunk count)
- `StopGenerationAsync` - Stop message generation

### File Operations
- `UploadFileAsync` - File upload with filename and file ID

### Conversation Operations
- All conversation operations log at Debug level via BaseApiClient

### Audio Operations
- `SpeechToTextAsync` - Speech to text conversion with file name
- `TextToAudioAsync` - Text to audio conversion

### Application Info Operations
- All operations log at Debug level via BaseApiClient

### Message Operations
- All operations log at Debug level via BaseApiClient

### Annotation Operations
- All operations log at Debug level via BaseApiClient

### Feedback Operations
- All operations log at Debug level via BaseApiClient

## Performance Considerations

Logging is designed to be lightweight:
- Uses structured logging (not string concatenation)
- Minimal overhead when disabled
- Only critical information at higher log levels
- Detailed information only at Debug level

## Troubleshooting

### Enable Verbose Logging

To troubleshoot issues, enable Debug or Trace level logging:

```csharp
builder.SetMinimumLevel(LogLevel.Debug);
```

### Common Log Messages

| Message | Level | Meaning |
|---------|-------|---------|
| "DifyApiClient initialized with base URL: {BaseUrl}" | Information | Client created successfully |
| "POST request to {Url}" | Debug | HTTP request initiated |
| "POST request to {Url} completed successfully" | Debug | HTTP request succeeded |
| "HTTP request failed with status {StatusCode}" | Error | API returned error |
| "Sending chat message in blocking mode" | Information | Chat operation started |
| "Chat message completed successfully" | Information | Chat operation succeeded |

## Best Practices

1. **Production**: Use Information level or higher
2. **Development**: Use Debug level for detailed diagnostics
3. **Troubleshooting**: Use Debug or Trace level temporarily
4. **Sensitive Data**: API keys are never logged
5. **Performance**: Disable Debug logging in production for better performance
6. **Structured Logging**: Use a structured logging sink (Serilog, NLog) for better analysis

## Example: Complete Logging Setup

```csharp
using Microsoft.Extensions.Logging;
using DifyApiClient;

// Create logger factory with multiple sinks
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddConsole(options =>
        {
            options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
        })
        .AddDebug()
        .AddEventLog() // Windows Event Log
        .AddFilter("DifyApiClient", LogLevel.Debug);
});

var logger = loggerFactory.CreateLogger<DifyApiClient>();

var options = new DifyApiClientOptions
{
    BaseUrl = "https://api.dify.ai/v1",
    ApiKey = Environment.GetEnvironmentVariable("DIFY_API_KEY")
};

using var client = new DifyApiClient(options, logger);

// All operations are now logged
var response = await client.SendChatMessageAsync(new ChatMessageRequest
{
    Query = "Hello",
    User = "user-123"
});
```

## Migration Guide

Existing code continues to work without any changes:

**Before (still works):**
```csharp
using var client = new DifyApiClient(options);
```

**After (with logging):**
```csharp
using var client = new DifyApiClient(options, logger);
```

The logging parameter is optional, ensuring full backward compatibility.
