using System.Text.Json.Serialization;

namespace DifyApiClient.Models;

/// <summary>
/// Conversation information
/// </summary>
public class Conversation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("inputs")]
    public Dictionary<string, object>? Inputs { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }
}

/// <summary>
/// Paginated conversation list response
/// </summary>
public class ConversationListResponse
{
    [JsonPropertyName("data")]
    public List<Conversation>? Data { get; set; }

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }
}

/// <summary>
/// Message in conversation history
/// </summary>
public class Message
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("conversation_id")]
    public string? ConversationId { get; set; }

    [JsonPropertyName("inputs")]
    public Dictionary<string, object>? Inputs { get; set; }

    [JsonPropertyName("query")]
    public string? Query { get; set; }

    [JsonPropertyName("answer")]
    public string? Answer { get; set; }

    [JsonPropertyName("feedback")]
    public MessageFeedback? Feedback { get; set; }

    [JsonPropertyName("retriever_resources")]
    public List<RetrieverResource>? RetrieverResources { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }
}

/// <summary>
/// Message feedback
/// </summary>
public class MessageFeedback
{
    [JsonPropertyName("rating")]
    public string? Rating { get; set; }
}

/// <summary>
/// Paginated message list response
/// </summary>
public class MessageListResponse
{
    [JsonPropertyName("data")]
    public List<Message>? Data { get; set; }

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }
}

/// <summary>
/// Request for message feedback
/// </summary>
public class MessageFeedbackRequest
{
    [JsonPropertyName("rating")]
    public required string Rating { get; set; }

    [JsonPropertyName("user")]
    public required string User { get; set; }
}

/// <summary>
/// Conversation rename request
/// </summary>
public class ConversationRenameRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("auto_generate")]
    public bool? AutoGenerate { get; set; }

    [JsonPropertyName("user")]
    public required string User { get; set; }
}

/// <summary>
/// Suggested questions response
/// </summary>
public class SuggestedQuestionsResponse
{
    [JsonPropertyName("data")]
    public List<string>? Data { get; set; }
}
