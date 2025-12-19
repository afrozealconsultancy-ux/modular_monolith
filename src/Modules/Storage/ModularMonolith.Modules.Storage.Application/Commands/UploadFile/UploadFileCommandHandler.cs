using MediatR;
using ModularMonolith.Modules.Storage.Application.Services;
using ModularMonolith.Modules.Storage.Contracts.Events;
using ModularMonolith.Modules.Storage.Domain.Entities;
using ModularMonolith.Modules.Storage.Domain.Repositories;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.CQRS;
using ModularMonolith.Shared.Abstractions.Events;
using ModularMonolith.Shared.Abstractions.Kernel;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Storage.Application.Commands.UploadFile;

internal sealed class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<Guid>>
{
    private readonly IStoredFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IVirusScanService _virusScanService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly IEventBus _eventBus;

    public UploadFileCommandHandler(
        IStoredFileRepository fileRepository,
        IFileStorageService fileStorageService,
        IVirusScanService virusScanService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ITenantContext tenantContext,
        IEventBus eventBus)
    {
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _virusScanService = virusScanService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _eventBus = eventBus;
    }

    public async Task<Result<Guid>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Storage.Upload))
        {
            return Result.Failure<Guid>(Error.Forbidden(
                "Storage.Upload.Forbidden",
                "You don't have permission to upload files"));
        }

        // Ensure tenant context
        if (!_tenantContext.HasTenant)
        {
            return Result.Failure<Guid>(Error.Unauthorized(
                "Storage.Upload.NoTenant",
                "Tenant context is required"));
        }

        // Scan file for viruses
        var (scanStatus, scanResult) = await _virusScanService.ScanFileAsync(request.FileStream, cancellationToken);

        if (scanStatus == Domain.Enums.FileScanStatus.Infected)
        {
            return Result.Failure<Guid>(Error.Validation(
                "Storage.Upload.InfectedFile",
                $"File is infected: {scanResult}"));
        }

        // Reset stream position after scanning
        request.FileStream.Position = 0;

        // Upload to SeaweedFS
        var seaweedFsFileId = await _fileStorageService.UploadFileAsync(
            request.FileStream,
            request.FileName,
            request.MimeType,
            cancellationToken);

        // Create StoredFile entity
        var file = StoredFile.Create(
            _tenantContext.TenantId!.Value,
            request.FileName,
            request.FileSize,
            request.MimeType,
            seaweedFsFileId,
            _currentUser.UserId!.Value);

        // Add metadata if provided
        if (request.Metadata != null)
        {
            foreach (var (key, value) in request.Metadata)
            {
                file.AddMetadata(key, value);
            }
        }

        // Mark scan as completed
        file.CompleteScan(scanStatus, scanResult);

        await _fileRepository.AddAsync(file, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _eventBus.PublishAsync(new FileUploadedIntegrationEvent
        {
            FileId = file.Id,
            TenantId = file.TenantId,
            FileName = file.FileName,
            FileSize = file.FileSize,
            MimeType = file.MimeType,
            UploadedBy = file.UploadedBy,
            UploadedAt = file.UploadedAt
        }, cancellationToken);

        return Result.Success(file.Id);
    }
}

public static class Permissions
{
    public static class Storage
    {
        public const string View = "storage.view";
        public const string Upload = "storage.upload";
        public const string Download = "storage.download";
        public const string Delete = "storage.delete";
    }
}
