# Architecture Overview

## New Layered Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        DifyApiClient                            │
│                      (Facade Pattern)                           │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Public API - 100% Backward Compatible                  │   │
│  │  - SendChatMessageAsync()                               │   │
│  │  - GetConversationsAsync()                              │   │
│  │  - UploadFileAsync()                                    │   │
│  │  - ... all existing methods ...                         │   │
│  └─────────────────────────────────────────────────────────┘   │
│                            ↓ Delegates to                       │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              Internal Service Composition                 │  │
│  ├──────────────────────────────────────────────────────────┤  │
│  │ ChatService │ ConversationService │ FileService │ etc... │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                             ↓ Uses
┌─────────────────────────────────────────────────────────────────┐
│                      Service Layer                              │
├─────────────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌────────────────────┐  ┌────────────────┐ │
│  │ IChatService │  │ IConversationSvc   │  │ IFileService   │ │
│  │      ↓       │  │        ↓           │  │       ↓        │ │
│  │ ChatService  │  │ ConversationSvc    │  │  FileService   │ │
│  └──────────────┘  └────────────────────┘  └────────────────┘ │
│                                                                 │
│  ┌──────────────┐  ┌────────────────────┐  ┌────────────────┐ │
│  │ IMessageSvc  │  │ IAudioService      │  │ IAppService    │ │
│  │      ↓       │  │        ↓           │  │       ↓        │ │
│  │ MessageSvc   │  │ AudioService       │  │  AppService    │ │
│  └──────────────┘  └────────────────────┘  └────────────────┘ │
│                                                                 │
│  ┌──────────────┐  ┌────────────────────┐                     │
│  │ IAnnotation  │  │ IFeedbackService   │                     │
│  │      ↓       │  │        ↓           │                     │
│  │ Annotation   │  │ FeedbackService    │                     │
│  └──────────────┘  └────────────────────┘                     │
└─────────────────────────────────────────────────────────────────┘
                             ↓ Inherits from
┌─────────────────────────────────────────────────────────────────┐
│                      Core Layer                                 │
├─────────────────────────────────────────────────────────────────┤
│                      BaseApiClient                              │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Protected Methods (Reusable HTTP Patterns):            │   │
│  │  • GetAsync<TResponse>(url)                             │   │
│  │  • PostAsync<TRequest, TResponse>(url, request)         │   │
│  │  • PutAsync<TRequest, TResponse>(url, request)          │   │
│  │  • DeleteAsync(url)                                     │   │
│  │  • PostAsync<TResponse>(url, content)                   │   │
│  │  • EnsureSuccessAsync(response) - Error handling        │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                             ↓ Uses
┌─────────────────────────────────────────────────────────────────┐
│                    Utilities & Exceptions                       │
├─────────────────────────────────────────────────────────────────┤
│  QueryStringBuilder          │   DifyApiException               │
│  • Fluent API                │   • StatusCode                   │
│  • Auto URL encoding         │   • ResponseBody                 │
│  • Type-safe building        │   • Enhanced error info          │
└─────────────────────────────────────────────────────────────────┘
                             ↓ Serializes/Deserializes
┌─────────────────────────────────────────────────────────────────┐
│                         Models Layer                            │
├─────────────────────────────────────────────────────────────────┤
│  ChatModels  │  ConversationModels  │  ApplicationModels        │
│  AudioModels │  AnnotationModels    │  FeedbackModels           │
│  FileUploadModels                                               │
└─────────────────────────────────────────────────────────────────┘
```

## Key Design Patterns

### 1. **Facade Pattern**
`DifyApiClient` acts as a simplified interface to the complex subsystem of services.

### 2. **Template Method Pattern**
`BaseApiClient` defines the skeleton of HTTP operations, allowing services to focus on business logic.

### 3. **Dependency Injection Ready**
All services depend on abstractions (interfaces), enabling:
- Easy testing with mocks
- Flexible composition
- Loose coupling

### 4. **Single Responsibility Principle**
Each service handles one specific domain:
- `ChatService` → Chat operations only
- `ConversationService` → Conversation management only
- etc.

## Benefits Visualization

```
┌─────────────────────────────────────────────────────────────────┐
│                    Before Refactoring                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│         ┌─────────────────────────────────────┐                │
│         │     DifyApiClient (God Object)      │                │
│         │                                     │                │
│         │  • 520 lines                       │                │
│         │  • 30+ methods                     │                │
│         │  • 8+ responsibilities             │                │
│         │  • Duplicated code                 │                │
│         │  • Mixed HTTP & business logic     │                │
│         │  • Hard to test                    │                │
│         │  • Tightly coupled                 │                │
│         └─────────────────────────────────────┘                │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

                            ↓ Refactored to

┌─────────────────────────────────────────────────────────────────┐
│                     After Refactoring                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────┐   ┌──────────────────────────────────┐   │
│  │ DifyApiClient   │   │     Service Layer (8 services)   │   │
│  │  (Facade)       │───│  • ChatService (~80 lines)       │   │
│  │  260 lines      │   │  • ConversationService (~90 L)   │   │
│  │  Delegates only │   │  • FileService (~30 lines)       │   │
│  └─────────────────┘   │  • MessageService (~45 lines)    │   │
│                        │  • AudioService (~45 lines)      │   │
│                        │  • ApplicationService (~50 L)    │   │
│                        │  • AnnotationService (~80 L)     │   │
│                        │  • FeedbackService (~30 lines)   │   │
│                        └──────────────────────────────────┘   │
│                                      ↓                         │
│                        ┌──────────────────────────────────┐   │
│                        │  Core Layer                      │   │
│                        │  • BaseApiClient (~100 lines)    │   │
│                        │  • QueryStringBuilder (~60 L)    │   │
│                        │  • DifyApiException (~25 lines)  │   │
│                        └──────────────────────────────────┘   │
│                                                                 │
│  ✅ Single Responsibility    ✅ DRY (No duplication)           │
│  ✅ Easy to test             ✅ Loose coupling                 │
│  ✅ Easy to maintain         ✅ Easy to extend                 │
└─────────────────────────────────────────────────────────────────┘
```

## File Structure

```
src/DifyApiClient/
├── DifyApiClient.cs              (Facade - 260 lines)
├── IDifyApiClient.cs              (Interface)
├── DifyApiClientOptions.cs        (Configuration)
│
├── Core/
│   └── BaseApiClient.cs           (Reusable HTTP patterns)
│
├── Utilities/
│   └── QueryStringBuilder.cs     (Query string helper)
│
├── Exceptions/
│   └── DifyApiException.cs        (Custom exception)
│
├── Services/
│   ├── IChatService.cs            (Interface)
│   ├── ChatService.cs             (Implementation - ~80 lines)
│   ├── IConversationService.cs    (Interface)
│   ├── ConversationService.cs     (Implementation - ~90 lines)
│   ├── IFileService.cs            (Interface)
│   ├── FileService.cs             (Implementation - ~30 lines)
│   ├── IMessageService.cs         (Interface)
│   ├── MessageService.cs          (Implementation - ~45 lines)
│   ├── IAudioService.cs           (Interface)
│   ├── AudioService.cs            (Implementation - ~45 lines)
│   ├── IApplicationService.cs     (Interface)
│   ├── ApplicationService.cs      (Implementation - ~50 lines)
│   ├── IAnnotationService.cs      (Interface)
│   ├── AnnotationService.cs       (Implementation - ~80 lines)
│   ├── IFeedbackService.cs        (Interface)
│   └── FeedbackService.cs         (Implementation - ~30 lines)
│
└── Models/
    ├── ChatModels.cs
    ├── ConversationModels.cs
    ├── ApplicationModels.cs
    ├── AudioModels.cs
    ├── AnnotationModels.cs
    ├── FeedbackModels.cs
    └── FileUploadModels.cs
```

## Code Metrics Comparison

| Metric                    | Before | After  | Improvement |
|---------------------------|--------|--------|-------------|
| Largest file (LOC)        | 520    | 260    | 50% ↓       |
| Average service size      | N/A    | ~55    | Focused     |
| Code duplication          | High   | None   | Eliminated  |
| Cyclomatic complexity     | High   | Low    | Much better |
| Test coverage potential   | Low    | High   | Much better |
| Maintainability Index     | Low    | High   | Much better |
