using System.Diagnostics;
using System.Reflection;

namespace DifyApiClient.Telemetry;

/// <summary>
/// Activity source for distributed tracing in DifyApiClient
/// </summary>
internal static class DifyActivitySource
{
    private static readonly AssemblyName AssemblyName = typeof(DifyActivitySource).Assembly.GetName();
    
    /// <summary>
    /// The name of the activity source
    /// </summary>
    public static readonly string Name = AssemblyName.Name ?? "DifyApiClient";
    
    /// <summary>
    /// The version of the activity source
    /// </summary>
    public static readonly string Version = AssemblyName.Version?.ToString() ?? "1.0.0";
    
    /// <summary>
    /// The activity source instance
    /// </summary>
    public static readonly ActivitySource Instance = new(Name, Version);
}
