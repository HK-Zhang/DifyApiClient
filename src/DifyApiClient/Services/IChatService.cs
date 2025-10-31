using DifyApiClient.Models;

namespace DifyApiClient.Services;

/// <summary>
/// Service for chat-related operations
/// </summary>
public interface IChatService
{
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
    Task StopGenerationAsync(
        string taskId,
        string user,
        CancellationToken cancellationToken = default);
}
