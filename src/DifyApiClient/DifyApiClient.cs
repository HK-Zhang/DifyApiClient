using DifyApiClient.Models;
using DifyApiClient.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DifyApiClient;

/// <summary>
/// Client for interacting with the Dify Chat API
/// </summary>
public class DifyApiClient : IDifyApiClient
{
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;
    private bool _disposed;

    // Feature-specific services
    private readonly IChatService _chatService;
    private readonly IConversationService _conversationService;
    private readonly IFileService _fileService;
    private readonly IMessageService _messageService;
    private readonly IAudioService _audioService;
    private readonly IApplicationService _applicationService;
    private readonly IAnnotationService _annotationService;
    private readonly IFeedbackService _feedbackService;

    public DifyApiClient(DifyApiClientOptions options)
        : this(options, new HttpClient(), disposeHttpClient: true)
    {
    }

    public DifyApiClient(DifyApiClientOptions options, HttpClient httpClient, bool disposeHttpClient = false)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);
        
        _httpClient = httpClient;
        _disposeHttpClient = disposeHttpClient;

        var baseUrl = options.BaseUrl.TrimEnd('/');
        _httpClient.BaseAddress = new Uri($"{baseUrl}/");
        _httpClient.Timeout = options.Timeout;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.ApiKey);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // Initialize services
        _chatService = new ChatService(_httpClient, jsonOptions);
        _conversationService = new ConversationService(_httpClient, jsonOptions);
        _fileService = new FileService(_httpClient, jsonOptions);
        _messageService = new MessageService(_httpClient, jsonOptions);
        _audioService = new AudioService(_httpClient, jsonOptions);
        _applicationService = new ApplicationService(_httpClient, jsonOptions);
        _annotationService = new AnnotationService(_httpClient, jsonOptions);
        _feedbackService = new FeedbackService(_httpClient, jsonOptions);
    }

    #region Chat Operations

    /// <summary>
    /// Send a chat message in blocking mode
    /// </summary>
    public Task<ChatCompletionResponse> SendChatMessageAsync(
        ChatMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        return _chatService.SendChatMessageAsync(request, cancellationToken);
    }

    /// <summary>
    /// Send a chat message in streaming mode
    /// </summary>
    public IAsyncEnumerable<ChunkChatCompletionResponse> SendChatMessageStreamAsync(
        ChatMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        return _chatService.SendChatMessageStreamAsync(request, cancellationToken);
    }

    /// <summary>
    /// Stop message generation
    /// </summary>
    public Task StopGenerationAsync(string taskId, string user, CancellationToken cancellationToken = default)
    {
        return _chatService.StopGenerationAsync(taskId, user, cancellationToken);
    }

    #endregion

    #region File Operations

    /// <summary>
    /// Upload a file
    /// </summary>
    public Task<FileUploadResponse> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default)
    {
        return _fileService.UploadFileAsync(fileStream, fileName, user, cancellationToken);
    }

    #endregion

    #region Conversation Operations

    /// <summary>
    /// Get conversation history messages
    /// </summary>
    public Task<MessageListResponse> GetConversationMessagesAsync(
        string conversationId,
        string user,
        string? firstId = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return _conversationService.GetConversationMessagesAsync(conversationId, user, firstId, limit, cancellationToken);
    }

    /// <summary>
    /// Get conversations list
    /// </summary>
    public Task<ConversationListResponse> GetConversationsAsync(
        string user,
        string? lastId = null,
        int limit = 20,
        bool? pinned = null,
        CancellationToken cancellationToken = default)
    {
        return _conversationService.GetConversationsAsync(user, lastId, limit, pinned, cancellationToken);
    }

    /// <summary>
    /// Delete a conversation
    /// </summary>
    public Task DeleteConversationAsync(
        string conversationId,
        string user,
        CancellationToken cancellationToken = default)
    {
        return _conversationService.DeleteConversationAsync(conversationId, user, cancellationToken);
    }

    /// <summary>
    /// Rename a conversation
    /// </summary>
    public Task<Conversation> RenameConversationAsync(
        string conversationId,
        ConversationRenameRequest request,
        CancellationToken cancellationToken = default)
    {
        return _conversationService.RenameConversationAsync(conversationId, request, cancellationToken);
    }

    /// <summary>
    /// Get conversation variables
    /// </summary>
    public Task<Dictionary<string, object>> GetConversationVariablesAsync(
        string conversationId,
        string user,
        CancellationToken cancellationToken = default)
    {
        return _conversationService.GetConversationVariablesAsync(conversationId, user, cancellationToken);
    }

    #endregion

    #region Message Operations

    /// <summary>
    /// Submit message feedback
    /// </summary>
    public Task SubmitMessageFeedbackAsync(
        string messageId,
        MessageFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        return _messageService.SubmitMessageFeedbackAsync(messageId, request, cancellationToken);
    }

    /// <summary>
    /// Get suggested questions for a message
    /// </summary>
    public Task<SuggestedQuestionsResponse> GetSuggestedQuestionsAsync(
        string messageId,
        string user,
        CancellationToken cancellationToken = default)
    {
        return _messageService.GetSuggestedQuestionsAsync(messageId, user, cancellationToken);
    }

    #endregion

    #region Audio Operations

    /// <summary>
    /// Convert speech to text
    /// </summary>
    public Task<string> SpeechToTextAsync(
        Stream audioStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default)
    {
        return _audioService.SpeechToTextAsync(audioStream, fileName, user, cancellationToken);
    }

    /// <summary>
    /// Convert text to audio
    /// </summary>
    public Task<Stream> TextToAudioAsync(
        TextToAudioRequest request,
        CancellationToken cancellationToken = default)
    {
        return _audioService.TextToAudioAsync(request, cancellationToken);
    }

    #endregion

    #region Application Info Operations

    /// <summary>
    /// Get application basic information
    /// </summary>
    public Task<ApplicationInfo> GetApplicationInfoAsync(CancellationToken cancellationToken = default)
    {
        return _applicationService.GetApplicationInfoAsync(cancellationToken);
    }

    /// <summary>
    /// Get application parameters
    /// </summary>
    public Task<ApplicationParameters> GetApplicationParametersAsync(CancellationToken cancellationToken = default)
    {
        return _applicationService.GetApplicationParametersAsync(cancellationToken);
    }

    /// <summary>
    /// Get application meta information
    /// </summary>
    public Task<ApplicationMeta> GetApplicationMetaAsync(CancellationToken cancellationToken = default)
    {
        return _applicationService.GetApplicationMetaAsync(cancellationToken);
    }

    /// <summary>
    /// Get application WebApp settings
    /// </summary>
    public Task<ApplicationSite> GetApplicationSiteAsync(CancellationToken cancellationToken = default)
    {
        return _applicationService.GetApplicationSiteAsync(cancellationToken);
    }

    #endregion

    #region Annotation Operations

    /// <summary>
    /// Get annotation list
    /// </summary>
    public Task<AnnotationListResponse> GetAnnotationsAsync(
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return _annotationService.GetAnnotationsAsync(page, limit, cancellationToken);
    }

    /// <summary>
    /// Create an annotation
    /// </summary>
    public Task<Annotation> CreateAnnotationAsync(
        AnnotationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _annotationService.CreateAnnotationAsync(request, cancellationToken);
    }

    /// <summary>
    /// Update an annotation
    /// </summary>
    public Task<Annotation> UpdateAnnotationAsync(
        string annotationId,
        AnnotationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _annotationService.UpdateAnnotationAsync(annotationId, request, cancellationToken);
    }

    /// <summary>
    /// Delete an annotation
    /// </summary>
    public Task DeleteAnnotationAsync(
        string annotationId,
        CancellationToken cancellationToken = default)
    {
        return _annotationService.DeleteAnnotationAsync(annotationId, cancellationToken);
    }

    /// <summary>
    /// Enable or disable annotation reply settings
    /// </summary>
    public Task<AnnotationReplyJobResponse> SetAnnotationReplyAsync(
        string action,
        AnnotationReplySettingsRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        return _annotationService.SetAnnotationReplyAsync(action, request, cancellationToken);
    }

    /// <summary>
    /// Query annotation reply settings task status
    /// </summary>
    public Task<AnnotationReplyJobResponse> GetAnnotationReplyStatusAsync(
        string action,
        string jobId,
        CancellationToken cancellationToken = default)
    {
        return _annotationService.GetAnnotationReplyStatusAsync(action, jobId, cancellationToken);
    }

    #endregion

    #region Feedback Operations

    /// <summary>
    /// Get application feedbacks
    /// </summary>
    public Task<FeedbackListResponse> GetFeedbacksAsync(
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return _feedbackService.GetFeedbacksAsync(page, limit, cancellationToken);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _disposeHttpClient)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }

    #endregion
}
