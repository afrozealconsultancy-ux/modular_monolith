using ModularMonolith.Shared.Abstractions.CQRS;

namespace ModularMonolith.Modules.Storage.Application.Commands.UploadFile;

public sealed record UploadFileCommand(
    string FileName,
    long FileSize,
    string MimeType,
    Stream FileStream,
    Dictionary<string, string>? Metadata = null) : ICommand<Result<Guid>>;
