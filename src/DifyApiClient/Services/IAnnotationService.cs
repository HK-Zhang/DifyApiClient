using DifyApiClient.Models;

namespace DifyApiClient.Services;

/// <summary>
/// Service for annotation operations
/// </summary>
public interface IAnnotationService
{
    /// <summary>
    /// Get annotation list
    /// </summary>
    Task<AnnotationListResponse> GetAnnotationsAsync(
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create an annotation
    /// </summary>
    Task<Annotation> CreateAnnotationAsync(
        AnnotationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an annotation
    /// </summary>
    Task<Annotation> UpdateAnnotationAsync(
        string annotationId,
        AnnotationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an annotation
    /// </summary>
    Task DeleteAnnotationAsync(
        string annotationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable or disable annotation reply settings
    /// </summary>
    Task<AnnotationReplyJobResponse> SetAnnotationReplyAsync(
        string action,
        AnnotationReplySettingsRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Query annotation reply settings task status
    /// </summary>
    Task<AnnotationReplyJobResponse> GetAnnotationReplyStatusAsync(
        string action,
        string jobId,
        CancellationToken cancellationToken = default);
}
