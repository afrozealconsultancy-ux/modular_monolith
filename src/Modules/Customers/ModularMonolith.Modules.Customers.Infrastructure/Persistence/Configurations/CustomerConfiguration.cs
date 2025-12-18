using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Customers.Domain.Entities;
using ModularMonolith.Modules.Customers.Domain.ValueObjects;

namespace ModularMonolith.Modules.Customers.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Customer entity.
/// </summary>
internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        // Owned collection - Addresses
        builder.OwnsMany(c => c.Addresses, address =>
        {
            address.ToTable("customer_addresses");

            address.WithOwner().HasForeignKey("CustomerId");

            address.HasKey(nameof(Address.Id));

            address.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(500);

            address.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.Type)
                .IsRequired()
                .HasConversion<int>();
        });

        // Indexes
        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => new { c.TenantId, c.Email }).IsUnique();
        builder.HasIndex(c => c.CreatedAt);
        builder.HasIndex(c => c.Status);

        // Query filter applied in BaseDbContext for IHasTenant
    }
}
