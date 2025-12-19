using ModularMonolith.Modules.Storage.Domain.Enums;

namespace ModularMonolith.Modules.Storage.Application.Services;

public interface IVirusScanService
{
    Task<(FileScanStatus Status, string? Result)> ScanFileAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
