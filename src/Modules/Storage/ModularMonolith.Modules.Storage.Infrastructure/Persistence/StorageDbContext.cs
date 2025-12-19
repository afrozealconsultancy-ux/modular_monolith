using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Storage.Domain.Entities;
using ModularMonolith.Shared.Abstractions.Kernel;
using ModularMonolith.Shared.Abstractions.Time;
using ModularMonolith.Shared.Infrastructure.Persistence;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Storage.Infrastructure.Persistence;

public sealed class StorageDbContext : BaseDbContext, IUnitOfWork
{
    public StorageDbContext(
        DbContextOptions<StorageDbContext> options,
        ITenantContext tenantContext,
        IDateTimeProvider dateTimeProvider)
        : base(options, tenantContext, dateTimeProvider)
    {
    }

    public DbSet<StoredFile> Files => Set<StoredFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("storage");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StorageDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
