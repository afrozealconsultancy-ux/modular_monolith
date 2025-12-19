using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Storage.Domain.Entities;
using ModularMonolith.Modules.Storage.Domain.Repositories;
using ModularMonolith.Shared.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Storage.Infrastructure.Persistence.Repositories;

internal sealed class StoredFileRepository : Repository<StoredFile, Guid>, IStoredFileRepository
{
    public StoredFileRepository(StorageDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<StoredFile?> GetBySeaweedFsFileIdAsync(string seaweedFsFileId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<StoredFile>()
            .FirstOrDefaultAsync(f => f.SeaweedFsFileId == seaweedFsFileId, cancellationToken);
    }

    public async Task<bool> ExistsWithSeaweedFsFileIdAsync(string seaweedFsFileId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<StoredFile>()
            .AnyAsync(f => f.SeaweedFsFileId == seaweedFsFileId, cancellationToken);
    }
}
