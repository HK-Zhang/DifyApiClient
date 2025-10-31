using DifyApiClient.Models;

namespace DifyApiClient.Services;

/// <summary>
/// Service for conversation management operations
/// </summary>
public interface IConversationService
{
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
}
