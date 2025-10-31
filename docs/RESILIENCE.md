# Resilience & Reliability Guide

DifyApiClient integrates with Polly to provide comprehensive resilience patterns including retry, circuit breaker, and timeout policies to handle transient faults and improve reliability.

## Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Standard Resilience Policies](#standard-resilience-policies)
- [Custom Policies](#custom-policies)
- [Best Practices](#best-practices)
- [Monitoring](#monitoring)

## Overview

Resilience policies help your application handle:
- **Transient network failures**: Temporary network issues
- **Service unavailability**: Dify API temporary downtime
- **Rate limiting**: 429 Too Many Requests responses
- **Timeouts**: Long-running requests
- **Server errors**: 5xx HTTP status codes

## Quick Start

### Enable Standard Resilience

```csharp
using DifyApiClient.Extensions;

builder.Services.AddDifyApiClientWithResilience(options =>
{
    options.BaseUrl = configuration["Dify:BaseUrl"]!;
    options.ApiKey = configuration["Dify:ApiKey"]!;
});
```

This automatically adds:
- ✅ Retry policy with exponential backoff (3 retries)
- ✅ Circuit breaker (opens after 5 failures for 30 seconds)
- ✅ Handles 5xx errors, network failures, and 429 rate limits

## Standard Resilience Policies

### Default Configuration

The `AddDifyApiClientWithResilience` method applies these policies:

#### 1. Retry Policy
- **Retry count**: 3 attempts
- **Backoff**: Exponential (2^retry seconds)
  - 1st retry: after 2 seconds
  - 2nd retry: after 4 seconds
  - 3rd retry: after 8 seconds
- **Triggers**: 5xx errors, network failures, 429 Too Many Requests

#### 2. Circuit Breaker
- **Failure threshold**: 5 consecutive failures
- **Break duration**: 30 seconds
- **Triggers**: Same as retry policy

### Custom Resilience Configuration

```csharp
services.AddDifyApiClient(options =>
{
    options.BaseUrl = "https://api.dify.ai/v1";
    options.ApiKey = "your-api-key";
})
.AddStandardResiliencePolicies(
    retryCount: 5,                    // 5 retry attempts
    circuitBreakerThreshold: 10,       // Open after 10 failures
    circuitBreakerDuration: 60         // Stay open for 60 seconds
);
```

## Custom Policies

### Custom Retry Policy

```csharp
using Polly;
using Polly.Extensions.Http;

var httpClientBuilder = services.AddDifyApiClient(options => { ... });

// Add custom retry with jitter
httpClientBuilder.AddCustomRetryPolicy(
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt =>
            {
                var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
                return baseDelay + jitter;
            },
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                var logger = context.GetLogger();
                logger?.LogWarning(
                    "Retry {RetryAttempt} after {Delay}ms. Reason: {Reason}",
                    retryAttempt,
                    timespan.TotalMilliseconds,
                    outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase ?? "Unknown"
                );
            }
        )
);
```

### Timeout Policy

```csharp
httpClientBuilder.AddTimeoutPolicy(timeoutSeconds: 30);
```

### Combined Policies

```csharp
var httpClientBuilder = services.AddDifyApiClient(options => { ... });

// Timeout
httpClientBuilder.AddTimeoutPolicy(30);

// Retry with exponential backoff
httpClientBuilder.AddPolicyHandler(
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
);

// Circuit breaker
httpClientBuilder.AddPolicyHandler(
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))
);
```

### Fallback Policy

```csharp
httpClientBuilder.AddPolicyHandler(
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .FallbackAsync(
            fallbackAction: async (cancellationToken) =>
            {
                // Return cached response or default response
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"answer\": \"Service temporarily unavailable\"}")
                };
            },
            onFallbackAsync: (outcome, context) =>
            {
                var logger = context.GetLogger();
                logger?.LogWarning("Fallback triggered due to: {Reason}",
                    outcome.Exception?.Message ?? "Unknown");
                return Task.CompletedTask;
            }
        )
);
```

## Best Practices

### 1. Use Standard Resilience in Production

```csharp
if (builder.Environment.IsProduction())
{
    builder.Services.AddDifyApiClientWithResilience(options => { ... });
}
else
{
    builder.Services.AddDifyApiClient(options => { ... });
}
```

### 2. Handle Rate Limiting Appropriately

```csharp
httpClientBuilder.AddPolicyHandler(
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: (retryAttempt, response, context) =>
            {
                // Check for Retry-After header
                if (response.Result?.Headers.RetryAfter?.Delta.HasValue == true)
                {
                    return response.Result.Headers.RetryAfter.Delta.Value;
                }
                
                // Default exponential backoff
                return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
            },
            onRetryAsync: async (outcome, timespan, retryAttempt, context) =>
            {
                var logger = context.GetLogger();
                logger?.LogWarning(
                    "Rate limited. Retry {RetryAttempt} after {Delay}s",
                    retryAttempt,
                    timespan.TotalSeconds
                );
                await Task.CompletedTask;
            }
        )
);
```

### 3. Don't Retry Non-Transient Errors

```csharp
httpClientBuilder.AddPolicyHandler(
    Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r =>
            r.StatusCode >= System.Net.HttpStatusCode.InternalServerError ||
            r.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
            r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
);
```

Don't retry:
- ❌ 400 Bad Request (client error)
- ❌ 401 Unauthorized (authentication issue)
- ❌ 403 Forbidden (authorization issue)
- ❌ 404 Not Found (resource doesn't exist)

Do retry:
- ✅ 408 Request Timeout
- ✅ 429 Too Many Requests
- ✅ 500 Internal Server Error
- ✅ 502 Bad Gateway
- ✅ 503 Service Unavailable
- ✅ 504 Gateway Timeout
- ✅ Network failures (HttpRequestException)

### 4. Set Appropriate Timeouts

```csharp
services.AddDifyApiClient(options =>
{
    options.BaseUrl = "https://api.dify.ai/v1";
    options.ApiKey = "your-api-key";
    options.Timeout = TimeSpan.FromSeconds(100); // Global timeout
})
.AddTimeoutPolicy(30); // Per-request timeout (shorter)
```

### 5. Monitor Circuit Breaker State

```csharp
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (outcome, duration, context) =>
        {
            var logger = context.GetLogger();
            logger?.LogError(
                "Circuit breaker opened for {Duration}s due to: {Reason}",
                duration.TotalSeconds,
                outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase ?? "Unknown"
            );
            
            // Send alert to monitoring system
            // metrics.IncrementCircuitBreakerOpened();
        },
        onReset: context =>
        {
            var logger = context.GetLogger();
            logger?.LogInformation("Circuit breaker reset");
            
            // metrics.IncrementCircuitBreakerReset();
        },
        onHalfOpen: () =>
        {
            // Circuit breaker is testing if the service is back
            // logger.LogInformation("Circuit breaker half-open, testing service");
        }
    );

httpClientBuilder.AddPolicyHandler(circuitBreakerPolicy);
```

## Monitoring

### Log Resilience Events

```csharp
httpClientBuilder.AddPolicyHandler(
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                var logger = context.GetLogger();
                var url = context["RequestUrl"] as string;
                
                logger?.LogWarning(
                    "Retry {RetryAttempt} for {Url} after {Delay}ms. Status: {StatusCode}, Error: {Error}",
                    retryAttempt,
                    url,
                    timespan.TotalMilliseconds,
                    outcome.Result?.StatusCode,
                    outcome.Exception?.Message
                );
            }
        )
);
```

### Metrics Collection

```csharp
// Using custom metrics service
public class ResilienceMetrics
{
    private readonly ILogger<ResilienceMetrics> _logger;
    private int _retryCount;
    private int _circuitBreakerOpenCount;
    
    public void RecordRetry(int retryAttempt, TimeSpan delay, string reason)
    {
        Interlocked.Increment(ref _retryCount);
        _logger.LogInformation(
            "Retry metric: Attempt={Attempt}, Delay={Delay}ms, Reason={Reason}, TotalRetries={TotalRetries}",
            retryAttempt, delay.TotalMilliseconds, reason, _retryCount
        );
    }
    
    public void RecordCircuitBreakerOpen(TimeSpan duration, string reason)
    {
        Interlocked.Increment(ref _circuitBreakerOpenCount);
        _logger.LogError(
            "Circuit breaker opened: Duration={Duration}s, Reason={Reason}, TotalOpens={TotalOpens}",
            duration.TotalSeconds, reason, _circuitBreakerOpenCount
        );
    }
}

// Register in DI
services.AddSingleton<ResilienceMetrics>();

// Use in policy
var metrics = serviceProvider.GetRequiredService<ResilienceMetrics>();

httpClientBuilder.AddPolicyHandler(
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                var reason = outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase ?? "Unknown";
                metrics.RecordRetry(retryAttempt, timespan, reason);
            }
        )
);
```

## Advanced Scenarios

### Bulkhead Isolation

Limit concurrent requests to protect resources:

```csharp
using Polly.Bulkhead;

var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
    maxParallelization: 10,
    maxQueuingActions: 20,
    onBulkheadRejectedAsync: context =>
    {
        var logger = context.GetLogger();
        logger?.LogWarning("Bulkhead rejected request - too many concurrent calls");
        return Task.CompletedTask;
    }
);

httpClientBuilder.AddPolicyHandler(bulkheadPolicy);
```

### Policy Wrap (Combining Policies)

```csharp
using Polly.Wrap;

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));

// Wrap policies: Timeout -> Retry -> Circuit Breaker
var policyWrap = Policy.WrapAsync(timeoutPolicy, retryPolicy, circuitBreakerPolicy);

httpClientBuilder.AddPolicyHandler(policyWrap);
```

## Testing Resilience

### Simulating Failures

```csharp
// In test environment, add a handler that simulates failures
if (builder.Environment.IsTesting())
{
    httpClientBuilder.AddHttpMessageHandler(() => new FailureSimulatorHandler());
}

public class FailureSimulatorHandler : DelegatingHandler
{
    private int _requestCount = 0;
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var count = Interlocked.Increment(ref _requestCount);
        
        // Fail first 3 requests
        if (count <= 3)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}
```

## Troubleshooting

### Issue: Too many retries causing delays

**Solution**: Reduce retry count or use shorter backoff:
```csharp
.AddStandardResiliencePolicies(retryCount: 2)
```

### Issue: Circuit breaker opens too frequently

**Solution**: Increase failure threshold or decrease break duration:
```csharp
.AddStandardResiliencePolicies(
    circuitBreakerThreshold: 10,
    circuitBreakerDuration: 15
)
```

### Issue: Requests timing out

**Solution**: Increase timeout or optimize requests:
```csharp
.AddTimeoutPolicy(timeoutSeconds: 60)
```
