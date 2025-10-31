using DifyApiClient.Models;

namespace DifyApiClient.Services;

/// <summary>
/// Service for audio operations
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Convert speech to text
    /// </summary>
    Task<string> SpeechToTextAsync(
        Stream audioStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convert text to audio
    /// </summary>
    Task<Stream> TextToAudioAsync(
        TextToAudioRequest request,
        CancellationToken cancellationToken = default);
}
