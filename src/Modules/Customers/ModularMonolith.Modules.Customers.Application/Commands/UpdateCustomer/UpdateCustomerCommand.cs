using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Shared.Abstractions.Messaging;

namespace ModularMonolith.Modules.Customers.Application.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string Name,
    string Email,
    string? PhoneNumber) : ICommand<Result>;
