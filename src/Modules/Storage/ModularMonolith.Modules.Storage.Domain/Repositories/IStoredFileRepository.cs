using ModularMonolith.Modules.Storage.Domain.Entities;
using ModularMonolith.Shared.Abstractions.Persistence;

namespace ModularMonolith.Modules.Storage.Domain.Repositories;

public interface IStoredFileRepository : IRepository<StoredFile, Guid>
{
    Task<StoredFile?> GetBySeaweedFsFileIdAsync(string seaweedFsFileId, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithSeaweedFsFileIdAsync(string seaweedFsFileId, CancellationToken cancellationToken = default);
}
