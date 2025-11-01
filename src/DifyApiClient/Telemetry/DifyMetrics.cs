using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace DifyApiClient.Telemetry;

/// <summary>
/// Metrics for DifyApiClient operations
/// </summary>
internal static class DifyMetrics
{
    private static readonly AssemblyName AssemblyName = typeof(DifyMetrics).Assembly.GetName();
    
    /// <summary>
    /// The name of the meter
    /// </summary>
    public static readonly string MeterName = AssemblyName.Name ?? "DifyApiClient";
    
    /// <summary>
    /// The version of the meter
    /// </summary>
    public static readonly string MeterVersion = AssemblyName.Version?.ToString() ?? "1.0.0";
    
    /// <summary>
    /// The meter instance
    /// </summary>
    public static readonly Meter Instance = new(MeterName, MeterVersion);
    
    /// <summary>
    /// Counter for total API requests
    /// </summary>
    public static readonly Counter<long> RequestCount = Instance.CreateCounter<long>(
        "dify.client.requests",
        unit: "{request}",
        description: "Total number of API requests made");
    
    /// <summary>
    /// Histogram for request duration
    /// </summary>
    public static readonly Histogram<double> RequestDuration = Instance.CreateHistogram<double>(
        "dify.client.request.duration",
        unit: "ms",
        description: "Duration of API requests in milliseconds");
    
    /// <summary>
    /// Counter for failed requests
    /// </summary>
    public static readonly Counter<long> RequestErrors = Instance.CreateCounter<long>(
        "dify.client.request.errors",
        unit: "{error}",
        description: "Total number of failed API requests");
    
    /// <summary>
    /// Counter for streaming operations
    /// </summary>
    public static readonly Counter<long> StreamingOperations = Instance.CreateCounter<long>(
        "dify.client.streaming.operations",
        unit: "{operation}",
        description: "Total number of streaming operations");
    
    /// <summary>
    /// Counter for streaming chunks received
    /// </summary>
    public static readonly Counter<long> StreamingChunks = Instance.CreateCounter<long>(
        "dify.client.streaming.chunks",
        unit: "{chunk}",
        description: "Total number of streaming chunks received");
    
    /// <summary>
    /// Histogram for file upload size
    /// </summary>
    public static readonly Histogram<long> FileUploadSize = Instance.CreateHistogram<long>(
        "dify.client.file.upload.size",
        unit: "bytes",
        description: "Size of uploaded files in bytes");
    
    /// <summary>
    /// UpDownCounter for active requests
    /// </summary>
    public static readonly UpDownCounter<int> ActiveRequests = Instance.CreateUpDownCounter<int>(
        "dify.client.requests.active",
        unit: "{request}",
        description: "Number of currently active API requests");
}
