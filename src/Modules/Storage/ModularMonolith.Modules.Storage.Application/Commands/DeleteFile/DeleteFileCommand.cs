using ModularMonolith.Shared.Abstractions.CQRS;

namespace ModularMonolith.Modules.Storage.Application.Commands.DeleteFile;

public sealed record DeleteFileCommand(Guid FileId) : ICommand<Result>;
