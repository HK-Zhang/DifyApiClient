using System.Text.Json.Serialization;

namespace DifyApiClient.Models;

/// <summary>
/// Annotation item
/// </summary>
public class Annotation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("question")]
    public string? Question { get; set; }

    [JsonPropertyName("answer")]
    public string? Answer { get; set; }

    [JsonPropertyName("hit_count")]
    public int HitCount { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }
}

/// <summary>
/// Paginated annotation list response
/// </summary>
public class AnnotationListResponse
{
    [JsonPropertyName("data")]
    public List<Annotation>? Data { get; set; }

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }
}

/// <summary>
/// Create/Update annotation request
/// </summary>
public class AnnotationRequest
{
    [JsonPropertyName("question")]
    public required string Question { get; set; }

    [JsonPropertyName("answer")]
    public required string Answer { get; set; }
}

/// <summary>
/// Annotation reply settings request
/// </summary>
public class AnnotationReplySettingsRequest
{
    [JsonPropertyName("score_threshold")]
    public double? ScoreThreshold { get; set; }

    [JsonPropertyName("embedding_provider_name")]
    public string? EmbeddingProviderName { get; set; }

    [JsonPropertyName("embedding_model_name")]
    public string? EmbeddingModelName { get; set; }
}

/// <summary>
/// Annotation reply settings job response
/// </summary>
public class AnnotationReplyJobResponse
{
    [JsonPropertyName("job_id")]
    public string? JobId { get; set; }

    [JsonPropertyName("job_status")]
    public string? JobStatus { get; set; }

    [JsonPropertyName("error_msg")]
    public string? ErrorMsg { get; set; }
}
