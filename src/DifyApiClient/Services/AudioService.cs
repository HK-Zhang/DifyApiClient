using DifyApiClient.Core;
using DifyApiClient.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of audio service
/// </summary>
internal class AudioService : BaseApiClient, IAudioService
{
    public AudioService(HttpClient httpClient, JsonSerializerOptions jsonOptions)
        : base(httpClient, jsonOptions)
    {
    }

    public async Task<string> SpeechToTextAsync(
        Stream audioStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(audioStream), "file", fileName);
        content.Add(new StringContent(user), "user");

        var response = await HttpClient.PostAsync("audio-to-text", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(
            JsonOptions,
            cancellationToken);
        return result?["text"] ?? string.Empty;
    }

    public async Task<Stream> TextToAudioAsync(
        TextToAudioRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.PostAsJsonAsync("text-to-audio", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
