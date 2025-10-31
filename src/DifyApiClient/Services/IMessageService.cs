using DifyApiClient.Models;

namespace DifyApiClient.Services;

/// <summary>
/// Service for message-related operations
/// </summary>
public interface IMessageService
{
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
}
