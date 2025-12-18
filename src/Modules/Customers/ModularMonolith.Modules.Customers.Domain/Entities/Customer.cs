using ModularMonolith.Shared.Abstractions.Tenancy;
using ModularMonolith.Modules.Customers.Domain.Events;
using ModularMonolith.Modules.Customers.Domain.ValueObjects;

namespace ModularMonolith.Modules.Customers.Domain.Entities;

/// <summary>
/// Customer aggregate root - tenant-scoped.
/// </summary>
public sealed class Customer : TenantAggregateRoot
{
    private readonly List<Address> _addresses = new();

    private Customer(Guid id, Guid tenantId, string name, string email, string? phoneNumber)
        : base(id, tenantId)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        Status = CustomerStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private Customer() : base(Guid.Empty, Guid.Empty)
    {
        Name = string.Empty;
        Email = string.Empty;
    }

    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyList<Address> Addresses => _addresses.AsReadOnly();

    /// <summary>
    /// Factory method to create a new customer.
    /// </summary>
    public static Customer Create(Guid tenantId, string name, string email, string? phoneNumber = null)
    {
        // Business rules validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Customer email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        var customer = new Customer(Guid.NewGuid(), tenantId, name, email, phoneNumber);

        // Raise domain event
        customer.RaiseDomainEvent(new CustomerCreatedDomainEvent(
            customer.Id,
            customer.TenantId,
            customer.Name,
            customer.Email));

        return customer;
    }

    /// <summary>
    /// Update customer information.
    /// </summary>
    public void Update(string name, string email, string? phoneNumber)
    {
        if (Status == CustomerStatus.Deleted)
            throw new InvalidOperationException("Cannot update deleted customer");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Customer email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CustomerUpdatedDomainEvent(Id, TenantId, Name, Email));
    }

    /// <summary>
    /// Add address to customer.
    /// </summary>
    public void AddAddress(Address address)
    {
        if (Status == CustomerStatus.Deleted)
            throw new InvalidOperationException("Cannot add address to deleted customer");

        _addresses.Add(address);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove address from customer.
    /// </summary>
    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address != null)
        {
            _addresses.Remove(address);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Deactivate customer (soft delete).
    /// </summary>
    public void Deactivate()
    {
        if (Status == CustomerStatus.Deleted)
            throw new InvalidOperationException("Customer already deleted");

        Status = CustomerStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate customer.
    /// </summary>
    public void Activate()
    {
        if (Status == CustomerStatus.Deleted)
            throw new InvalidOperationException("Cannot activate deleted customer");

        Status = CustomerStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Delete customer (soft delete).
    /// </summary>
    public void Delete()
    {
        Status = CustomerStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CustomerDeletedDomainEvent(Id, TenantId));
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public enum CustomerStatus
{
    Active = 1,
    Inactive = 2,
    Deleted = 3
}
