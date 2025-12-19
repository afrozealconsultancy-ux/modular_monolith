using ModularMonolith.Modules.Storage.Application.Queries.GetFile;
using ModularMonolith.Shared.Abstractions.CQRS;

namespace ModularMonolith.Modules.Storage.Application.Queries.GetFiles;

public sealed record GetFilesQuery(
    int Page = 1,
    int PageSize = 10,
    bool IncludeDeleted = false) : IQuery<Result<PagedResult<FileDto>>>;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
