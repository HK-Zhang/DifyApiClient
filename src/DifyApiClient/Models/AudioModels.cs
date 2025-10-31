using System.Text.Json.Serialization;

namespace DifyApiClient.Models;

/// <summary>
/// Speech to text request
/// </summary>
public class SpeechToTextRequest
{
    [JsonPropertyName("user")]
    public required string User { get; set; }
}

/// <summary>
/// Text to audio request
/// </summary>
public class TextToAudioRequest
{
    [JsonPropertyName("message_id")]
    public required string MessageId { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("user")]
    public required string User { get; set; }
}
