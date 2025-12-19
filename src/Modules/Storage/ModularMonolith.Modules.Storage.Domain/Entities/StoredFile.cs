using ModularMonolith.Modules.Storage.Domain.Enums;
using ModularMonolith.Modules.Storage.Domain.Events;
using ModularMonolith.Modules.Storage.Domain.ValueObjects;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Storage.Domain.Entities;

public sealed class StoredFile : TenantAggregateRoot
{
    private readonly List<FileMetadata> _metadata = new();

    private StoredFile(
        Guid id,
        Guid tenantId,
        string fileName,
        long fileSize,
        string mimeType,
        string seaweedFsFileId,
        Guid uploadedBy)
        : base(id, tenantId)
    {
        FileName = fileName;
        FileSize = fileSize;
        MimeType = mimeType;
        SeaweedFsFileId = seaweedFsFileId;
        UploadedBy = uploadedBy;
        UploadedAt = DateTime.UtcNow;
        ScanStatus = FileScanStatus.NotScanned;
        IsDeleted = false;
    }

    public string FileName { get; private set; }
    public long FileSize { get; private set; }
    public string MimeType { get; private set; }
    public string SeaweedFsFileId { get; private set; }
    public Guid UploadedBy { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public FileScanStatus ScanStatus { get; private set; }
    public string? ScanResult { get; private set; }
    public DateTime? ScannedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public IReadOnlyList<FileMetadata> Metadata => _metadata.AsReadOnly();

    public static StoredFile Create(
        Guid tenantId,
        string fileName,
        long fileSize,
        string mimeType,
        string seaweedFsFileId,
        Guid uploadedBy)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        if (fileSize <= 0)
            throw new ArgumentException("File size must be greater than zero", nameof(fileSize));

        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("MIME type cannot be empty", nameof(mimeType));

        if (string.IsNullOrWhiteSpace(seaweedFsFileId))
            throw new ArgumentException("SeaweedFS file ID cannot be empty", nameof(seaweedFsFileId));

        var file = new StoredFile(
            Guid.NewGuid(),
            tenantId,
            fileName,
            fileSize,
            mimeType,
            seaweedFsFileId,
            uploadedBy);

        file.RaiseDomainEvent(new FileUploadedDomainEvent(
            file.Id,
            file.TenantId,
            file.FileName,
            file.FileSize,
            file.MimeType,
            file.UploadedBy));

        return file;
    }

    public void AddMetadata(string key, string value)
    {
        var metadata = FileMetadata.Create(key, value);

        // Remove existing metadata with same key
        var existing = _metadata.FirstOrDefault(m => m.Key == key);
        if (existing != null)
        {
            _metadata.Remove(existing);
        }

        _metadata.Add(metadata);
    }

    public void CompleteScan(FileScanStatus scanStatus, string? scanResult = null)
    {
        if (scanStatus == FileScanStatus.NotScanned || scanStatus == FileScanStatus.Scanning)
            throw new InvalidOperationException("Invalid scan completion status");

        ScanStatus = scanStatus;
        ScanResult = scanResult;
        ScannedAt = DateTime.UtcNow;

        RaiseDomainEvent(new FileScanCompletedDomainEvent(
            Id,
            TenantId,
            ScanStatus,
            ScanResult));
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new InvalidOperationException("File is already deleted");

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new FileDeletedDomainEvent(
            Id,
            TenantId,
            FileName));
    }
}
