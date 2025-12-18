using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Modules.Customers.Domain.Entities;

namespace ModularMonolith.Modules.Customers.Application.Queries.GetCustomers;

// This handler needs DbContext for efficient querying
// We'll inject it from Infrastructure layer
internal sealed class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, Result<PagedList<CustomerListItemDto>>>
{
    private readonly ICustomersQueryService _queryService;
    private readonly ICurrentUser _currentUser;

    public GetCustomersQueryHandler(
        ICustomersQueryService queryService,
        ICurrentUser currentUser)
    {
        _queryService = queryService;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedList<CustomerListItemDto>>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Customers.View))
        {
            return Result.Failure<PagedList<CustomerListItemDto>>(Error.Forbidden(
                "Customers.View.Forbidden",
                "You don't have permission to view customers"));
        }

        var result = await _queryService.GetCustomersAsync(
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(result);
    }
}

// Query service interface (implemented in Infrastructure)
public interface ICustomersQueryService
{
    Task<PagedList<CustomerListItemDto>> GetCustomersAsync(
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
