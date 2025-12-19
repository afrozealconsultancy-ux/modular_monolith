using MediatR;
using ModularMonolith.Modules.Storage.Application.Services;
using ModularMonolith.Modules.Storage.Contracts.Events;
using ModularMonolith.Modules.Storage.Domain.Repositories;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.CQRS;
using ModularMonolith.Shared.Abstractions.Events;
using ModularMonolith.Shared.Abstractions.Kernel;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Storage.Application.Commands.DeleteFile;

internal sealed class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result>
{
    private readonly IStoredFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly IEventBus _eventBus;

    public DeleteFileCommandHandler(
        IStoredFileRepository fileRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ITenantContext tenantContext,
        IEventBus eventBus)
    {
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Storage.Delete))
        {
            return Result.Failure(Error.Forbidden(
                "Storage.Delete.Forbidden",
                "You don't have permission to delete files"));
        }

        // Ensure tenant context
        if (!_tenantContext.HasTenant)
        {
            return Result.Failure(Error.Unauthorized(
                "Storage.Delete.NoTenant",
                "Tenant context is required"));
        }

        var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);

        if (file == null)
        {
            return Result.Failure(Error.NotFound(
                "Storage.Delete.NotFound",
                $"File with ID '{request.FileId}' was not found"));
        }

        // Delete from domain (soft delete)
        file.Delete();

        // Delete from SeaweedFS
        await _fileStorageService.DeleteFileAsync(file.SeaweedFsFileId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _eventBus.PublishAsync(new FileDeletedIntegrationEvent
        {
            FileId = file.Id,
            TenantId = file.TenantId,
            FileName = file.FileName
        }, cancellationToken);

        return Result.Success();
    }
}
