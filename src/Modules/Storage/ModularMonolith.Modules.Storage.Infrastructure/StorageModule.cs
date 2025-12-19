using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Modules.Storage.Application.Commands.DeleteFile;
using ModularMonolith.Modules.Storage.Application.Commands.UploadFile;
using ModularMonolith.Modules.Storage.Application.Queries;
using ModularMonolith.Modules.Storage.Application.Queries.GetFile;
using ModularMonolith.Modules.Storage.Application.Queries.GetFiles;
using ModularMonolith.Modules.Storage.Application.Services;
using ModularMonolith.Modules.Storage.Domain.Repositories;
using ModularMonolith.Modules.Storage.Infrastructure.Persistence;
using ModularMonolith.Modules.Storage.Infrastructure.Persistence.Queries;
using ModularMonolith.Modules.Storage.Infrastructure.Persistence.Repositories;
using ModularMonolith.Modules.Storage.Infrastructure.Services;
using ModularMonolith.Shared.Abstractions.Kernel;
using ModularMonolith.Shared.Abstractions.Modules;
using ModularMonolith.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Modules.Storage.Infrastructure;

public sealed class StorageModule : IModule
{
    public string Name => "Storage";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddModuleDbContext<StorageDbContext>(configuration, "storage");
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<StorageDbContext>());

        // Repositories
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();

        // Query Services
        services.AddScoped<IStorageQueryService, StorageQueryService>();

        // External Services (stubs for now)
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IVirusScanService, VirusScanService>();

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UploadFileCommand).Assembly));
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/storage")
            .RequireAuthorization()
            .WithTags("Storage");

        group.MapPost("/upload", UploadFile)
            .DisableAntiforgery(); // For file uploads

        group.MapGet("/{id:guid}", GetFile);
        group.MapGet("/", GetFiles);
        group.MapDelete("/{id:guid}", DeleteFile);
    }

    public async Task InitializeAsync(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    private static async Task<IResult> UploadFile(
        HttpRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType || request.Form.Files.Count == 0)
        {
            return Results.BadRequest("No file uploaded");
        }

        var file = request.Form.Files[0];

        using var stream = file.OpenReadStream();

        var metadata = new Dictionary<string, string>();
        foreach (var (key, value) in request.Form)
        {
            if (key != "file")
            {
                metadata[key] = value.ToString();
            }
        }

        var command = new UploadFileCommand(
            file.FileName,
            file.Length,
            file.ContentType,
            stream,
            metadata);

        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new { fileId = result.Value })
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> GetFile(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetFileQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> GetFiles(
        IMediator mediator,
        int page = 1,
        int pageSize = 10,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFilesQuery(page, pageSize, includeDeleted);
        var result = await mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> DeleteFile(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFileCommand(id);
        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.Error);
    }
}
