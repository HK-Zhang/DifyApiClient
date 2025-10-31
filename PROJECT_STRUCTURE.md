# Project Structure

```
DifyApiClient/
├── DifyApiClient.sln                          # Solution file
├── README.md                                   # Main documentation
│
├── src/
│   └── DifyApiClient/                         # Main library project
│       ├── DifyApiClient.csproj
│       ├── DifyApiClient.cs                   # Main API client
│       ├── DifyApiClientOptions.cs            # Configuration options
│       └── Models/                            # Data models
│           ├── ChatModels.cs                  # Chat-related models
│           ├── ConversationModels.cs          # Conversation models
│           ├── ApplicationModels.cs           # Application info models
│           ├── AnnotationModels.cs            # Annotation models
│           ├── AudioModels.cs                 # Audio operation models
│           ├── FileUploadModels.cs            # File upload models
│           └── FeedbackModels.cs              # Feedback models
│
├── tests/
│   └── DifyApiClient.Tests/                   # Unit test project
│       ├── DifyApiClient.Tests.csproj
│       ├── DifyApiClientTests.cs              # Main test suite
│       └── MockHttpMessageHandler.cs          # Test helper
│
└── samples/
    └── DifyApiClient.Samples/                 # Sample console app
        ├── DifyApiClient.Samples.csproj
        └── Program.cs                         # Interactive demo
```

## API Coverage

### ✅ Chat Operations
- [x] POST /chat-messages (blocking)
- [x] POST /chat-messages (streaming)
- [x] POST /chat-messages/:task_id/stop

### ✅ File Operations
- [x] POST /files/upload

### ✅ Conversation Management
- [x] GET /conversations
- [x] GET /messages
- [x] DELETE /conversations/:conversation_id
- [x] POST /conversations/:conversation_id/name
- [x] GET /conversations/:conversation_id/variables

### ✅ Message Operations
- [x] POST /messages/:message_id/feedbacks
- [x] GET /messages/:message_id/suggested

### ✅ Audio Operations
- [x] POST /audio-to-text
- [x] POST /text-to-audio

### ✅ Application Information
- [x] GET /info
- [x] GET /parameters
- [x] GET /meta
- [x] GET /site

### ✅ Annotations
- [x] GET /apps/annotations
- [x] POST /apps/annotations
- [x] PUT /apps/annotations/:annotation_id
- [x] DELETE /apps/annotations/:annotation_id
- [x] POST /apps/annotation-reply/:action
- [x] GET /apps/annotation-reply/:action/status/:job_id

### ✅ Feedbacks
- [x] GET /apps/feedbacks

## Total: 23 API endpoints implemented!
