using DifyApiClient.Telemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace DifyApiClient.Extensions;

/// <summary>
/// Extension methods for configuring OpenTelemetry with DifyApiClient
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds DifyApiClient tracing to OpenTelemetry
    /// </summary>
    /// <param name="builder">The TracerProviderBuilder</param>
    /// <returns>The TracerProviderBuilder for chaining</returns>
    public static TracerProviderBuilder AddDifyApiClientInstrumentation(this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        return builder.AddSource(DifyActivitySource.Name);
    }
    
    /// <summary>
    /// Adds DifyApiClient metrics to OpenTelemetry
    /// </summary>
    /// <param name="builder">The MeterProviderBuilder</param>
    /// <returns>The MeterProviderBuilder for chaining</returns>
    public static MeterProviderBuilder AddDifyApiClientInstrumentation(this MeterProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        return builder.AddMeter(DifyMetrics.MeterName);
    }
}
