using DifyApiClient.Core;
using DifyApiClient.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of chat service
/// </summary>
internal class ChatService : BaseApiClient, IChatService
{
    public ChatService(HttpClient httpClient, JsonSerializerOptions jsonOptions)
        : base(httpClient, jsonOptions)
    {
    }

    public async Task<ChatCompletionResponse> SendChatMessageAsync(
        ChatMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        request.ResponseMode = "blocking";
        return await PostAsync<ChatMessageRequest, ChatCompletionResponse>(
            "chat-messages",
            request,
            cancellationToken);
    }

    public async IAsyncEnumerable<ChunkChatCompletionResponse> SendChatMessageStreamAsync(
        ChatMessageRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.ResponseMode = "streaming";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "chat-messages")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await HttpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:"))
                continue;

            var jsonData = line["data:".Length..].Trim();
            if (string.IsNullOrWhiteSpace(jsonData))
                continue;

            var chunk = JsonSerializer.Deserialize<ChunkChatCompletionResponse>(jsonData, JsonOptions);
            if (chunk != null)
                yield return chunk;
        }
    }

    public async Task StopGenerationAsync(
        string taskId,
        string user,
        CancellationToken cancellationToken = default)
    {
        var request = new { user };
        await PostAsync($"chat-messages/{taskId}/stop", request, cancellationToken);
    }
}
