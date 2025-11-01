# OpenTelemetry & Metrics Guide

DifyApiClient provides comprehensive OpenTelemetry integration for distributed tracing and metrics collection, enabling full observability of your API interactions.

## Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Distributed Tracing](#distributed-tracing)
- [Metrics](#metrics)
- [ASP.NET Core Integration](#aspnet-core-integration)
- [Exporters](#exporters)
- [Best Practices](#best-practices)

## Overview

DifyApiClient includes built-in instrumentation for:
- **Distributed Tracing** - Track requests across services with W3C Trace Context
- **Metrics** - Monitor performance, errors, and usage patterns
- **Activity Tags** - Rich contextual information for debugging

## Quick Start

### Installation

The OpenTelemetry packages are already included as dependencies. For exporting data, install your preferred exporters:

```bash
# Console exporter (for development)
dotnet add package OpenTelemetry.Exporter.Console

# OTLP exporter (for production - Jaeger, Tempo, etc.)
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol

# Prometheus exporter (for metrics)
dotnet add package OpenTelemetry.Exporter.Prometheus.AspNetCore
```

### Basic Setup

```csharp
using DifyApiClient.Extensions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add DifyApiClient
builder.Services.AddDifyApiClient(options =>
{
    options.BaseUrl = configuration["Dify:BaseUrl"]!;
    options.ApiKey = configuration["Dify:ApiKey"]!;
});

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("MyApplication"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddDifyApiClientInstrumentation()  // Add DifyApiClient tracing
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddDifyApiClientInstrumentation()  // Add DifyApiClient metrics
        .AddConsoleExporter());

var app = builder.Build();
```

## Distributed Tracing

### Activity Names

DifyApiClient creates activities (spans) for each API operation:

- `GET {endpoint}` - GET requests
- `POST {endpoint}` - POST requests  
- `PUT {endpoint}` - PUT requests
- `DELETE {endpoint}` - DELETE requests
- `POST chat-messages (streaming)` - Streaming chat operations

### Activity Tags

Each activity includes rich contextual tags:

| Tag | Description | Example |
|-----|-------------|---------|
| `http.method` | HTTP method | `POST` |
| `http.url` | Request URL | `chat-messages` |
| `http.status_code` | Response status code | `200` |
| `http.timeout_ms` | Request timeout (if overridden) | `30000` |
| `streaming` | Whether request is streaming | `true` |
| `chunk_count` | Number of chunks received (streaming) | `25` |
| `error` | Whether request failed | `true` |
| `exception.type` | Exception type name | `DifyApiException` |
| `exception.message` | Exception message | `Rate limit exceeded` |

### Example Trace Output

```
Activity.DisplayName:       POST chat-messages
Activity.Kind:              Client
Activity.StartTime:         2025-11-01T10:30:00.1234567Z
Activity.Duration:          00:00:01.2345678
Activity.Tags:
    http.method:            POST
    http.url:               chat-messages
    http.status_code:       200
Activity.Status:            Ok
```

### Viewing Traces

#### With Console Exporter (Development)

```csharp
.WithTracing(tracing => tracing
    .AddDifyApiClientInstrumentation()
    .AddConsoleExporter())
```

Output:
```
Activity.DisplayName: POST chat-messages
Activity.Tags:
    http.method: POST
    http.url: chat-messages
    http.status_code: 200
```

#### With Jaeger (Production)

```csharp
.WithTracing(tracing => tracing
    .AddDifyApiClientInstrumentation()
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://jaeger:4317");
    }))
```

Then view in Jaeger UI at `http://localhost:16686`

## Metrics

### Available Metrics

#### Request Metrics

**`dify.client.requests`** (Counter)
- Total number of API requests
- Tags: `http.method`, `http.status_code`

**`dify.client.request.duration`** (Histogram)
- Request duration in milliseconds
- Tags: `http.method`, `http.status_code`
- Unit: `ms`

**`dify.client.request.errors`** (Counter)
- Total number of failed requests
- Tags: `http.method`, `error.type`

**`dify.client.requests.active`** (UpDownCounter)
- Number of currently active requests
- Real-time concurrency tracking

#### Streaming Metrics

**`dify.client.streaming.operations`** (Counter)
- Total streaming operations started
- Tags: `operation` (e.g., `chat`)

**`dify.client.streaming.chunks`** (Counter)
- Total streaming chunks received
- Tags: `operation`

#### File Metrics

**`dify.client.file.upload.size`** (Histogram)
- Size of uploaded files in bytes
- Tags: `file_name`
- Unit: `bytes`

### Example Metrics Queries

#### Prometheus

```promql
# Request rate per method
rate(dify_client_requests_total[5m])

# Average request duration
rate(dify_client_request_duration_sum[5m]) / rate(dify_client_request_duration_count[5m])

# Error rate
rate(dify_client_request_errors_total[5m])

# Active requests
dify_client_requests_active

# Streaming chunks per second
rate(dify_client_streaming_chunks_total[1m])
```

### Viewing Metrics

#### With Prometheus

```csharp
using OpenTelemetry.Exporter;

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddDifyApiClientInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

// Expose metrics endpoint
app.UseOpenTelemetryPrometheusScrapingEndpoint();
```

Access metrics at: `http://localhost:9090/metrics`

#### With Console Exporter (Development)

```csharp
.WithMetrics(metrics => metrics
    .AddDifyApiClientInstrumentation()
    .AddConsoleExporter())
```

Output:
```
Meter: DifyApiClient
Instrument: dify.client.requests
Value: 156
Tags: http.method=POST, http.status_code=200

Instrument: dify.client.request.duration
Value: 234.56 ms
Tags: http.method=POST, http.status_code=200
```

## ASP.NET Core Integration

### Complete Example

```csharp
using DifyApiClient.Extensions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add DifyApiClient with resilience
builder.Services.AddDifyApiClientWithResilience(options =>
{
    options.BaseUrl = builder.Configuration["Dify:BaseUrl"]!;
    options.ApiKey = builder.Configuration["Dify:ApiKey"]!;
});

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "MyApiService",
            serviceVersion: "1.0.0"))
    .WithTracing(tracing => tracing
        .SetSampler(new AlwaysOnSampler())
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddDifyApiClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"]!);
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddDifyApiClientInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapGet("/chat", async (IDifyApiClient client, string message) =>
{
    // This will be traced end-to-end
    var response = await client.SendChatMessageAsync(new ChatMessageRequest
    {
        Query = message,
        User = "api-user"
    });
    
    return Results.Ok(response);
});

app.Run();
```

### Configuration (appsettings.json)

```json
{
  "OpenTelemetry": {
    "Endpoint": "http://otel-collector:4317",
    "ServiceName": "MyApiService"
  },
  "Dify": {
    "BaseUrl": "https://api.dify.ai/v1",
    "ApiKey": "your-api-key"
  }
}
```

## Exporters

### OTLP Exporter (Recommended for Production)

Exports to OpenTelemetry Collector, Jaeger, Tempo, etc.

```bash
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
```

```csharp
.WithTracing(tracing => tracing
    .AddDifyApiClientInstrumentation()
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://otel-collector:4317");
        options.Protocol = OtlpExportProtocol.Grpc;
    }))
```

### Jaeger Exporter

Direct export to Jaeger (alternative to OTLP):

```bash
dotnet add package OpenTelemetry.Exporter.Jaeger
```

```csharp
.WithTracing(tracing => tracing
    .AddDifyApiClientInstrumentation()
    .AddJaegerExporter(options =>
    {
        options.AgentHost = "localhost";
        options.AgentPort = 6831;
    }))
```

### Zipkin Exporter

```bash
dotnet add package OpenTelemetry.Exporter.Zipkin
```

```csharp
.WithTracing(tracing => tracing
    .AddDifyApiClientInstrumentation()
    .AddZipkinExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
    }))
```

### Azure Monitor Exporter

For Azure Application Insights:

```bash
dotnet add package Azure.Monitor.OpenTelemetry.Exporter
```

```csharp
.WithTracing(tracing => tracing
    .AddDifyApiClientInstrumentation()
    .AddAzureMonitorTraceExporter(options =>
    {
        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    }))
```

## Best Practices

### 1. Use Sampling in Production

Don't trace every request - use sampling:

```csharp
.WithTracing(tracing => tracing
    .SetSampler(new TraceIdRatioBasedSampler(0.1)) // Sample 10% of traces
    .AddDifyApiClientInstrumentation())
```

### 2. Add Custom Tags

Add application-specific context:

```csharp
using System.Diagnostics;

var activity = Activity.Current;
activity?.SetTag("user.id", userId);
activity?.SetTag("tenant.id", tenantId);

var response = await difyClient.SendChatMessageAsync(request);
```

### 3. Use Resource Attributes

Identify your service:

```csharp
.ConfigureResource(resource => resource
    .AddService(
        serviceName: "MyService",
        serviceVersion: "1.0.0",
        serviceInstanceId: Environment.MachineName)
    .AddAttributes(new Dictionary<string, object>
    {
        ["environment"] = builder.Environment.EnvironmentName,
        ["deployment.region"] = "us-east-1"
    }))
```

### 4. Monitor Key Metrics

Set up alerts on:
- **Error rate**: `dify.client.request.errors`
- **Latency**: p95/p99 of `dify.client.request.duration`
- **Active requests**: `dify.client.requests.active` (detect leaks)
- **Request rate**: `dify.client.requests`

### 5. Correlate with Logs

Use Activity.TraceId in logs:

```csharp
var traceId = Activity.Current?.TraceId.ToString();
logger.LogInformation("Processing request {TraceId}", traceId);
```

### 6. Use Baggage for Cross-Service Context

```csharp
Baggage.SetBaggage("user.id", userId);
Baggage.SetBaggage("session.id", sessionId);
```

## Advanced Scenarios

### Custom Activity Source

Create additional spans for specific operations:

```csharp
using var activity = DifyActivitySource.Instance.StartActivity("ProcessChatResponse");
activity?.SetTag("response.length", response.Answer.Length);
activity?.SetTag("response.tokens", response.Metadata?.Usage?.TotalTokens);

// Process response
// ...

activity?.SetStatus(ActivityStatusCode.Ok);
```

### Metric Enrichment

Add custom dimensions to metrics:

```csharp
// This is done automatically by DifyApiClient
DifyMetrics.RequestCount.Add(1, 
    new KeyValuePair<string, object?>("http.method", "POST"),
    new KeyValuePair<string, object?>("custom.dimension", "value"));
```

### Distributed Context Propagation

Activities automatically propagate with W3C Trace Context:

```
traceparent: 00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01
```

This allows end-to-end tracing across:
- ASP.NET Core → DifyApiClient → Dify API

## Troubleshooting

### Issue: No traces appearing

**Solution**: Ensure exporter is configured and service is running:

```csharp
// Add console exporter for debugging
.AddConsoleExporter()
```

### Issue: Too many spans

**Solution**: Adjust sampling rate:

```csharp
.SetSampler(new TraceIdRatioBasedSampler(0.01)) // Sample 1%
```

### Issue: Missing HTTP client spans

**Solution**: Add HTTP client instrumentation:

```csharp
.AddHttpClientInstrumentation()
```

### Issue: Metrics not exported

**Solution**: Ensure periodic export is configured:

```csharp
.WithMetrics(metrics => metrics
    .AddDifyApiClientInstrumentation()
    .AddConsoleExporter(options =>
    {
        options.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;
    }))
```

## Example Dashboard Queries

### Grafana (with Prometheus)

```promql
# Request Rate
sum(rate(dify_client_requests_total[5m])) by (http_method)

# Error Rate
sum(rate(dify_client_request_errors_total[5m])) / sum(rate(dify_client_requests_total[5m]))

# P95 Latency
histogram_quantile(0.95, rate(dify_client_request_duration_bucket[5m]))

# Active Requests
dify_client_requests_active

# Streaming Throughput
rate(dify_client_streaming_chunks_total[1m])
```

## Complete Example

```csharp
using DifyApiClient.Extensions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// DI
builder.Services.AddDifyApiClientWithResilience(options =>
{
    options.BaseUrl = builder.Configuration["Dify:BaseUrl"]!;
    options.ApiKey = builder.Configuration["Dify:ApiKey"]!;
});

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("ChatService", "1.0.0"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddDifyApiClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddDifyApiClientInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapPost("/chat", async (ChatRequest req, IDifyApiClient client) =>
{
    var response = await client.SendChatMessageAsync(new ChatMessageRequest
    {
        Query = req.Message,
        User = req.UserId
    });
    
    return Results.Ok(response);
});

app.Run();
```

This provides full observability with distributed tracing and metrics for all DifyApiClient operations! 🚀
