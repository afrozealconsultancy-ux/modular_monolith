using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Shared.Abstractions.Messaging;

namespace ModularMonolith.Modules.Customers.Application.Commands.DeleteCustomer;

public sealed record DeleteCustomerCommand(Guid CustomerId) : ICommand<Result>;
