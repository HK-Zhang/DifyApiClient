using DifyApiClient.Core;
using DifyApiClient.Models;
using DifyApiClient.Telemetry;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DifyApiClient.Services;

/// <summary>
/// Implementation of file service
/// </summary>
internal class FileService(HttpClient httpClient, JsonSerializerOptions jsonOptions, ILogger? logger = null) : BaseApiClient(httpClient, jsonOptions, logger), IFileService
{
    public async Task<FileUploadResponse> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string user,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Uploading file: {FileName}", fileName);
        
        // Record file size metric
        if (fileStream.CanSeek)
        {
            DifyMetrics.FileUploadSize.Record(fileStream.Length, 
                new KeyValuePair<string, object?>("file_name", fileName));
        }
        
        using var content = new MultipartFormDataContent
        {
            { new StreamContent(fileStream), "file", fileName },
            { new StringContent(user), "user" }
        };

        var result = await PostAsync<FileUploadResponse>(
            "files/upload",
            content,
            cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("File uploaded successfully: {FileName}, ID: {FileId}", fileName, result.Id);
        return result;
    }
}
