using ModularMonolith.Shared.Abstractions.CQRS;

namespace ModularMonolith.Modules.Storage.Application.Queries.GetFile;

public sealed record GetFileQuery(Guid FileId) : IQuery<Result<FileDto>>;

public sealed record FileDto(
    Guid Id,
    string FileName,
    long FileSize,
    string MimeType,
    string ScanStatus,
    Guid UploadedBy,
    DateTime UploadedAt,
    bool IsDeleted);
