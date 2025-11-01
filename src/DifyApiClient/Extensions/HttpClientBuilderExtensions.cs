using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace DifyApiClient.Extensions;

/// <summary>
/// Extension methods for configuring HttpClient resilience policies
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds standard retry and circuit breaker policies to DifyApiClient
    /// </summary>
    /// <param name="builder">The IHttpClientBuilder</param>
    /// <param name="retryCount">Number of retry attempts (default: 3)</param>
    /// <param name="circuitBreakerThreshold">Number of failures before opening circuit (default: 5)</param>
    /// <param name="circuitBreakerDuration">Duration to keep circuit open in seconds (default: 30)</param>
    /// <returns>The IHttpClientBuilder for chaining</returns>
    public static IHttpClientBuilder AddStandardResiliencePolicies(
        this IHttpClientBuilder builder,
        int retryCount = 3,
        int circuitBreakerThreshold = 5,
        int circuitBreakerDuration = 30)
    {
        // Retry policy with exponential backoff
        builder.AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Log retry attempts if needed
                    var message = outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase ?? "Unknown error";
                    Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds}s due to: {message}");
                }));

        // Circuit breaker policy
        builder.AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: circuitBreakerThreshold,
                durationOfBreak: TimeSpan.FromSeconds(circuitBreakerDuration),
                onBreak: (outcome, duration) =>
                {
                    var message = outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase ?? "Unknown error";
                    Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds}s due to: {message}");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker reset");
                }));

        return builder;
    }

    /// <summary>
    /// Adds a custom retry policy to DifyApiClient
    /// </summary>
    /// <param name="builder">The IHttpClientBuilder</param>
    /// <param name="retryPolicy">Custom Polly retry policy</param>
    /// <returns>The IHttpClientBuilder for chaining</returns>
    public static IHttpClientBuilder AddCustomRetryPolicy(
        this IHttpClientBuilder builder,
        IAsyncPolicy<HttpResponseMessage> retryPolicy)
    {
        builder.AddPolicyHandler(retryPolicy);
        return builder;
    }

    /// <summary>
    /// Adds a timeout policy to DifyApiClient
    /// </summary>
    /// <param name="builder">The IHttpClientBuilder</param>
    /// <param name="timeoutSeconds">Timeout in seconds (default: 30)</param>
    /// <returns>The IHttpClientBuilder for chaining</returns>
    public static IHttpClientBuilder AddTimeoutPolicy(
        this IHttpClientBuilder builder,
        int timeoutSeconds = 30)
    {
        builder.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(timeoutSeconds)));
        return builder;
    }
}
