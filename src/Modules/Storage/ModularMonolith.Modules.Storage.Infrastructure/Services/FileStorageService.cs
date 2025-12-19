using ModularMonolith.Modules.Storage.Application.Services;

namespace ModularMonolith.Modules.Storage.Infrastructure.Services;

/// <summary>
/// Stub implementation of IFileStorageService for SeaweedFS integration.
/// TODO: Implement actual SeaweedFS client integration
/// </summary>
internal sealed class FileStorageService : IFileStorageService
{
    public Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual SeaweedFS upload
        // For now, return a mock file ID
        var mockFileId = $"seaweedfs_{Guid.NewGuid():N}_{fileName}";
        return Task.FromResult(mockFileId);
    }

    public Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual SeaweedFS download
        throw new NotImplementedException("SeaweedFS download not yet implemented");
    }

    public Task DeleteFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual SeaweedFS deletion
        // For now, just return completed task
        return Task.CompletedTask;
    }
}
