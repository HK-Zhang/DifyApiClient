using DifyApiClient.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace DifyApiClient;

/// <summary>
/// Client for interacting with the Dify Chat API
/// </summary>
public class DifyApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly DifyApiClientOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public DifyApiClient(DifyApiClientOptions options)
        : this(options, new HttpClient())
    {
    }

    public DifyApiClient(DifyApiClientOptions options, HttpClient httpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = _options.Timeout;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Chat Operations

    /// <summary>
    /// Send a chat message in blocking mode
    /// </summary>
    public async Task<ChatCompletionResponse> SendChatMessageAsync(
        ChatMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        request.ResponseMode = "blocking";
        var response = await _httpClient.PostAsJsonAsync("chat-messages", request, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    /// <summary>
    /// Send a chat message in streaming mode
    /// </summary>
    public async IAsyncEnumerable<ChunkChatCompletionResponse> SendChatMessageStreamAsync(
        ChatMessageRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.ResponseMode = "streaming";
        
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "chat-messages")
        {
            Content = JsonContent.Create(request, options: _jsonOptions)
        };
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:"))
                continue;

            var jsonData = line["data:".Length..].Trim();
            if (string.IsNullOrWhiteSpace(jsonData))
                continue;

            var chunk = JsonSerializer.Deserialize<ChunkChatCompletionResponse>(jsonData, _jsonOptions);
            if (chunk != null)
                yield return chunk;
        }
    }

    /// <summary>
    /// Stop message generation
    /// </summary>
    public async Task StopGenerationAsync(string taskId, string user, CancellationToken cancellationToken = default)
    {
        var request = new { user };
        var response = await _httpClient.PostAsJsonAsync($"chat-messages/{taskId}/stop", request, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region File Operations

    /// <summary>
    /// Upload a file
    /// </summary>
    public async Task<FileUploadResponse> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        content.Add(new StringContent(user), "user");

        var response = await _httpClient.PostAsync("files/upload", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FileUploadResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    #endregion

    #region Conversation Operations

    /// <summary>
    /// Get conversation history messages
    /// </summary>
    public async Task<MessageListResponse> GetConversationMessagesAsync(
        string conversationId,
        string user,
        string? firstId = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>
        {
            $"user={Uri.EscapeDataString(user)}",
            $"conversation_id={Uri.EscapeDataString(conversationId)}",
            $"limit={limit}"
        };

        if (!string.IsNullOrEmpty(firstId))
            queryParams.Add($"first_id={Uri.EscapeDataString(firstId)}");

        var url = $"messages?{string.Join("&", queryParams)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MessageListResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    /// <summary>
    /// Get conversations list
    /// </summary>
    public async Task<ConversationListResponse> GetConversationsAsync(
        string user,
        string? lastId = null,
        int limit = 20,
        bool? pinned = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>
        {
            $"user={Uri.EscapeDataString(user)}",
            $"limit={limit}"
        };

        if (!string.IsNullOrEmpty(lastId))
            queryParams.Add($"last_id={Uri.EscapeDataString(lastId)}");

        if (pinned.HasValue)
            queryParams.Add($"pinned={pinned.Value.ToString().ToLower()}");

        var url = $"conversations?{string.Join("&", queryParams)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ConversationListResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    /// <summary>
    /// Delete a conversation
    /// </summary>
    public async Task DeleteConversationAsync(
        string conversationId,
        string user,
        CancellationToken cancellationToken = default)
    {
        var url = $"conversations/{conversationId}?user={Uri.EscapeDataString(user)}";
        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Rename a conversation
    /// </summary>
    public async Task<Conversation> RenameConversationAsync(
        string conversationId,
        ConversationRenameRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"conversations/{conversationId}/name", request, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Conversation>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    /// <summary>
    /// Get conversation variables
    /// </summary>
    public async Task<Dictionary<string, object>> GetConversationVariablesAsync(
        string conversationId,
        string user,
        CancellationToken cancellationToken = default)
    {
        var url = $"conversations/{conversationId}/variables?user={Uri.EscapeDataString(user)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    #endregion

    #region Message Operations

    /// <summary>
    /// Submit message feedback
    /// </summary>
    public async Task SubmitMessageFeedbackAsync(
        string messageId,
        MessageFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"messages/{messageId}/feedbacks", request, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Get suggested questions for a message
    /// </summary>
    public async Task<SuggestedQuestionsResponse> GetSuggestedQuestionsAsync(
        string messageId,
        string user,
        CancellationToken cancellationToken = default)
    {
        var url = $"messages/{messageId}/suggested?user={Uri.EscapeDataString(user)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SuggestedQuestionsResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    #endregion

    #region Audio Operations

    /// <summary>
    /// Convert speech to text
    /// </summary>
    public async Task<string> SpeechToTextAsync(
        Stream audioStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(audioStream), "file", fileName);
        content.Add(new StringContent(user), "user");

        var response = await _httpClient.PostAsync("audio-to-text", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(_jsonOptions, cancellationToken);
        return result?["text"] ?? string.Empty;
    }

    /// <summary>
    /// Convert text to audio
    /// </summary>
    public async Task<Stream> TextToAudioAsync(
        TextToAudioRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("text-to-audio", request, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    #endregion

    #region Application Info Operations

    /// <summary>
    /// Get application basic information
    /// </summary>
    public async Task<ApplicationInfo> GetApplicationInfoAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("info", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplicationInfo>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    /// <summary>
    /// Get application parameters
    /// </summary>
    public async Task<ApplicationParameters> GetApplicationParametersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("parameters", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplicationParameters>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    /// <summary>
    /// Get application meta information
    /// </summary>
    public async Task<ApplicationMeta> GetApplicationMetaAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("meta", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplicationMeta>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    /// <summary>
    /// Get application WebApp settings
    /// </summary>
    public async Task<ApplicationSite> GetApplicationSiteAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("site", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplicationSite>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    #endregion

    #region Annotation Operations

    /// <summary>
    /// Get annotation list
    /// </summary>
    public async Task<AnnotationListResponse> GetAnnotationsAsync(
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var url = $"apps/annotations?page={page}&limit={limit}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AnnotationListResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    /// <summary>
    /// Create an annotation
    /// </summary>
    public async Task<Annotation> CreateAnnotationAsync(
        AnnotationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("apps/annotations", request, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Annotation>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    /// <summary>
    /// Update an annotation
    /// </summary>
    public async Task<Annotation> UpdateAnnotationAsync(
        string annotationId,
        AnnotationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"apps/annotations/{annotationId}", request, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Annotation>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    /// <summary>
    /// Delete an annotation
    /// </summary>
    public async Task DeleteAnnotationAsync(
        string annotationId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"apps/annotations/{annotationId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Enable or disable annotation reply settings
    /// </summary>
    public async Task<AnnotationReplyJobResponse> SetAnnotationReplyAsync(
        string action,
        AnnotationReplySettingsRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        if (action != "enable" && action != "disable")
            throw new ArgumentException("Action must be 'enable' or 'disable'", nameof(action));

        var response = await _httpClient.PostAsJsonAsync($"apps/annotation-reply/{action}", request ?? new AnnotationReplySettingsRequest(), _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AnnotationReplyJobResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    /// <summary>
    /// Query annotation reply settings task status
    /// </summary>
    public async Task<AnnotationReplyJobResponse> GetAnnotationReplyStatusAsync(
        string action,
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (action != "enable" && action != "disable")
            throw new ArgumentException("Action must be 'enable' or 'disable'", nameof(action));

        var response = await _httpClient.GetAsync($"apps/annotation-reply/{action}/status/{jobId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AnnotationReplyJobResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
    }

    #endregion

    #region Feedback Operations

    /// <summary>
    /// Get application feedbacks
    /// </summary>
    public async Task<FeedbackListResponse> GetFeedbacksAsync(
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var url = $"apps/feedbacks?page={page}&limit={limit}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FeedbackListResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization returned null");
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
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }

    #endregion
}
