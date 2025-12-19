namespace ModularMonolith.Modules.Storage.Application.Services;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileId, CancellationToken cancellationToken = default);
}
