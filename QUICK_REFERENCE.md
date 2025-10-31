# Dify API Client - Quick Reference

## Setup
```csharp
var options = new DifyApiClientOptions
{
    BaseUrl = "http://osl4243/v1",
    ApiKey = "your-api-key"
};
using var client = new DifyApiClient(options);
```

## Chat Operations

### Send Message (Blocking)
```csharp
var response = await client.SendChatMessageAsync(new ChatMessageRequest
{
    Query = "Hello",
    User = "user-123",
    ConversationId = "conv-123" // optional
});
```

### Send Message (Streaming)
```csharp
await foreach (var chunk in client.SendChatMessageStreamAsync(request))
{
    if (chunk.Event == "message") Console.Write(chunk.Answer);
}
```

### Stop Generation
```csharp
await client.StopGenerationAsync(taskId, userId);
```

## File Operations

### Upload File
```csharp
using var stream = File.OpenRead("file.pdf");
var file = await client.UploadFileAsync(stream, "file.pdf", "user-123");
```

## Conversations

### List Conversations
```csharp
var list = await client.GetConversationsAsync("user-123", limit: 20);
```

### Get Messages
```csharp
var messages = await client.GetConversationMessagesAsync("conv-123", "user-123");
```

### Delete Conversation
```csharp
await client.DeleteConversationAsync("conv-123", "user-123");
```

### Rename Conversation
```csharp
await client.RenameConversationAsync("conv-123", new ConversationRenameRequest
{
    Name = "New Name",
    User = "user-123"
});
```

## Messages

### Submit Feedback
```csharp
await client.SubmitMessageFeedbackAsync("msg-123", new MessageFeedbackRequest
{
    Rating = "like", // or "dislike"
    User = "user-123"
});
```

### Get Suggestions
```csharp
var suggestions = await client.GetSuggestedQuestionsAsync("msg-123", "user-123");
```

## Audio

### Speech to Text
```csharp
using var audio = File.OpenRead("audio.wav");
var text = await client.SpeechToTextAsync(audio, "audio.wav", "user-123");
```

### Text to Audio
```csharp
using var audio = await client.TextToAudioAsync(new TextToAudioRequest
{
    MessageId = "msg-123",
    Text = "Hello",
    User = "user-123"
});
```

## Application Info

### Get Basic Info
```csharp
var info = await client.GetApplicationInfoAsync();
```

### Get Parameters
```csharp
var params = await client.GetApplicationParametersAsync();
```

### Get Meta
```csharp
var meta = await client.GetApplicationMetaAsync();
```

### Get Site Settings
```csharp
var site = await client.GetApplicationSiteAsync();
```

## Annotations

### List Annotations
```csharp
var list = await client.GetAnnotationsAsync(page: 1, limit: 20);
```

### Create Annotation
```csharp
var annotation = await client.CreateAnnotationAsync(new AnnotationRequest
{
    Question = "Q?",
    Answer = "A"
});
```

### Update Annotation
```csharp
await client.UpdateAnnotationAsync("ann-123", new AnnotationRequest
{
    Question = "Updated Q?",
    Answer = "Updated A"
});
```

### Delete Annotation
```csharp
await client.DeleteAnnotationAsync("ann-123");
```

### Enable Annotation Reply
```csharp
var job = await client.SetAnnotationReplyAsync("enable", new AnnotationReplySettingsRequest
{
    ScoreThreshold = 0.9,
    EmbeddingProviderName = "openai",
    EmbeddingModelName = "text-embedding-ada-002"
});
```

### Check Job Status
```csharp
var status = await client.GetAnnotationReplyStatusAsync("enable", jobId);
```

## Feedbacks

### Get Feedbacks
```csharp
var feedbacks = await client.GetFeedbacksAsync(page: 1, limit: 20);
```

## Response Modes

| Mode | Use Case | Return Type |
|------|----------|-------------|
| `blocking` | Simple request/response | `ChatCompletionResponse` |
| `streaming` | Real-time output | `IAsyncEnumerable<ChunkChatCompletionResponse>` |

## Common Patterns

### With Cancellation
```csharp
var cts = new CancellationTokenSource();
var response = await client.SendChatMessageAsync(request, cts.Token);
```

### Error Handling
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

### Pagination
```csharp
var page = 1;
ConversationListResponse conversations;
do
{
    conversations = await client.GetConversationsAsync(userId, limit: 20);
    // Process conversations.Data
    page++;
} while (conversations.HasMore);
```

## File Types Supported

| Type | Extensions |
|------|------------|
| Document | TXT, MD, PDF, HTML, XLSX, DOCX, CSV, etc. |
| Image | JPG, PNG, GIF, WEBP, SVG |
| Audio | MP3, WAV, M4A, WEBM, AMR |
| Video | MP4, MOV, MPEG, MPGA |

## Build Commands

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run sample
dotnet run --project samples/DifyApiClient.Samples
```
