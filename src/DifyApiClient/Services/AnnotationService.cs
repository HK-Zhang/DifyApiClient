using DifyApiClient.Core;
using DifyApiClient.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of annotation service
/// </summary>
internal class AnnotationService(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger? logger = null) : BaseApiClient(httpClient, jsonOptions, logger), IAnnotationService
{
    public async Task<AnnotationListResponse> GetAnnotationsAsync(
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<AnnotationListResponse>(
            $"apps/annotations?page={page}&limit={limit}",
            cancellationToken);
    }

    public async Task<Annotation> CreateAnnotationAsync(
        AnnotationRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<AnnotationRequest, Annotation>(
            "apps/annotations",
            request,
            cancellationToken);
    }

    public async Task<Annotation> UpdateAnnotationAsync(
        string annotationId,
        AnnotationRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PutAsync<AnnotationRequest, Annotation>(
            $"apps/annotations/{annotationId}",
            request,
            cancellationToken);
    }

    public async Task DeleteAnnotationAsync(
        string annotationId,
        CancellationToken cancellationToken = default)
    {
        await DeleteAsync($"apps/annotations/{annotationId}", cancellationToken);
    }

    public async Task<AnnotationReplyJobResponse> SetAnnotationReplyAsync(
        string action,
        AnnotationReplySettingsRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        if (action != "enable" && action != "disable")
            throw new ArgumentException("Action must be 'enable' or 'disable'", nameof(action));

        return await PostAsync<AnnotationReplySettingsRequest, AnnotationReplyJobResponse>(
            $"apps/annotation-reply/{action}",
            request ?? new AnnotationReplySettingsRequest(),
            cancellationToken);
    }

    public async Task<AnnotationReplyJobResponse> GetAnnotationReplyStatusAsync(
        string action,
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (action != "enable" && action != "disable")
            throw new ArgumentException("Action must be 'enable' or 'disable'", nameof(action));

        return await GetAsync<AnnotationReplyJobResponse>(
            $"apps/annotation-reply/{action}/status/{jobId}",
            cancellationToken);
    }
}
