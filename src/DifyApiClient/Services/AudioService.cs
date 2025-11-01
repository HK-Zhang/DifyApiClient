using DifyApiClient.Core;
using DifyApiClient.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of audio service
/// </summary>
internal class AudioService(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger? logger = null) : BaseApiClient(httpClient, jsonOptions, logger), IAudioService
{
    public async Task<string> SpeechToTextAsync(
        Stream audioStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Converting speech to text from file: {FileName}", fileName);
        using var content = new MultipartFormDataContent
        {
            { new StreamContent(audioStream), "file", fileName },
            { new StringContent(user), "user" }
        };

        var response = await HttpClient.PostAsync("audio-to-text", content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(
            JsonOptions,
            cancellationToken).ConfigureAwait(false);
        var text = result?["text"] ?? string.Empty;
        Logger.LogInformation("Speech to text conversion completed, text length: {Length}", text.Length);
        return text;
    }

    public async Task<Stream> TextToAudioAsync(
        TextToAudioRequest request,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Converting text to audio");
        var response = await HttpClient.PostAsJsonAsync("text-to-audio", request, JsonOptions, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("Text to audio conversion completed");
        return stream;
    }
}
