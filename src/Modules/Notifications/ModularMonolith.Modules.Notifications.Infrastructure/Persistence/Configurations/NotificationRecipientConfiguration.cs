using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Notifications.Domain.Entities;

namespace ModularMonolith.Modules.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
{
    public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
    {
        builder.ToTable("notification_recipients");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.NotificationId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.TenantId)
            .IsRequired();

        builder.Property(r => r.DeliveryChannel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.RecipientAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.DeliveryStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.SentAt);

        builder.Property(r => r.FailureReason)
            .HasMaxLength(1000);

        builder.Property(r => r.RetryCount)
            .IsRequired();

        builder.Property(r => r.IsRead)
            .IsRequired();

        builder.Property(r => r.ReadAt);

        // Relationship
        builder.HasOne(r => r.Notification)
            .WithMany()
            .HasForeignKey(r => r.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.NotificationId);
        builder.HasIndex(r => new { r.UserId, r.TenantId });
        builder.HasIndex(r => r.DeliveryStatus);
        builder.HasIndex(r => r.IsRead);
        builder.HasIndex(r => r.CreatedAt);
    }
}
