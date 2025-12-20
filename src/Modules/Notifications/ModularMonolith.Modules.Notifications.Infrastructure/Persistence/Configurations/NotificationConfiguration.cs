using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Notifications.Domain.Entities;

namespace ModularMonolith.Modules.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Scope)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Subject)
            .HasMaxLength(500);

        builder.Property(n => n.Content)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(n => n.CreatedByTenantId);

        builder.Property(n => n.CreatedByUserId)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.ScheduledFor);

        builder.Property(n => n.IsProcessed)
            .IsRequired();

        builder.Property(n => n.ProcessedAt);

        // Target Tenant IDs as JSON
        builder.Property(n => n.TargetTenantIds)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>());

        // Recipients collection
        builder.HasMany<NotificationRecipient>()
            .WithOne(r => r.Notification)
            .HasForeignKey(r => r.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(n => n.Scope);
        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => n.CreatedByTenantId);
        builder.HasIndex(n => n.CreatedByUserId);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.IsProcessed);
        builder.HasIndex(n => n.ScheduledFor);
    }
}
