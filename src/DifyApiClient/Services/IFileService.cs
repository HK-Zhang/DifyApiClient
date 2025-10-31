using DifyApiClient.Models;

namespace DifyApiClient.Services;

/// <summary>
/// Service for file operations
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Upload a file
    /// </summary>
    Task<FileUploadResponse> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default);
}
