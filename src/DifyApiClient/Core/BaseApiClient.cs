using DifyApiClient.Exceptions;
using DifyApiClient.Telemetry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace DifyApiClient.Core;

/// <summary>
/// Base class for API client operations with common HTTP patterns
/// </summary>
internal class BaseApiClient(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger? logger = null)
{
    protected readonly HttpClient HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    protected readonly JsonSerializerOptions JsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
    protected readonly ILogger Logger = logger ?? NullLogger.Instance;

    private const string NullDeserializationError = "Response deserialization returned null";

    protected async Task<TResponse> GetAsync<TResponse>(
        string url,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = DifyActivitySource.Instance.StartActivity($"GET {url}", ActivityKind.Client);
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", url);

        var stopwatch = Stopwatch.StartNew();
        DifyMetrics.ActiveRequests.Add(1);

        try
        {
            Logger.LogDebug("GET request to {Url}", url);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (timeout.HasValue)
            {
                activity?.SetTag("http.timeout_ms", timeout.Value.TotalMilliseconds);
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeout.Value);
                var response = await HttpClient.SendAsync(request, cts.Token).ConfigureAwait(false);
                await EnsureSuccessAsync(response, cts.Token).ConfigureAwait(false);

                var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cts.Token).ConfigureAwait(false)
                    ?? throw new DifyApiException(NullDeserializationError);

                RecordSuccess(activity, stopwatch, url, "GET", (int)response.StatusCode);
                Logger.LogDebug("GET request to {Url} completed successfully", url);
                return result;
            }
            else
            {
                var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

                var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken).ConfigureAwait(false)
                    ?? throw new DifyApiException(NullDeserializationError);

                RecordSuccess(activity, stopwatch, url, "GET", (int)response.StatusCode);
                Logger.LogDebug("GET request to {Url} completed successfully", url);
                return result;
            }
        }
        catch (Exception ex)
        {
            RecordError(activity, stopwatch, url, "GET", ex);
            throw;
        }
        finally
        {
            DifyMetrics.ActiveRequests.Add(-1);
        }
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(
        string url,
        TRequest request,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = DifyActivitySource.Instance.StartActivity($"POST {url}", ActivityKind.Client);
        activity?.SetTag("http.method", "POST");
        activity?.SetTag("http.url", url);

        var stopwatch = Stopwatch.StartNew();
        DifyMetrics.ActiveRequests.Add(1);

        try
        {
            Logger.LogDebug("POST request to {Url}", url);

            if (timeout.HasValue)
            {
                activity?.SetTag("http.timeout_ms", timeout.Value.TotalMilliseconds);
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeout.Value);

                var response = await HttpClient.PostAsJsonAsync(url, request, JsonOptions, cts.Token).ConfigureAwait(false);
                await EnsureSuccessAsync(response, cts.Token).ConfigureAwait(false);

                var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cts.Token).ConfigureAwait(false)
                    ?? throw new DifyApiException(NullDeserializationError);

                RecordSuccess(activity, stopwatch, url, "POST", (int)response.StatusCode);
                Logger.LogDebug("POST request to {Url} completed successfully", url);
                return result;
            }
            else
            {
                var response = await HttpClient.PostAsJsonAsync(url, request, JsonOptions, cancellationToken).ConfigureAwait(false);
                await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

                var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken).ConfigureAwait(false)
                    ?? throw new DifyApiException(NullDeserializationError);

                RecordSuccess(activity, stopwatch, url, "POST", (int)response.StatusCode);
                Logger.LogDebug("POST request to {Url} completed successfully", url);
                return result;
            }
        }
        catch (Exception ex)
        {
            RecordError(activity, stopwatch, url, "POST", ex);
            throw;
        }
        finally
        {
            DifyMetrics.ActiveRequests.Add(-1);
        }
    }

    protected async Task PostAsync<TRequest>(
        string url,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("POST request to {Url} (no response body)", url);

        var response = await HttpClient.PostAsJsonAsync(url, request, JsonOptions, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        Logger.LogDebug("POST request to {Url} completed successfully", url);
    }

    protected async Task<TResponse> PostAsync<TResponse>(
        string url,
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("POST request to {Url} with custom content", url);

        var response = await HttpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken).ConfigureAwait(false)
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

        var response = await HttpClient.PutAsJsonAsync(url, request, JsonOptions, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken).ConfigureAwait(false)
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
        var response = await HttpClient.DeleteAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        Logger.LogDebug("DELETE request to {Url} completed successfully", url);
    }

    private async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

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

    private static void RecordSuccess(Activity? activity, Stopwatch stopwatch, string url, string method, int statusCode)
    {
        stopwatch.Stop();
        var duration = stopwatch.Elapsed.TotalMilliseconds;

        activity?.SetTag("http.status_code", statusCode);
        activity?.SetTag("http.url", url);
        activity?.SetStatus(ActivityStatusCode.Ok);

        DifyMetrics.RequestCount.Add(1, new KeyValuePair<string, object?>("http.method", method),
                new KeyValuePair<string, object?>("http.status_code", statusCode));
        DifyMetrics.RequestDuration.Record(duration, new KeyValuePair<string, object?>("http.method", method),
     new KeyValuePair<string, object?>("http.status_code", statusCode));
    }

    private static void RecordError(Activity? activity, Stopwatch stopwatch, string url, string method, Exception exception)
    {
        stopwatch.Stop();
        var duration = stopwatch.Elapsed.TotalMilliseconds;

        activity?.SetTag("error", true);
        activity?.SetTag("http.url", url);
        activity?.SetTag("exception.type", exception.GetType().FullName);
        activity?.SetTag("exception.message", exception.Message);
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);

        var statusCode = exception is DifyApiException difyEx && difyEx.StatusCode.HasValue ? difyEx.StatusCode.Value : 0;

        DifyMetrics.RequestErrors.Add(1, new KeyValuePair<string, object?>("http.method", method),
        new KeyValuePair<string, object?>("error.type", exception.GetType().Name));
        DifyMetrics.RequestDuration.Record(duration, new KeyValuePair<string, object?>("http.method", method),
     new KeyValuePair<string, object?>("http.status_code", statusCode),
        new KeyValuePair<string, object?>("error", true));
    }
}
