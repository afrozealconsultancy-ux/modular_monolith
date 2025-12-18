namespace ModularMonolith.Shared.Abstractions.Messaging;

/// <summary>
/// Marker interface for commands that don't return a value.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Interface for commands that return a result.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface ICommand<out TResponse> : ICommand
{
}
