namespace ModularMonolith.Shared.Abstractions.Domain;

/// <summary>
/// Base class for entities with Guid identifiers.
/// </summary>
public abstract class Entity : Entity<Guid>
{
    protected Entity(Guid id) : base(id)
    {
    }

    protected Entity() : base(Guid.NewGuid())
    {
    }
}

/// <summary>
/// Base class for entities with custom identifier types.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class Entity<TId> : IEntity<TId>, IEquatable<Entity<TId>> where TId : notnull
{
    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; protected init; }

    Guid IEntity.Id => Id is Guid guid ? guid : Guid.Parse(Id.ToString()!);

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Equals(entity);
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
