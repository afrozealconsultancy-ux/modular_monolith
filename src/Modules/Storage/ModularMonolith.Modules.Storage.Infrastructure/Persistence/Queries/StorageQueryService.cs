using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Storage.Application.Queries;
using ModularMonolith.Modules.Storage.Application.Queries.GetFile;
using ModularMonolith.Modules.Storage.Application.Queries.GetFiles;

namespace ModularMonolith.Modules.Storage.Infrastructure.Persistence.Queries;

internal sealed class StorageQueryService : IStorageQueryService
{
    private readonly StorageDbContext _dbContext;

    public StorageQueryService(StorageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<FileDto>> GetFilesAsync(
        int page,
        int pageSize,
        bool includeDeleted,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Files.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(f => !f.IsDeleted);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var files = await query
            .OrderByDescending(f => f.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FileDto(
                f.Id,
                f.FileName,
                f.FileSize,
                f.MimeType,
                f.ScanStatus.ToString(),
                f.UploadedBy,
                f.UploadedAt,
                f.IsDeleted))
            .ToListAsync(cancellationToken);

        return new PagedResult<FileDto>(files, page, pageSize, totalCount);
    }
}
