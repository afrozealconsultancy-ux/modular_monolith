using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Shared.Abstractions.Messaging;

namespace ModularMonolith.Modules.Customers.Application.Queries.GetCustomers;

public sealed record GetCustomersQuery(
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IQuery<Result<PagedList<CustomerListItemDto>>>;

public sealed record CustomerListItemDto(
    Guid Id,
    string Name,
    string Email,
    string? PhoneNumber,
    int Status,
    DateTime CreatedAt);
