using DifyApiClient.Core;
using DifyApiClient.Models;
using DifyApiClient.Telemetry;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of chat service
/// </summary>
internal class ChatService(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger? logger = null) : BaseApiClient(httpClient, jsonOptions, logger), IChatService
{
    public async Task<ChatCompletionResponse> SendChatMessageAsync(
        ChatMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Sending chat message in blocking mode");
        request.ResponseMode = "blocking";
        var result = await PostAsync<ChatMessageRequest, ChatCompletionResponse>(
            "chat-messages",
            request,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("Chat message completed successfully");
        return result;
    }

    public async IAsyncEnumerable<ChunkChatCompletionResponse> SendChatMessageStreamAsync(
        ChatMessageRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = DifyActivitySource.Instance.StartActivity("POST chat-messages (streaming)", ActivityKind.Client);
        activity?.SetTag("http.method", "POST");
        activity?.SetTag("http.url", "chat-messages");
        activity?.SetTag("streaming", true);
        
        DifyMetrics.StreamingOperations.Add(1, new KeyValuePair<string, object?>("operation", "chat"));
        
        Logger.LogInformation("Sending chat message in streaming mode");
        request.ResponseMode = "streaming";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "chat-messages")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await HttpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);

        var chunkCount = 0;
        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:"))
                continue;

            var jsonData = line["data:".Length..].Trim();
            if (string.IsNullOrWhiteSpace(jsonData))
                continue;

            var chunk = JsonSerializer.Deserialize<ChunkChatCompletionResponse>(jsonData, JsonOptions);
            if (chunk != null)
            {
                chunkCount++;
                DifyMetrics.StreamingChunks.Add(1, new KeyValuePair<string, object?>("operation", "chat"));
                yield return chunk;
            }
        }
        
        activity?.SetTag("chunk_count", chunkCount);
        activity?.SetStatus(ActivityStatusCode.Ok);
        Logger.LogInformation("Streaming chat message completed, received {ChunkCount} chunks", chunkCount);
    }

    public async Task StopGenerationAsync(
        string taskId,
        string user,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Stopping chat message generation for task {TaskId}", taskId);
        var request = new { user };
        await PostAsync($"chat-messages/{taskId}/stop", request, cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("Chat message generation stopped for task {TaskId}", taskId);
    }
}
