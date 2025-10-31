using DifyApiClient.Core;
using DifyApiClient.Models;
using DifyApiClient.Utilities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of conversation service
/// </summary>
internal class ConversationService : BaseApiClient, IConversationService
{
    public ConversationService(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger? logger = null)
        : base(httpClient, jsonOptions, logger)
    {
    }

    public async Task<MessageListResponse> GetConversationMessagesAsync(
        string conversationId,
        string user,
        string? firstId = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var queryString = new QueryStringBuilder()
            .Add("user", user)
            .Add("conversation_id", conversationId)
            .Add("limit", limit)
            .AddIfNotNull("first_id", firstId)
            .Build();

        return await GetAsync<MessageListResponse>(
            $"messages?{queryString}",
            cancellationToken);
    }

    public async Task<ConversationListResponse> GetConversationsAsync(
        string user,
        string? lastId = null,
        int limit = 20,
        bool? pinned = null,
        CancellationToken cancellationToken = default)
    {
        var queryString = new QueryStringBuilder()
            .Add("user", user)
            .Add("limit", limit)
            .AddIfNotNull("last_id", lastId)
            .AddIfNotNull("pinned", pinned)
            .Build();

        return await GetAsync<ConversationListResponse>(
            $"conversations?{queryString}",
            cancellationToken);
    }

    public async Task DeleteConversationAsync(
        string conversationId,
        string user,
        CancellationToken cancellationToken = default)
    {
        var queryString = new QueryStringBuilder()
            .Add("user", user)
            .Build();

        await DeleteAsync(
            $"conversations/{conversationId}?{queryString}",
            cancellationToken);
    }

    public async Task<Conversation> RenameConversationAsync(
        string conversationId,
        ConversationRenameRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<ConversationRenameRequest, Conversation>(
            $"conversations/{conversationId}/name",
            request,
            cancellationToken);
    }

    public async Task<Dictionary<string, object>> GetConversationVariablesAsync(
        string conversationId,
        string user,
        CancellationToken cancellationToken = default)
    {
        var queryString = new QueryStringBuilder()
            .Add("user", user)
            .Build();

        return await GetAsync<Dictionary<string, object>>(
            $"conversations/{conversationId}/variables?{queryString}",
            cancellationToken);
    }
}
