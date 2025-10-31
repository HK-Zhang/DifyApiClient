using DifyApiClient.Exceptions;
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

    private const string NullDeserializationError = "Response deserialization returned null";

    public BaseApiClient(HttpClient httpClient, JsonSerializerOptions jsonOptions)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        JsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
    }

    protected async Task<TResponse> GetAsync<TResponse>(
        string url,
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken)
            ?? throw new DifyApiException(NullDeserializationError);
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(
        string url,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.PostAsJsonAsync(url, request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken)
            ?? throw new DifyApiException(NullDeserializationError);
    }

    protected async Task PostAsync<TRequest>(
        string url,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.PostAsJsonAsync(url, request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    protected async Task<TResponse> PostAsync<TResponse>(
        string url,
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.PostAsync(url, content, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken)
            ?? throw new DifyApiException(NullDeserializationError);
    }

    protected async Task<TResponse> PutAsync<TRequest, TResponse>(
        string url,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.PutAsJsonAsync(url, request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken)
            ?? throw new DifyApiException(NullDeserializationError);
    }

    protected async Task DeleteAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        // Use simple DELETE - the API issue is actually on the server side
        var response = await HttpClient.DeleteAsync(url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new DifyApiException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). " +
                $"Response body: {errorContent}",
                (int)response.StatusCode,
                errorContent);
        }
    }
}
