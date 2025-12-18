namespace ModularMonolith.Shared.Abstractions.Tenancy;

/// <summary>
/// Represents tenant information.
/// </summary>
public sealed record TenantInfo
{
    public required Guid TenantId { get; init; }
    public required string Name { get; init; }
    public required string Identifier { get; init; } // Slug/subdomain
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
}
