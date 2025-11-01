using DifyApiClient.Core;
using DifyApiClient.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of application service
/// </summary>
internal class ApplicationService(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger? logger = null) : BaseApiClient(httpClient, jsonOptions, logger), IApplicationService
{
    public async Task<ApplicationInfo> GetApplicationInfoAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<ApplicationInfo>("info", cancellationToken);
    }

    public async Task<ApplicationParameters> GetApplicationParametersAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<ApplicationParameters>("parameters", cancellationToken);
    }

    public async Task<ApplicationMeta> GetApplicationMetaAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<ApplicationMeta>("meta", cancellationToken);
    }

    public async Task<ApplicationSite> GetApplicationSiteAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<ApplicationSite>("site", cancellationToken);
    }
}
