# Dify API Client

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
- Async/await pattern
- IAsyncEnumerable for streaming
- Nullable reference types
- Strong typing with models

✅ **Production Ready**
- Proper error handling
- Cancellation token support
- IDisposable implementation
- Comprehensive unit tests

## Installation

```bash
# Build the project
dotnet build

# Run tests
dotnet test
```

## Quick Start

```csharp
using DifyApiClient;
using DifyApiClient.Models;

// Initialize the client
var options = new DifyApiClientOptions
{
    BaseUrl = "http://osl4243/v1",
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
