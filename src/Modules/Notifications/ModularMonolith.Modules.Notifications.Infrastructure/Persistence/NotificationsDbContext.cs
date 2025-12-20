using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Notifications.Domain.Entities;
using ModularMonolith.Shared.Abstractions.Kernel;
using ModularMonolith.Shared.Abstractions.Tenancy;
using ModularMonolith.Shared.Abstractions.Time;
using ModularMonolith.Shared.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext : BaseDbContext, IUnitOfWork
{
    public NotificationsDbContext(
        DbContextOptions<NotificationsDbContext> options,
        ITenantContext tenantContext,
        IDateTimeProvider dateTimeProvider)
        : base(options, tenantContext, dateTimeProvider)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationRecipient> NotificationRecipients => Set<NotificationRecipient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
