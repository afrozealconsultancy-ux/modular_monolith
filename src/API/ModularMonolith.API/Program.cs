using ModularMonolith.Shared.Abstractions.Modules;
using ModularMonolith.Shared.Infrastructure.Extensions;
using ModularMonolith.Shared.Infrastructure.Middleware;
using ModularMonolith.Modules.Customers.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Modular Monolith API",
        Version = "v1",
        Description = "Multi-tenant modular monolith with .NET 8"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add shared infrastructure (CurrentUser, TenantContext, EventBus, etc.)
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Add Keycloak authentication (dual realm: staff + tenant)
builder.Services.AddKeycloakAuthentication(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
    .AddRabbitMQ(builder.Configuration.GetSection("RabbitMQ")["Host"]!);

// Discover and register modules
var modules = DiscoverModules();
foreach (var module in modules)
{
    if (IsModuleEnabled(module, builder.Configuration))
    {
        Log.Information("Registering module: {ModuleName}", module.Name);
        module.RegisterServices(builder.Services, builder.Configuration);
    }
}

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Modular Monolith API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

// Exception handling (must be first)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Tenant resolution
app.UseMiddleware<TenantResolutionMiddleware>();

// CORS (if needed)
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger"));

// Map module endpoints
foreach (var module in modules)
{
    if (IsModuleEnabled(module, app.Configuration))
    {
        Log.Information("Mapping endpoints for module: {ModuleName}", module.Name);
        module.MapEndpoints(app);
    }
}

// Initialize modules (run migrations, seed data, etc.)
foreach (var module in modules)
{
    if (IsModuleEnabled(module, app.Configuration))
    {
        Log.Information("Initializing module: {ModuleName}", module.Name);
        await module.InitializeAsync(app);
    }
}

Log.Information("Modular Monolith API starting...");
Log.Information("Modules loaded: {ModuleCount}", modules.Count(m => IsModuleEnabled(m, app.Configuration)));

app.Run();

// Helper methods
static List<IModule> DiscoverModules()
{
    return new List<IModule>
    {
        new CustomersModule(),
        // Add other modules here as they're built:
        // new IdentityModule(),
        // new StorageModule(),
        // new NotificationsModule(),
        // new OrdersModule(),
        // new CatalogModule(),
    };
}

static bool IsModuleEnabled(IModule module, IConfiguration configuration)
{
    return configuration.GetValue<bool>($"Modules:{module.Name}:Enabled", true);
}
