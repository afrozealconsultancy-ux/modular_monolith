using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Shared.Abstractions.Messaging;

namespace ModularMonolith.Modules.Customers.Application.Queries.GetCustomer;

public sealed record GetCustomerQuery(Guid CustomerId) : IQuery<Result<CustomerDto>>;

public sealed record CustomerDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Email,
    string? PhoneNumber,
    int Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<CustomerAddressDto> Addresses);

public sealed record CustomerAddressDto(
    Guid Id,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    int Type);
