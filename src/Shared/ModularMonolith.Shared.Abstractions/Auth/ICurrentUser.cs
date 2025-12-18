namespace ModularMonolith.Shared.Abstractions.Auth;

/// <summary>
/// Interface for accessing the current authenticated user.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the username of the current user.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the email of the current user.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the roles assigned to the current user.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks if the current user has a specific role.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user has the role; otherwise, false.</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Gets a claim value from the current user's claims.
    /// </summary>
    /// <param name="claimType">The type of the claim.</param>
    /// <returns>The claim value if found; otherwise, null.</returns>
    string? GetClaim(string claimType);
}
