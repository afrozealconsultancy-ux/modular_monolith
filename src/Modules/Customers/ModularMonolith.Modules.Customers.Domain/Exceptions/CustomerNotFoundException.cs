using ModularMonolith.Shared.Abstractions.Exceptions;

namespace ModularMonolith.Modules.Customers.Domain.Exceptions;

public sealed class CustomerNotFoundException : NotFoundException
{
    public CustomerNotFoundException(Guid customerId)
        : base($"Customer with ID '{customerId}' was not found")
    {
    }
}
