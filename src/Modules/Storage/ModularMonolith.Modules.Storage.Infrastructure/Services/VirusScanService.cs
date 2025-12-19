using ModularMonolith.Modules.Storage.Application.Services;
using ModularMonolith.Modules.Storage.Domain.Enums;

namespace ModularMonolith.Modules.Storage.Infrastructure.Services;

/// <summary>
/// Stub implementation of IVirusScanService for ClamAV integration.
/// TODO: Implement actual ClamAV client integration
/// </summary>
internal sealed class VirusScanService : IVirusScanService
{
    public Task<(FileScanStatus Status, string? Result)> ScanFileAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual ClamAV scanning
        // For now, always return Clean status
        return Task.FromResult<(FileScanStatus, string?)>((FileScanStatus.Clean, "No threats detected (mock scan)"));
    }
}
