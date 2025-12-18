using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Shared.Abstractions.Messaging;

namespace ModularMonolith.Modules.Customers.Application.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string Name,
    string Email,
    string? PhoneNumber,
    List<AddressDto>? Addresses) : ICommand<Result<Guid>>;

public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    int Type);
