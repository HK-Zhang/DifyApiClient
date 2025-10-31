using DifyApiClient.Core;
using DifyApiClient.Models;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of feedback service
/// </summary>
internal class FeedbackService : BaseApiClient, IFeedbackService
{
    public FeedbackService(HttpClient httpClient, JsonSerializerOptions jsonOptions)
        : base(httpClient, jsonOptions)
    {
    }

    public async Task<FeedbackListResponse> GetFeedbacksAsync(
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<FeedbackListResponse>(
            $"apps/feedbacks?page={page}&limit={limit}",
            cancellationToken);
    }
}
