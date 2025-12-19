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

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Recipient)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.Subject)
            .HasMaxLength(500);

        builder.Property(n => n.Content)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.SentAt);

        builder.Property(n => n.FailureReason)
            .HasMaxLength(1000);

        builder.Property(n => n.RetryCount)
            .IsRequired();

        builder.Property(n => n.TenantId)
            .IsRequired();

        // Indexes
        builder.HasIndex(n => n.TenantId);
        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.Recipient);
    }
}
