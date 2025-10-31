using DifyApiClient.Core;
using DifyApiClient.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of audio service
/// </summary>
internal class AudioService : BaseApiClient, IAudioService
{
    public AudioService(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger? logger = null)
        : base(httpClient, jsonOptions, logger)
    {
    }

    public async Task<string> SpeechToTextAsync(
        Stream audioStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Converting speech to text from file: {FileName}", fileName);
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(audioStream), "file", fileName);
        content.Add(new StringContent(user), "user");

        var response = await HttpClient.PostAsync("audio-to-text", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(
            JsonOptions,
            cancellationToken);
        var text = result?["text"] ?? string.Empty;
        Logger.LogInformation("Speech to text conversion completed, text length: {Length}", text.Length);
        return text;
    }

    public async Task<Stream> TextToAudioAsync(
        TextToAudioRequest request,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Converting text to audio");
        var response = await HttpClient.PostAsJsonAsync("text-to-audio", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        Logger.LogInformation("Text to audio conversion completed");
        return stream;
    }
}
