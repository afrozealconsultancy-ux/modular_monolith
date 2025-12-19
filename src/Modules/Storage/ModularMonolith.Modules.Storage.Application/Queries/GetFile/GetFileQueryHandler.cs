using MediatR;
using ModularMonolith.Modules.Storage.Domain.Repositories;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.CQRS;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Storage.Application.Queries.GetFile;

internal sealed class GetFileQueryHandler : IRequestHandler<GetFileQuery, Result<FileDto>>
{
    private readonly IStoredFileRepository _fileRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;

    public GetFileQueryHandler(
        IStoredFileRepository fileRepository,
        ICurrentUser currentUser,
        ITenantContext tenantContext)
    {
        _fileRepository = fileRepository;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    public async Task<Result<FileDto>> Handle(GetFileQuery request, CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Storage.View))
        {
            return Result.Failure<FileDto>(Error.Forbidden(
                "Storage.GetFile.Forbidden",
                "You don't have permission to view files"));
        }

        // Ensure tenant context
        if (!_tenantContext.HasTenant)
        {
            return Result.Failure<FileDto>(Error.Unauthorized(
                "Storage.GetFile.NoTenant",
                "Tenant context is required"));
        }

        var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);

        if (file == null)
        {
            return Result.Failure<FileDto>(Error.NotFound(
                "Storage.GetFile.NotFound",
                $"File with ID '{request.FileId}' was not found"));
        }

        var dto = new FileDto(
            file.Id,
            file.FileName,
            file.FileSize,
            file.MimeType,
            file.ScanStatus.ToString(),
            file.UploadedBy,
            file.UploadedAt,
            file.IsDeleted);

        return Result.Success(dto);
    }
}
