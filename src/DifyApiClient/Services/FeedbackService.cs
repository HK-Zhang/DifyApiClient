using DifyApiClient.Core;
using DifyApiClient.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of feedback service
/// </summary>
internal class FeedbackService(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger? logger = null) : BaseApiClient(httpClient, jsonOptions, logger), IFeedbackService
{
    public async Task<FeedbackListResponse> GetFeedbacksAsync(
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<FeedbackListResponse>(
            $"apps/feedbacks?page={page}&limit={limit}",
            cancellationToken: cancellationToken);
    }
}
