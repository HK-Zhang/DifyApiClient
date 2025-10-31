using System.Text.Json.Serialization;

namespace DifyApiClient.Models;

/// <summary>
/// Request for sending a chat message
/// </summary>
public class ChatMessageRequest
{
    [JsonPropertyName("query")]
    public required string Query { get; set; }

    [JsonPropertyName("inputs")]
    public Dictionary<string, object>? Inputs { get; set; }

    [JsonPropertyName("response_mode")]
    public string ResponseMode { get; set; } = "blocking";

    [JsonPropertyName("user")]
    public required string User { get; set; }

    [JsonPropertyName("conversation_id")]
    public string? ConversationId { get; set; }

    [JsonPropertyName("files")]
    public List<FileInfo>? Files { get; set; }

    [JsonPropertyName("auto_generate_name")]
    public bool AutoGenerateName { get; set; } = true;

    [JsonPropertyName("trace_id")]
    public string? TraceId { get; set; }
}

/// <summary>
/// File information for chat message
/// </summary>
public class FileInfo
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("transfer_method")]
    public required string TransferMethod { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("upload_file_id")]
    public string? UploadFileId { get; set; }
}

/// <summary>
/// Chat completion response (blocking mode)
/// </summary>
public class ChatCompletionResponse
{
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("task_id")]
    public string? TaskId { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    [JsonPropertyName("conversation_id")]
    public string? ConversationId { get; set; }

    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("answer")]
    public string? Answer { get; set; }

    [JsonPropertyName("metadata")]
    public MessageMetadata? Metadata { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }
}

/// <summary>
/// Streaming chunk response
/// </summary>
public class ChunkChatCompletionResponse
{
    [JsonPropertyName("event")]
    public required string Event { get; set; }

    [JsonPropertyName("task_id")]
    public string? TaskId { get; set; }

    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    [JsonPropertyName("conversation_id")]
    public string? ConversationId { get; set; }

    [JsonPropertyName("answer")]
    public string? Answer { get; set; }

    [JsonPropertyName("created_at")]
    public long? CreatedAt { get; set; }

    [JsonPropertyName("metadata")]
    public MessageMetadata? Metadata { get; set; }

    [JsonPropertyName("workflow_run_id")]
    public string? WorkflowRunId { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

/// <summary>
/// Message metadata
/// </summary>
public class MessageMetadata
{
    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }

    [JsonPropertyName("retriever_resources")]
    public List<RetrieverResource>? RetrieverResources { get; set; }
}

/// <summary>
/// Usage information
/// </summary>
public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>
/// Retriever resource (citation)
/// </summary>
public class RetrieverResource
{
    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("dataset_id")]
    public string? DatasetId { get; set; }

    [JsonPropertyName("dataset_name")]
    public string? DatasetName { get; set; }

    [JsonPropertyName("document_id")]
    public string? DocumentId { get; set; }

    [JsonPropertyName("document_name")]
    public string? DocumentName { get; set; }

    [JsonPropertyName("segment_id")]
    public string? SegmentId { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
