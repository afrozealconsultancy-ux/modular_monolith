namespace ModularMonolith.Modules.Customers.Domain.ValueObjects;

/// <summary>
/// Address value object.
/// </summary>
public sealed class Address
{
    private Address(
        Guid id,
        string street,
        string city,
        string state,
        string postalCode,
        string country,
        AddressType type)
    {
        Id = id;
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        Type = type;
    }

    // EF Core constructor
    private Address()
    {
        Street = string.Empty;
        City = string.Empty;
        State = string.Empty;
        PostalCode = string.Empty;
        Country = string.Empty;
    }

    public Guid Id { get; private set; }
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }
    public AddressType Type { get; private set; }

    public static Address Create(
        string street,
        string city,
        string state,
        string postalCode,
        string country,
        AddressType type = AddressType.Other)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        return new Address(Guid.NewGuid(), street, city, state, postalCode, country, type);
    }
}

public enum AddressType
{
    Billing = 1,
    Shipping = 2,
    Other = 3
}
