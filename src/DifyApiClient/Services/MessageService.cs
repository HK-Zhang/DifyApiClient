using DifyApiClient.Core;
using DifyApiClient.Models;
using DifyApiClient.Utilities;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of message service
/// </summary>
internal class MessageService : BaseApiClient, IMessageService
{
    public MessageService(HttpClient httpClient, JsonSerializerOptions jsonOptions)
        : base(httpClient, jsonOptions)
    {
    }

    public async Task SubmitMessageFeedbackAsync(
        string messageId,
        MessageFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        await PostAsync(
            $"messages/{messageId}/feedbacks",
            request,
            cancellationToken);
    }

    public async Task<SuggestedQuestionsResponse> GetSuggestedQuestionsAsync(
        string messageId,
        string user,
        CancellationToken cancellationToken = default)
    {
        var queryString = new QueryStringBuilder()
            .Add("user", user)
            .Build();

        return await GetAsync<SuggestedQuestionsResponse>(
            $"messages/{messageId}/suggested?{queryString}",
            cancellationToken);
    }
}
