using DifyApiClient.Models;

namespace DifyApiClient.Services;

/// <summary>
/// Service for application information operations
/// </summary>
public interface IApplicationService
{
    /// <summary>
    /// Get application basic information
    /// </summary>
    Task<ApplicationInfo> GetApplicationInfoAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get application parameters
    /// </summary>
    Task<ApplicationParameters> GetApplicationParametersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get application meta information
    /// </summary>
    Task<ApplicationMeta> GetApplicationMetaAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get application WebApp settings
    /// </summary>
    Task<ApplicationSite> GetApplicationSiteAsync(
        CancellationToken cancellationToken = default);
}
