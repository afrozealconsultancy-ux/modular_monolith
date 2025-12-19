using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Storage.Domain.Entities;
using ModularMonolith.Modules.Storage.Domain.ValueObjects;

namespace ModularMonolith.Modules.Storage.Infrastructure.Persistence.Configurations;

internal sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("files");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.FileSize)
            .IsRequired();

        builder.Property(f => f.MimeType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.SeaweedFsFileId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.UploadedBy)
            .IsRequired();

        builder.Property(f => f.UploadedAt)
            .IsRequired();

        builder.Property(f => f.ScanStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(f => f.ScanResult)
            .HasMaxLength(1000);

        builder.Property(f => f.ScannedAt);

        builder.Property(f => f.IsDeleted)
            .IsRequired();

        builder.Property(f => f.DeletedAt);

        builder.Property(f => f.TenantId)
            .IsRequired();

        // Owned collection for metadata
        builder.OwnsMany(f => f.Metadata, metadata =>
        {
            metadata.ToTable("file_metadata");

            metadata.WithOwner().HasForeignKey("FileId");

            metadata.Property<Guid>("FileId");
            metadata.HasKey("FileId", "Key");

            metadata.Property(m => m.Key)
                .IsRequired()
                .HasMaxLength(100);

            metadata.Property(m => m.Value)
                .HasMaxLength(1000);
        });

        // Indexes
        builder.HasIndex(f => f.TenantId);
        builder.HasIndex(f => f.SeaweedFsFileId).IsUnique();
        builder.HasIndex(f => f.UploadedBy);
        builder.HasIndex(f => f.UploadedAt);
        builder.HasIndex(f => f.IsDeleted);
    }
}
