namespace ModularMonolith.Shared.Abstractions.Messaging;

/// <summary>
/// Interface for queries that return data without modifying state.
/// </summary>
/// <typeparam name="TResponse">The type of the query response.</typeparam>
public interface IQuery<out TResponse>
{
}
