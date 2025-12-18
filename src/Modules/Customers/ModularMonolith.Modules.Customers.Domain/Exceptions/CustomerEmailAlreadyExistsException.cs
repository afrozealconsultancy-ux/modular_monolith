using ModularMonolith.Shared.Abstractions.Exceptions;

namespace ModularMonolith.Modules.Customers.Domain.Exceptions;

public sealed class CustomerEmailAlreadyExistsException : ConflictException
{
    public CustomerEmailAlreadyExistsException(string email)
        : base($"A customer with email '{email}' already exists")
    {
    }
}
