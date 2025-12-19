using ModularMonolith.Modules.Storage.Application.Queries.GetFile;
using ModularMonolith.Modules.Storage.Application.Queries.GetFiles;

namespace ModularMonolith.Modules.Storage.Application.Queries;

public interface IStorageQueryService
{
    Task<PagedResult<FileDto>> GetFilesAsync(
        int page,
        int pageSize,
        bool includeDeleted,
        CancellationToken cancellationToken = default);
}
