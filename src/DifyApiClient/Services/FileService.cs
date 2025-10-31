using DifyApiClient.Core;
using DifyApiClient.Models;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of file service
/// </summary>
internal class FileService : BaseApiClient, IFileService
{
    public FileService(HttpClient httpClient, JsonSerializerOptions jsonOptions)
        : base(httpClient, jsonOptions)
    {
    }

    public async Task<FileUploadResponse> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        content.Add(new StringContent(user), "user");

        return await PostAsync<FileUploadResponse>(
            "files/upload",
            content,
            cancellationToken);
    }
}
