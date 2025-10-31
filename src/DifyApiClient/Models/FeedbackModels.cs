using System.Text.Json.Serialization;

namespace DifyApiClient.Models;

/// <summary>
/// Application feedback item
/// </summary>
public class AppFeedback
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("rating")]
    public string? Rating { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("from_source")]
    public string? FromSource { get; set; }

    [JsonPropertyName("from_end_user_id")]
    public string? FromEndUserId { get; set; }

    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    [JsonPropertyName("conversation_id")]
    public string? ConversationId { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }
}

/// <summary>
/// Paginated feedback list response
/// </summary>
public class FeedbackListResponse
{
    [JsonPropertyName("data")]
    public List<AppFeedback>? Data { get; set; }

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }
}
