using DifyApiClient.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace DifyApiClient.Core;

/// <summary>
/// Base class for API client operations with common HTTP patterns
/// </summary>
internal class BaseApiClient
{
    protected readonly HttpClient HttpClient;
    protected readonly JsonSerializerOptions JsonOptions;
    protected readonly ILogger Logger;

    private const string NullDeserializationError = "Response deserialization returned null";

    public BaseApiClient(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger? logger = null)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        JsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
        Logger = logger ?? NullLogger.Instance;
    }

    protected async Task<TResponse> GetAsync<TResponse>(
        string url,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("GET request to {Url}", url);
        
        var response = await HttpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        
        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken)
            ?? throw new DifyApiException(NullDeserializationError);
            
        Logger.LogDebug("GET request to {Url} completed successfully", url);
        return result;
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(
        string url,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("POST request to {Url}", url);
        
        var response = await HttpClient.PostAsJsonAsync(url, request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        
        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken)
            ?? throw new DifyApiException(NullDeserializationError);
            
        Logger.LogDebug("POST request to {Url} completed successfully", url);
        return result;
    }

    protected async Task PostAsync<TRequest>(
        string url,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("POST request to {Url} (no response body)", url);
        
        var response = await HttpClient.PostAsJsonAsync(url, request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        
        Logger.LogDebug("POST request to {Url} completed successfully", url);
    }

    protected async Task<TResponse> PostAsync<TResponse>(
        string url,
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("POST request to {Url} with custom content", url);
        
        var response = await HttpClient.PostAsync(url, content, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        
        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken)
            ?? throw new DifyApiException(NullDeserializationError);
            
        Logger.LogDebug("POST request to {Url} completed successfully", url);
        return result;
    }

    protected async Task<TResponse> PutAsync<TRequest, TResponse>(
        string url,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("PUT request to {Url}", url);
        
        var response = await HttpClient.PutAsJsonAsync(url, request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        
        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken)
            ?? throw new DifyApiException(NullDeserializationError);
            
        Logger.LogDebug("PUT request to {Url} completed successfully", url);
        return result;
    }

    protected async Task DeleteAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("DELETE request to {Url}", url);
        
        // Use simple DELETE - the API issue is actually on the server side
        var response = await HttpClient.DeleteAsync(url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        
        Logger.LogDebug("DELETE request to {Url} completed successfully", url);
    }

    private async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            Logger.LogError(
                "HTTP request failed with status {StatusCode} ({ReasonPhrase}). Response: {ResponseBody}",
                (int)response.StatusCode,
                response.ReasonPhrase,
                errorContent);
            
            throw new DifyApiException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). " +
                $"Response body: {errorContent}",
                (int)response.StatusCode,
                errorContent);
        }
    }
}
