using DifyApiClient.Models;

namespace DifyApiClient;

/// <summary>
/// Interface for the Dify API Client
/// </summary>
public interface IDifyApiClient : IDisposable
{
    #region Chat Operations

    /// <summary>
    /// Send a chat message in blocking mode
    /// </summary>
    Task<ChatCompletionResponse> SendChatMessageAsync(
        ChatMessageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a chat message in streaming mode
    /// </summary>
    IAsyncEnumerable<ChunkChatCompletionResponse> SendChatMessageStreamAsync(
        ChatMessageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop message generation
    /// </summary>
    Task StopGenerationAsync(string taskId, string user, CancellationToken cancellationToken = default);

    #endregion

    #region File Operations

    /// <summary>
    /// Upload a file
    /// </summary>
    Task<FileUploadResponse> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default);

    #endregion

    #region Conversation Operations

    /// <summary>
    /// Get conversation history messages
    /// </summary>
    Task<MessageListResponse> GetConversationMessagesAsync(
        string conversationId,
        string user,
        string? firstId = null,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get conversations list
    /// </summary>
    Task<ConversationListResponse> GetConversationsAsync(
        string user,
        string? lastId = null,
        int limit = 20,
        bool? pinned = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a conversation
    /// </summary>
    Task DeleteConversationAsync(
        string conversationId,
        string user,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rename a conversation
    /// </summary>
    Task<Conversation> RenameConversationAsync(
        string conversationId,
        ConversationRenameRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get conversation variables
    /// </summary>
    Task<Dictionary<string, object>> GetConversationVariablesAsync(
        string conversationId,
        string user,
        CancellationToken cancellationToken = default);

    #endregion

    #region Message Operations

    /// <summary>
    /// Submit message feedback
    /// </summary>
    Task SubmitMessageFeedbackAsync(
        string messageId,
        MessageFeedbackRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get suggested questions for a message
    /// </summary>
    Task<SuggestedQuestionsResponse> GetSuggestedQuestionsAsync(
        string messageId,
        string user,
        CancellationToken cancellationToken = default);

    #endregion

    #region Audio Operations

    /// <summary>
    /// Convert speech to text
    /// </summary>
    Task<string> SpeechToTextAsync(
        Stream audioStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convert text to audio
    /// </summary>
    Task<Stream> TextToAudioAsync(
        TextToAudioRequest request,
        CancellationToken cancellationToken = default);

    #endregion

    #region Application Info Operations

    /// <summary>
    /// Get application basic information
    /// </summary>
    Task<ApplicationInfo> GetApplicationInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get application parameters
    /// </summary>
    Task<ApplicationParameters> GetApplicationParametersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get application meta information
    /// </summary>
    Task<ApplicationMeta> GetApplicationMetaAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get application WebApp settings
    /// </summary>
    Task<ApplicationSite> GetApplicationSiteAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Annotation Operations

    /// <summary>
    /// Get annotation list
    /// </summary>
    Task<AnnotationListResponse> GetAnnotationsAsync(
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create an annotation
    /// </summary>
    Task<Annotation> CreateAnnotationAsync(
        AnnotationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an annotation
    /// </summary>
    Task<Annotation> UpdateAnnotationAsync(
        string annotationId,
        AnnotationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an annotation
    /// </summary>
    Task DeleteAnnotationAsync(
        string annotationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable or disable annotation reply settings
    /// </summary>
    Task<AnnotationReplyJobResponse> SetAnnotationReplyAsync(
        string action,
        AnnotationReplySettingsRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Query annotation reply settings task status
    /// </summary>
    Task<AnnotationReplyJobResponse> GetAnnotationReplyStatusAsync(
        string action,
        string jobId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Feedback Operations

    /// <summary>
    /// Get application feedbacks
    /// </summary>
    Task<FeedbackListResponse> GetFeedbacksAsync(
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default);

    #endregion
}
