namespace ModularMonolith.Modules.Storage.Domain.Enums;

public enum FileScanStatus
{
    NotScanned = 0,
    Scanning = 1,
    Clean = 2,
    Infected = 3,
    ScanFailed = 4
}
