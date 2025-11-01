# Dify API Client

[![Build and Test](https://github.com/HK-Zhang/DifyApiClient/actions/workflows/build-test.yml/badge.svg)](https://github.com/HK-Zhang/DifyApiClient/actions/workflows/build-test.yml)
[![Publish to NuGet](https://github.com/HK-Zhang/DifyApiClient/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/HK-Zhang/DifyApiClient/actions/workflows/publish-nuget.yml)
[![NuGet](https://img.shields.io/nuget/v/DifyApiClient.svg)](https://www.nuget.org/packages/DifyApiClient/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive .NET client library for the [Dify](https://dify.ai/) Chat Application API.

## Features

✅ **Complete API Coverage**
- Chat operations (blocking & streaming)
- File upload
- Conversation management
- Message feedback
- Audio operations (speech-to-text, text-to-audio)
- Application information
- Annotations management
- Feedbacks retrieval

✅ **Modern C# Implementation**
- Async/await pattern with ConfigureAwait(false)
- IAsyncEnumerable for streaming
- Nullable reference types
- Strong typing with models

✅ **Production Ready**
- Proper error handling with DifyApiException
- Cancellation token support
- IDisposable implementation
- Comprehensive unit tests
- **Built-in logging support** (Microsoft.Extensions.Logging)
- **ConfigureAwait(false)** on all async calls (prevents deadlocks)
- **Per-request timeout override** (fine-grained timeout control)

✅ **Observability**
- **OpenTelemetry integration** (distributed tracing & metrics)
- Activity tracking for all HTTP operations
- Metrics for request count, duration, errors, and active requests
- Streaming operation and chunk metrics
- File upload size tracking
- Structured logging for all operations
- HTTP request/response logging
- Error logging with detailed context
- Optional and backward compatible

✅ **Dependency Injection**
- **IHttpClientFactory integration** (prevents socket exhaustion)
- Extension methods for easy DI setup
- Scoped/Transient lifetime support
- Compatible with ASP.NET Core

✅ **Resilience & Reliability**
- **Polly integration** for retry policies
- Circuit breaker pattern
- Exponential backoff
- Transient fault handling
- Rate limit handling (429 responses)

✅ **Multi-Targeting**
- .NET 8.0
- .NET 9.0

✅ **Quality**
- XML documentation
- Deterministic builds
- Source Link support
- Strong typing

## Installation

### NuGet Package Manager

```bash
dotnet add package DifyApiClient
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package DifyApiClient
```

### From Source

```bash
# Clone the repository
git clone https://github.com/HK-Zhang/DifyApiClient.git

# Build the project
dotnet build

# Run tests
dotnet test
```

## Quick Start

### Basic Usage

```csharp
using DifyApiClient;
using DifyApiClient.Models;

// Initialize the client
var options = new DifyApiClientOptions
{
    BaseUrl = "http://localhost/v1",
    ApiKey = "your-api-key"
};

using var client = new DifyApiClient(options);

// Send a chat message
var request = new ChatMessageRequest
{
    Query = "Hello, how are you?",
    User = "user-123"
};

var response = await client.SendChatMessageAsync(request);
Console.WriteLine(response.Answer);
```

### With Dependency Injection (Recommended)

```csharp
using DifyApiClient.Extensions;

// In Program.cs or Startup.cs
builder.Services.AddDifyApiClientWithResilience(options =>
{
    options.BaseUrl = builder.Configuration["Dify:BaseUrl"]!;
    options.ApiKey = builder.Configuration["Dify:ApiKey"]!;
});

// In your service or controller
public class ChatService
{
    private readonly IDifyApiClient _difyClient;
    
    public ChatService(IDifyApiClient difyClient)
    {
        _difyClient = difyClient;
    }
    
    public async Task<string> SendMessage(string message)
    {
        var response = await _difyClient.SendChatMessageAsync(new ChatMessageRequest
        {
            Query = message,
            User = "user-123"
        });
        return response.Answer;
    }
}
```

**Benefits of DI approach:**
- ✅ Uses IHttpClientFactory (prevents socket exhaustion)
- ✅ Automatic retry and circuit breaker policies
- ✅ Built-in logging
- ✅ Easier testing

## Documentation

- [OpenTelemetry Guide](docs/OPENTELEMETRY.md) - Distributed tracing and metrics instrumentation
- [Timeout Configuration](docs/TIMEOUT_CONFIGURATION.md) - Global and per-request timeout configuration
- [Dependency Injection Guide](docs/DEPENDENCY_INJECTION.md) - Complete DI setup with IHttpClientFactory
- [Resilience Guide](docs/RESILIENCE.md) - Retry policies, circuit breakers, and fault handling
- [Logging Guide](docs/LOGGING.md) - Comprehensive logging documentation
- [Changelog](docs/CHANGELOG.md) - Version history and release notes
- [Setup Summary](docs/SETUP_SUMMARY.md) - Project setup information
- [GitHub Actions Summary](docs/GITHUB_ACTIONS_SUMMARY.md) - CI/CD pipeline details
- [Publication Checklist](docs/PUBLICATION_CHECKLIST.md) - Release checklist
- [Documentation Index](docs/DOCUMENTATION_INDEX.md) - Complete documentation index

## Usage Examples

### Streaming Chat

```csharp
var request = new ChatMessageRequest
{
    Query = "Tell me a story",
    User = "user-123"
};

await foreach (var chunk in client.SendChatMessageStreamAsync(request))
{
    if (chunk.Event == "message" && chunk.Answer != null)
    {
        Console.Write(chunk.Answer);
    }
    else if (chunk.Event == "message_end")
    {
        Console.WriteLine("\n\nDone!");
        break;
    }
}
```

### File Upload

```csharp
using var fileStream = File.OpenRead("document.pdf");
var fileInfo = await client.UploadFileAsync(
    fileStream, 
    "document.pdf", 
    "user-123"
);

Console.WriteLine($"Uploaded file: {fileInfo.Id}");
```

### Logging Support

Enable comprehensive logging for observability and debugging:

```csharp
using Microsoft.Extensions.Logging;

// Create a logger factory
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddConsole()
        .AddFilter("DifyApiClient", LogLevel.Debug);
});

var logger = loggerFactory.CreateLogger<DifyApiClient>();

// Create client with logger
var options = new DifyApiClientOptions
{
    BaseUrl = "http://localhost/v1",
    ApiKey = "your-api-key"
};

using var client = new DifyApiClient(options, logger);

// All operations are now logged
var response = await client.SendChatMessageAsync(request);
```

**See [LOGGING.md](docs/LOGGING.md) for detailed logging documentation.**

### OpenTelemetry Integration

DifyApiClient includes built-in OpenTelemetry support for distributed tracing and metrics:

```csharp
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using DifyApiClient.Extensions;

// In Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerBuilder =>
    {
        tracerBuilder
            .AddDifyApiClientInstrumentation()
            .AddConsoleExporter();
    })
    .WithMetrics(meterBuilder =>
    {
        meterBuilder
            .AddDifyApiClientInstrumentation()
            .AddConsoleExporter();
    });
```

**Available Metrics:**
- `difyapiclient.requests.count` - Total request count
- `difyapiclient.requests.duration` - Request duration (ms)
- `difyapiclient.requests.errors` - Failed requests
- `difyapiclient.requests.active` - Concurrent requests
- `difyapiclient.streaming.operations` - Streaming operations
- `difyapiclient.streaming.chunks` - Chunks received
- `difyapiclient.files.upload_size` - File upload sizes (bytes)

**See [OPENTELEMETRY.md](docs/OPENTELEMETRY.md) for complete documentation.**

### Per-Request Timeout Override

Configure different timeouts for different operations:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

// Quick operation - 30 second timeout
var response = await client.SendChatMessageAsync(request, cts.Token);
```

Or extend the client with timeout parameters:

```csharp
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

// Usage
var response = await client.SendChatMessageWithTimeoutAsync(
    request, 
    TimeSpan.FromSeconds(15));
```

**See [TIMEOUT_CONFIGURATION.md](docs/TIMEOUT_CONFIGURATION.md) for detailed timeout configuration.**

### Conversation Management

```csharp
// Get conversations
var conversations = await client.GetConversationsAsync("user-123");

foreach (var conv in conversations.Data)
{
    Console.WriteLine($"{conv.Id}: {conv.Name}");
}

// Get messages in a conversation
var messages = await client.GetConversationMessagesAsync(
    conversationId: "conv-123",
    user: "user-123"
);

// Delete a conversation
await client.DeleteConversationAsync("conv-123", "user-123");
```

### Annotations

```csharp
// Create an annotation
var annotation = await client.CreateAnnotationAsync(new AnnotationRequest
{
    Question = "What is your name?",
    Answer = "I am Dify"
});

// Get all annotations
var annotations = await client.GetAnnotationsAsync(page: 1, limit: 20);

// Update an annotation
await client.UpdateAnnotationAsync(
    annotation.Id!, 
    new AnnotationRequest
    {
        Question = "What is your name?",
        Answer = "I am Dify AI Assistant"
    }
);

// Delete an annotation
await client.DeleteAnnotationAsync(annotation.Id!);
```

### Application Information

```csharp
// Get basic info
var info = await client.GetApplicationInfoAsync();
Console.WriteLine($"App: {info.Name}");

// Get parameters
var parameters = await client.GetApplicationParametersAsync();
Console.WriteLine($"Opening: {parameters.OpeningStatement}");

// Get meta information
var meta = await client.GetApplicationMetaAsync();

// Get WebApp settings
var site = await client.GetApplicationSiteAsync();
Console.WriteLine($"Theme: {site.ChatColorTheme}");
```

### Audio Operations

```csharp
// Speech to text
using var audioStream = File.OpenRead("audio.wav");
var text = await client.SpeechToTextAsync(audioStream, "audio.wav", "user-123");
Console.WriteLine($"Transcribed: {text}");

// Text to audio
var request = new TextToAudioRequest
{
    MessageId = "msg-123",
    Text = "Hello world",
    User = "user-123"
};

using var audioResponse = await client.TextToAudioAsync(request);
using var output = File.Create("output.wav");
await audioResponse.CopyToAsync(output);
```

## API Reference

### DifyApiClient Methods

#### Chat Operations
- `SendChatMessageAsync(request)` - Send message in blocking mode
- `SendChatMessageStreamAsync(request)` - Send message in streaming mode
- `StopGenerationAsync(taskId, user)` - Stop message generation

#### File Operations
- `UploadFileAsync(stream, fileName, user)` - Upload a file

#### Conversation Operations
- `GetConversationsAsync(user, lastId, limit, pinned)` - List conversations
- `GetConversationMessagesAsync(conversationId, user, firstId, limit)` - Get messages
- `DeleteConversationAsync(conversationId, user)` - Delete conversation
- `RenameConversationAsync(conversationId, request)` - Rename conversation
- `GetConversationVariablesAsync(conversationId, user)` - Get variables

#### Message Operations
- `SubmitMessageFeedbackAsync(messageId, request)` - Submit feedback
- `GetSuggestedQuestionsAsync(messageId, user)` - Get suggestions

#### Audio Operations
- `SpeechToTextAsync(audioStream, fileName, user)` - Convert speech to text
- `TextToAudioAsync(request)` - Convert text to audio

#### Application Operations
- `GetApplicationInfoAsync()` - Get basic information
- `GetApplicationParametersAsync()` - Get parameters
- `GetApplicationMetaAsync()` - Get meta information
- `GetApplicationSiteAsync()` - Get WebApp settings

#### Annotation Operations
- `GetAnnotationsAsync(page, limit)` - List annotations
- `CreateAnnotationAsync(request)` - Create annotation
- `UpdateAnnotationAsync(annotationId, request)` - Update annotation
- `DeleteAnnotationAsync(annotationId)` - Delete annotation
- `SetAnnotationReplyAsync(action, request)` - Enable/disable annotation reply
- `GetAnnotationReplyStatusAsync(action, jobId)` - Check job status

#### Feedback Operations
- `GetFeedbacksAsync(page, limit)` - Get application feedbacks

## Models

All request and response models are located in the `DifyApiClient.Models` namespace:

- **Chat**: `ChatMessageRequest`, `ChatCompletionResponse`, `ChunkChatCompletionResponse`
- **Conversation**: `Conversation`, `Message`, `ConversationListResponse`, `MessageListResponse`
- **Application**: `ApplicationInfo`, `ApplicationParameters`, `ApplicationMeta`, `ApplicationSite`
- **Annotation**: `Annotation`, `AnnotationRequest`, `AnnotationListResponse`
- **Audio**: `SpeechToTextRequest`, `TextToAudioRequest`
- **File**: `FileUploadResponse`, `FileInfo`
- **Feedback**: `AppFeedback`, `FeedbackListResponse`

## Configuration

```csharp
var options = new DifyApiClientOptions
{
    BaseUrl = "http://your-dify-instance/v1",
    ApiKey = "your-api-key",
    Timeout = TimeSpan.FromSeconds(100) // Optional, default is 100 seconds
};
```

## Error Handling

The client throws `HttpRequestException` for HTTP errors. Always wrap calls in try-catch:

```csharp
try
{
    var response = await client.SendChatMessageAsync(request);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"API Error: {ex.Message}");
}
```

## Testing

Run the unit tests:

```bash
dotnet test
```

The test project includes comprehensive tests for all major functionality with mocked HTTP responses.

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues and questions, please open an issue on the GitHub repository.
