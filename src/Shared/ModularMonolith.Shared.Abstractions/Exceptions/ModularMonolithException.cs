namespace ModularMonolith.Shared.Abstractions.Exceptions;

/// <summary>
/// Base exception for the modular monolith application.
/// </summary>
public abstract class ModularMonolithException : Exception
{
    protected ModularMonolithException(string message) : base(message)
    {
    }

    protected ModularMonolithException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : ModularMonolithException
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with key '{key}' was not found.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a validation fails.
/// </summary>
public class ValidationException : ModularMonolithException
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList();
    }

    public IReadOnlyList<string> Errors { get; } = new List<string>();
}

/// <summary>
/// Exception thrown when a conflict occurs (e.g., duplicate resource).
/// </summary>
public class ConflictException : ModularMonolithException
{
    public ConflictException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when an unauthorized access attempt is made.
/// </summary>
public class UnauthorizedException : ModularMonolithException
{
    public UnauthorizedException(string message = "Unauthorized access.")
        : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a forbidden access attempt is made.
/// </summary>
public class ForbiddenException : ModularMonolithException
{
    public ForbiddenException(string message = "Access forbidden.")
        : base(message)
    {
    }
}
