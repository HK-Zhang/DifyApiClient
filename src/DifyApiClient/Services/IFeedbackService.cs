using DifyApiClient.Models;

namespace DifyApiClient.Services;

/// <summary>
/// Service for feedback operations
/// </summary>
public interface IFeedbackService
{
    /// <summary>
    /// Get application feedbacks
    /// </summary>
    Task<FeedbackListResponse> GetFeedbacksAsync(
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default);
}
