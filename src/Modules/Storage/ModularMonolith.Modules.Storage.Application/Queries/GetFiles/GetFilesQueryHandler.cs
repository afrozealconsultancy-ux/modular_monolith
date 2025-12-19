using MediatR;
using ModularMonolith.Modules.Storage.Application.Queries.GetFile;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.CQRS;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Storage.Application.Queries.GetFiles;

internal sealed class GetFilesQueryHandler : IRequestHandler<GetFilesQuery, Result<PagedResult<FileDto>>>
{
    private readonly IStorageQueryService _queryService;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;

    public GetFilesQueryHandler(
        IStorageQueryService queryService,
        ICurrentUser currentUser,
        ITenantContext tenantContext)
    {
        _queryService = queryService;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PagedResult<FileDto>>> Handle(GetFilesQuery request, CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Storage.View))
        {
            return Result.Failure<PagedResult<FileDto>>(Error.Forbidden(
                "Storage.GetFiles.Forbidden",
                "You don't have permission to view files"));
        }

        // Ensure tenant context
        if (!_tenantContext.HasTenant)
        {
            return Result.Failure<PagedResult<FileDto>>(Error.Unauthorized(
                "Storage.GetFiles.NoTenant",
                "Tenant context is required"));
        }

        var result = await _queryService.GetFilesAsync(
            request.Page,
            request.PageSize,
            request.IncludeDeleted,
            cancellationToken);

        return Result.Success(result);
    }
}
