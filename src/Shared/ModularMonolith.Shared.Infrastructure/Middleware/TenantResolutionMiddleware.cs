using Microsoft.AspNetCore.Http;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Shared.Infrastructure.Middleware;

/// <summary>
/// Middleware to resolve tenant context from request.
/// Runs early in the pipeline to set up tenant context for the request.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Tenant context is automatically resolved from CurrentUser
        // which reads from JWT claims

        // Additional validation could be added here if needed
        // For example, checking if tenant is active, not suspended, etc.

        await _next(context);
    }
}
