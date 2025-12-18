# Modular Monolith Architecture Plan - Enhanced

## Overview
This document outlines the architecture for a .NET 9 modular monolith with advanced features including authentication, storage, notifications, and comprehensive failure handling, designed for easy migration to microservices.

## Core Principles

1. **Module Independence**: Each module is self-contained with clear boundaries
2. **Separate Schemas**: Each module owns its data in a separate database schema
3. **Async Communication**: Modules communicate via events (RabbitMQ)
4. **CQRS Pattern**: MediatR for command/query separation
5. **Resilience First**: Built-in failure handling, retries, and circuit breakers
6. **Security by Default**: Keycloak authentication, ClamAV scanning
7. **Microservice-Ready**: Clear contracts and no direct dependencies between modules

## Technology Stack

### Core Framework
- **.NET 9.0** (Latest version)
- **C# 13** (Latest language features)
- **Minimal APIs** + **Controllers** (Hybrid approach)

### Messaging & Events
- **MediatR** - In-process messaging and CQRS
- **RabbitMQ + MassTransit** - Async messaging between modules
- **Outbox Pattern** - Reliable event publishing

### Data & Persistence
- **Entity Framework Core 9** - Data access with separate schemas
- **Dapper** - High-performance queries (read models)
- **PostgreSQL** - Primary database (better schema support than SQL Server)
- **Redis** - Caching and distributed locks

### Authentication & Authorization
- **Keycloak** - Identity and Access Management (OAuth2/OIDC)
- **Duende IdentityServer** - Fallback option
- **JWT Bearer Authentication**

### Storage & Security
- **SeaweedFS** - Distributed file storage
- **ClamAV** - Virus scanning
- **MinIO** - Alternative/fallback storage

### Notifications
- **Custom Notification Module** - Multi-channel support
  - Email: SMTP, SendGrid, AWS SES
  - SMS: Twilio, AWS SNS
  - Push: Firebase, OneSignal
  - In-App: SignalR
  - Webhook: Custom HTTP endpoints

### Resilience & Reliability
- **Polly** - Resilience patterns (retry, circuit breaker, bulkhead, timeout)
- **Saga Pattern** - Distributed transactions
- **Outbox Pattern** - At-least-once delivery
- **Health Checks** - Endpoint monitoring

### Observability
- **Serilog** - Structured logging
- **OpenTelemetry** - Distributed tracing and metrics
- **Seq** - Log aggregation
- **Prometheus** - Metrics collection
- **Grafana** - Dashboards

### Validation & Documentation
- **FluentValidation** - Request validation
- **Swagger/OpenAPI** - API documentation
- **Scalar** - Modern API documentation UI

### Testing
- **xUnit** - Unit and integration tests
- **Testcontainers** - Integration testing with Docker
- **FluentAssertions** - Readable assertions
- **Bogus** - Test data generation
- **NetArchTest** - Architecture tests

## Enhanced Solution Structure

```
ModularMonolith/
├── src/
│   ├── API/
│   │   └── ModularMonolith.API/                    # API Gateway/Host
│   │       ├── Controllers/
│   │       ├── Middleware/
│   │       │   ├── ExceptionHandlingMiddleware.cs
│   │       │   ├── CorrelationIdMiddleware.cs
│   │       │   └── RequestLoggingMiddleware.cs
│   │       ├── Extensions/
│   │       ├── Program.cs
│   │       └── appsettings.json
│   │
│   ├── Modules/
│   │   ├── Identity/                               # NEW: Authentication Module
│   │   │   ├── ModularMonolith.Modules.Identity.Domain/
│   │   │   ├── ModularMonolith.Modules.Identity.Application/
│   │   │   ├── ModularMonolith.Modules.Identity.Infrastructure/
│   │   │   │   ├── Keycloak/
│   │   │   │   │   ├── KeycloakService.cs
│   │   │   │   │   ├── KeycloakSettings.cs
│   │   │   │   │   └── KeycloakAuthHandler.cs
│   │   │   │   └── Persistence/
│   │   │   └── ModularMonolith.Modules.Identity.Contracts/
│   │   │
│   │   ├── Storage/                                # NEW: File Storage Module
│   │   │   ├── ModularMonolith.Modules.Storage.Domain/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── StoredFile.cs
│   │   │   │   │   └── ScanResult.cs
│   │   │   │   └── Enums/
│   │   │   │       ├── ScanStatus.cs
│   │   │   │       └── StorageProvider.cs
│   │   │   │
│   │   │   ├── ModularMonolith.Modules.Storage.Application/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── UploadFile/
│   │   │   │   │   ├── DeleteFile/
│   │   │   │   │   └── ScanFile/
│   │   │   │   └── Queries/
│   │   │   │       └── GetFile/
│   │   │   │
│   │   │   ├── ModularMonolith.Modules.Storage.Infrastructure/
│   │   │   │   ├── SeaweedFS/
│   │   │   │   │   ├── SeaweedFSClient.cs
│   │   │   │   │   └── SeaweedFSSettings.cs
│   │   │   │   ├── ClamAV/
│   │   │   │   │   ├── ClamAVScanner.cs
│   │   │   │   │   └── ClamAVSettings.cs
│   │   │   │   ├── Persistence/
│   │   │   │   └── BackgroundServices/
│   │   │   │       └── FileScanningService.cs
│   │   │   │
│   │   │   └── ModularMonolith.Modules.Storage.Contracts/
│   │   │       └── Events/
│   │   │           ├── FileUploadedEvent.cs
│   │   │           ├── FileScannedEvent.cs
│   │   │           └── VirusDetectedEvent.cs
│   │   │
│   │   ├── Notifications/                          # NEW: Advanced Notifications
│   │   │   ├── ModularMonolith.Modules.Notifications.Domain/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Notification.cs
│   │   │   │   │   ├── NotificationTemplate.cs
│   │   │   │   │   ├── NotificationChannel.cs
│   │   │   │   │   └── NotificationLog.cs
│   │   │   │   ├── Enums/
│   │   │   │   │   ├── ChannelType.cs           # Email, SMS, Push, InApp, Webhook
│   │   │   │   │   ├── NotificationStatus.cs
│   │   │   │   │   └── Priority.cs
│   │   │   │   └── ValueObjects/
│   │   │   │       └── NotificationContent.cs
│   │   │   │
│   │   │   ├── ModularMonolith.Modules.Notifications.Application/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── SendNotification/
│   │   │   │   │   ├── CreateTemplate/
│   │   │   │   │   └── ConfigureChannel/
│   │   │   │   ├── Queries/
│   │   │   │   │   └── GetNotificationHistory/
│   │   │   │   └── Abstractions/
│   │   │   │       ├── INotificationChannel.cs
│   │   │   │       └── ITemplateEngine.cs
│   │   │   │
│   │   │   ├── ModularMonolith.Modules.Notifications.Infrastructure/
│   │   │   │   ├── Channels/
│   │   │   │   │   ├── EmailChannel.cs
│   │   │   │   │   ├── SmsChannel.cs
│   │   │   │   │   ├── PushNotificationChannel.cs
│   │   │   │   │   ├── InAppChannel.cs           # SignalR
│   │   │   │   │   └── WebhookChannel.cs
│   │   │   │   ├── Providers/
│   │   │   │   │   ├── Email/
│   │   │   │   │   │   ├── SmtpProvider.cs
│   │   │   │   │   │   ├── SendGridProvider.cs
│   │   │   │   │   │   └── AwsSesProvider.cs
│   │   │   │   │   ├── Sms/
│   │   │   │   │   │   ├── TwilioProvider.cs
│   │   │   │   │   │   └── AwsSnsProvider.cs
│   │   │   │   │   └── Push/
│   │   │   │   │       ├── FirebaseProvider.cs
│   │   │   │   │       └── OneSignalProvider.cs
│   │   │   │   ├── TemplateEngines/
│   │   │   │   │   ├── LiquidTemplateEngine.cs
│   │   │   │   │   └── RazorTemplateEngine.cs
│   │   │   │   ├── BackgroundServices/
│   │   │   │   │   └── NotificationProcessingService.cs
│   │   │   │   └── Persistence/
│   │   │   │
│   │   │   └── ModularMonolith.Modules.Notifications.Contracts/
│   │   │       └── Events/
│   │   │           ├── NotificationSentEvent.cs
│   │   │           └── NotificationFailedEvent.cs
│   │   │
│   │   ├── Customers/
│   │   │   ├── ModularMonolith.Modules.Customers.Domain/
│   │   │   ├── ModularMonolith.Modules.Customers.Application/
│   │   │   ├── ModularMonolith.Modules.Customers.Infrastructure/
│   │   │   └── ModularMonolith.Modules.Customers.Contracts/
│   │   │
│   │   ├── Orders/
│   │   │   ├── ModularMonolith.Modules.Orders.Domain/
│   │   │   ├── ModularMonolith.Modules.Orders.Application/
│   │   │   ├── ModularMonolith.Modules.Orders.Infrastructure/
│   │   │   └── ModularMonolith.Modules.Orders.Contracts/
│   │   │
│   │   └── Catalog/
│   │       ├── ModularMonolith.Modules.Catalog.Domain/
│   │       ├── ModularMonolith.Modules.Catalog.Application/
│   │       ├── ModularMonolith.Modules.Catalog.Infrastructure/
│   │       └── ModularMonolith.Modules.Catalog.Contracts/
│   │
│   └── Shared/
│       ├── ModularMonolith.Shared.Abstractions/
│       │   ├── Domain/
│       │   │   ├── IEntity.cs
│       │   │   ├── IAggregateRoot.cs
│       │   │   └── IDomainEvent.cs
│       │   ├── Messaging/
│       │   │   ├── ICommand.cs
│       │   │   ├── IQuery.cs
│       │   │   └── IIntegrationEvent.cs
│       │   ├── Persistence/
│       │   │   ├── IRepository.cs
│       │   │   └── IUnitOfWork.cs
│       │   ├── Auth/
│       │   │   ├── ICurrentUser.cs
│       │   │   └── IAuthService.cs
│       │   ├── Storage/
│       │   │   └── IStorageService.cs
│       │   └── Notifications/
│       │       └── INotificationService.cs
│       │
│       └── ModularMonolith.Shared.Infrastructure/
│           ├── Auth/
│           │   └── CurrentUserService.cs
│           ├── EventBus/
│           │   ├── RabbitMQ/
│           │   ├── InMemory/
│           │   └── Outbox/                         # Outbox pattern
│           │       ├── OutboxMessage.cs
│           │       ├── OutboxProcessor.cs
│           │       └── OutboxDbContext.cs
│           ├── Messaging/
│           │   └── MediatR/
│           ├── Persistence/
│           │   ├── EFCore/
│           │   │   ├── BaseDbContext.cs
│           │   │   └── UnitOfWork.cs
│           │   └── Interceptors/
│           │       ├── AuditInterceptor.cs
│           │       └── SoftDeleteInterceptor.cs
│           ├── Resilience/                         # NEW: Resilience patterns
│           │   ├── Policies/
│           │   │   ├── RetryPolicy.cs
│           │   │   ├── CircuitBreakerPolicy.cs
│           │   │   ├── BulkheadPolicy.cs
│           │   │   └── TimeoutPolicy.cs
│           │   └── Saga/
│           │       ├── ISaga.cs
│           │       ├── SagaOrchestrator.cs
│           │       └── SagaState.cs
│           ├── Behaviors/
│           │   ├── LoggingBehavior.cs
│           │   ├── ValidationBehavior.cs
│           │   ├── TransactionBehavior.cs
│           │   ├── CachingBehavior.cs
│           │   └── ResilienceBehavior.cs            # NEW
│           ├── Caching/
│           │   ├── ICacheService.cs
│           │   └── RedisCacheService.cs
│           └── Observability/
│               ├── Telemetry.cs
│               └── HealthChecks/
│
└── tests/
    ├── ModularMonolith.Modules.Identity.Tests/
    ├── ModularMonolith.Modules.Storage.Tests/
    ├── ModularMonolith.Modules.Notifications.Tests/
    ├── ModularMonolith.Modules.Customers.Tests/
    ├── ModularMonolith.Modules.Orders.Tests/
    ├── ModularMonolith.ArchitectureTests/
    └── ModularMonolith.IntegrationTests/
```

## Module Design Pattern

Each module follows **Clean Architecture** with four layers:

### 1. Domain Layer
- Core business logic, entities, value objects
- **No external dependencies**
- Contains: Entities, Value Objects, Domain Events, Exceptions, Specifications

### 2. Application Layer
- Use cases, orchestration, DTOs
- **Dependencies**: Domain Layer, Shared.Abstractions
- Contains: Commands, Queries, Handlers, DTOs, Validators, Integration Events

### 3. Infrastructure Layer
- Technical implementations
- **Dependencies**: Application, Domain, Shared.Infrastructure
- Contains: DbContext, Repositories, External Services, Event Bus

### 4. Contracts Layer
- Public API for other modules
- **No dependencies** (pure DTOs)
- Contains: Integration Events, Public DTOs, Interfaces

## Database Strategy

### PostgreSQL with Separate Schemas

```csharp
// Each module has its own schema
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema("customers");
    // OR "orders", "catalog", "identity", "storage", "notifications"
}
```

### Schema Organization
```
PostgreSQL Database: modular_monolith
├── Schema: identity         (users, roles, permissions)
├── Schema: storage          (files, scan_results)
├── Schema: notifications    (notifications, templates, channels, logs)
├── Schema: customers        (customers, addresses)
├── Schema: orders           (orders, order_items)
├── Schema: catalog          (products, categories, inventory)
└── Schema: outbox           (outbox_messages, inbox_messages)
```

### Migration Path
1. **Phase 1 (Monolith)**: Single database, separate schemas
2. **Phase 2 (Distributed Monolith)**: Separate databases, same infrastructure
3. **Phase 3 (Microservices)**: Independent services, separate deployments

## Authentication with Keycloak

### Architecture
```
User Request → API Gateway → JWT Validation → Keycloak Verification → Module
                    ↓
              [Middleware]
                    ↓
         Extract User Context
                    ↓
         Store in ICurrentUser
```

### Keycloak Integration

```csharp
public class KeycloakSettings
{
    public string Authority { get; set; }          // https://keycloak.domain.com/realms/modular-monolith
    public string Audience { get; set; }           // modular-monolith-api
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public bool RequireHttpsMetadata { get; set; }
    public int TokenExpirationMinutes { get; set; }
}

// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakSettings.Authority;
        options.Audience = keycloakSettings.Audience;
        options.RequireHttpsMetadata = keycloakSettings.RequireHttpsMetadata;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Log and handle authentication failures
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireCustomerRole", policy =>
        policy.RequireRole("customer"));
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("admin"));
});
```

### Current User Service

```csharp
public interface ICurrentUser
{
    Guid UserId { get; }
    string Email { get; }
    string[] Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Guid UserId =>
        Guid.Parse(_httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

    public string Email =>
        _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public string[] Roles =>
        _httpContextAccessor.HttpContext?.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray() ?? Array.Empty<string>();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) =>
        Roles.Contains(role);
}
```

## Storage Module with SeaweedFS & ClamAV

### File Upload Flow

```
Upload Request → API → Command Handler → SeaweedFS Upload → Queue Scan → ClamAV Scan
                                              ↓                              ↓
                                        Store Metadata               Update Scan Status
                                              ↓                              ↓
                                      Publish Event                  Publish Event
```

### SeaweedFS Client

```csharp
public interface IStorageService
{
    Task<string> UploadFileAsync(Stream file, string fileName, string contentType,
        CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string fileId,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string fileId,
        CancellationToken cancellationToken = default);
    Task<FileMetadata> GetMetadataAsync(string fileId,
        CancellationToken cancellationToken = default);
}

public class SeaweedFSClient : IStorageService
{
    private readonly HttpClient _httpClient;
    private readonly SeaweedFSSettings _settings;
    private readonly ILogger<SeaweedFSClient> _logger;

    public async Task<string> UploadFileAsync(
        Stream file,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // 1. Get file ID from master
        var assignResponse = await _httpClient.GetFromJsonAsync<AssignResponse>(
            $"{_settings.MasterUrl}/dir/assign",
            cancellationToken);

        // 2. Upload to volume server
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(file), "file", fileName);

        var uploadUrl = $"http://{assignResponse.Url}/{assignResponse.Fid}";
        var response = await _httpClient.PostAsync(uploadUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return assignResponse.Fid;
    }

    public async Task<Stream> DownloadFileAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        // 1. Lookup file location
        var lookupResponse = await _httpClient.GetFromJsonAsync<LookupResponse>(
            $"{_settings.MasterUrl}/dir/lookup?volumeId={GetVolumeId(fileId)}",
            cancellationToken);

        // 2. Download from volume server
        var downloadUrl = $"http://{lookupResponse.Locations[0].Url}/{fileId}";
        var response = await _httpClient.GetAsync(downloadUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
```

### ClamAV Scanner

```csharp
public interface IVirusScanner
{
    Task<ScanResult> ScanFileAsync(Stream file,
        CancellationToken cancellationToken = default);
    Task<bool> PingAsync(CancellationToken cancellationToken = default);
}

public class ClamAVScanner : IVirusScanner
{
    private readonly ClamAVSettings _settings;
    private readonly ILogger<ClamAVScanner> _logger;

    public async Task<ScanResult> ScanFileAsync(
        Stream file,
        CancellationToken cancellationToken = default)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, cancellationToken);

        using var stream = client.GetStream();

        // Send INSTREAM command
        await stream.WriteAsync(Encoding.UTF8.GetBytes("zINSTREAM\0"), cancellationToken);

        // Send file in chunks
        var buffer = new byte[8192];
        int bytesRead;
        while ((bytesRead = await file.ReadAsync(buffer, cancellationToken)) > 0)
        {
            var chunkSize = BitConverter.GetBytes(bytesRead);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(chunkSize);

            await stream.WriteAsync(chunkSize, 0, 4, cancellationToken);
            await stream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
        }

        // Send zero-length chunk to indicate end
        await stream.WriteAsync(new byte[4], cancellationToken);

        // Read response
        var response = new byte[1024];
        var responseLength = await stream.ReadAsync(response, cancellationToken);
        var result = Encoding.UTF8.GetString(response, 0, responseLength);

        return new ScanResult
        {
            IsClean = result.Contains("OK"),
            ThreatName = result.Contains("FOUND") ? ExtractThreatName(result) : null,
            ScannedAt = DateTime.UtcNow
        };
    }
}
```

### Background File Scanning

```csharp
public class FileScanningService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FileScanningService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
                var scanner = scope.ServiceProvider.GetRequiredService<IVirusScanner>();
                var storage = scope.ServiceProvider.GetRequiredService<IStorageService>();

                // Get pending files
                var pendingFiles = await dbContext.StoredFiles
                    .Where(f => f.ScanStatus == ScanStatus.Pending)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var file in pendingFiles)
                {
                    // Download file
                    var fileStream = await storage.DownloadFileAsync(file.FileId, stoppingToken);

                    // Scan
                    var scanResult = await scanner.ScanFileAsync(fileStream, stoppingToken);

                    // Update status
                    file.ScanStatus = scanResult.IsClean ? ScanStatus.Clean : ScanStatus.Infected;
                    file.ThreatName = scanResult.ThreatName;
                    file.ScannedAt = DateTime.UtcNow;

                    await dbContext.SaveChangesAsync(stoppingToken);

                    // Publish event
                    if (!scanResult.IsClean)
                    {
                        // Publish VirusDetectedEvent
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in file scanning service");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
```

## Advanced Notification Module

### Channel Architecture

```
Notification Request → Template Engine → Channel Selector → Provider → Delivery
         ↓                   ↓                  ↓              ↓           ↓
    Validation          Render Content    Route to Channel  Send      Log Result
```

### Notification Entity

```csharp
public class Notification : AggregateRoot
{
    public Guid RecipientId { get; private set; }
    public NotificationType Type { get; private set; }
    public Priority Priority { get; private set; }
    public NotificationContent Content { get; private set; }
    public List<ChannelType> Channels { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime? ScheduledFor { get; private set; }
    public DateTime? SentAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        AddDomainEvent(new NotificationSentDomainEvent(Id, RecipientId));
    }

    public void MarkAsFailed(string error)
    {
        RetryCount++;
        ErrorMessage = error;
        Status = RetryCount >= 3
            ? NotificationStatus.Failed
            : NotificationStatus.Pending;
        AddDomainEvent(new NotificationFailedDomainEvent(Id, error));
    }
}
```

### Channel Interface

```csharp
public interface INotificationChannel
{
    ChannelType ChannelType { get; }
    Task<bool> CanHandleAsync(Notification notification);
    Task<SendResult> SendAsync(Notification notification,
        CancellationToken cancellationToken = default);
}

// Email Channel
public class EmailChannel : INotificationChannel
{
    private readonly IEmailProviderFactory _providerFactory;
    private readonly ILogger<EmailChannel> _logger;

    public ChannelType ChannelType => ChannelType.Email;

    public async Task<SendResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        var provider = await _providerFactory.GetProviderAsync();

        return await provider.SendEmailAsync(new EmailMessage
        {
            To = notification.Content.Recipient,
            Subject = notification.Content.Subject,
            Body = notification.Content.Body,
            IsHtml = true
        }, cancellationToken);
    }
}

// SMS Channel
public class SmsChannel : INotificationChannel
{
    private readonly ISmsProviderFactory _providerFactory;

    public ChannelType ChannelType => ChannelType.Sms;

    public async Task<SendResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        var provider = await _providerFactory.GetProviderAsync();

        return await provider.SendSmsAsync(new SmsMessage
        {
            PhoneNumber = notification.Content.Recipient,
            Body = notification.Content.Body
        }, cancellationToken);
    }
}

// In-App Channel (SignalR)
public class InAppChannel : INotificationChannel
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public ChannelType ChannelType => ChannelType.InApp;

    public async Task<SendResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .User(notification.RecipientId.ToString())
            .SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Content.Subject,
                notification.Content.Body,
                notification.Priority
            }, cancellationToken);

        return SendResult.Success();
    }
}

// Push Notification Channel
public class PushNotificationChannel : INotificationChannel
{
    private readonly IPushProviderFactory _providerFactory;

    public ChannelType ChannelType => ChannelType.Push;

    public async Task<SendResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        var provider = await _providerFactory.GetProviderAsync();

        return await provider.SendPushAsync(new PushMessage
        {
            DeviceToken = notification.Content.Recipient,
            Title = notification.Content.Subject,
            Body = notification.Content.Body,
            Data = notification.Content.Data
        }, cancellationToken);
    }
}

// Webhook Channel
public class WebhookChannel : INotificationChannel
{
    private readonly HttpClient _httpClient;

    public ChannelType ChannelType => ChannelType.Webhook;

    public async Task<SendResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            notification.Content.Recipient, // Webhook URL
            new
            {
                notification.Id,
                notification.Type,
                notification.Content.Subject,
                notification.Content.Body,
                notification.Content.Data
            },
            cancellationToken);

        return response.IsSuccessStatusCode
            ? SendResult.Success()
            : SendResult.Failure($"HTTP {response.StatusCode}");
    }
}
```

### Provider Factory Pattern

```csharp
public interface IEmailProviderFactory
{
    Task<IEmailProvider> GetProviderAsync();
}

public class EmailProviderFactory : IEmailProviderFactory
{
    private readonly IOptionsMonitor<NotificationSettings> _settings;
    private readonly IServiceProvider _serviceProvider;

    public async Task<IEmailProvider> GetProviderAsync()
    {
        var providerType = _settings.CurrentValue.Email.Provider;

        return providerType switch
        {
            "SendGrid" => _serviceProvider.GetRequiredService<SendGridProvider>(),
            "SMTP" => _serviceProvider.GetRequiredService<SmtpProvider>(),
            "AwsSes" => _serviceProvider.GetRequiredService<AwsSesProvider>(),
            _ => throw new InvalidOperationException($"Unknown email provider: {providerType}")
        };
    }
}

// SendGrid Implementation
public class SendGridProvider : IEmailProvider
{
    private readonly SendGridClient _client;
    private readonly ILogger<SendGridProvider> _logger;

    public async Task<SendResult> SendEmailAsync(EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_settings.FromEmail, _settings.FromName),
            Subject = message.Subject,
            HtmlContent = message.Body
        };
        msg.AddTo(message.To);

        var response = await _client.SendEmailAsync(msg, cancellationToken);

        return response.IsSuccessStatusCode
            ? SendResult.Success()
            : SendResult.Failure($"SendGrid error: {response.StatusCode}");
    }
}
```

### Configuration

```json
{
  "Notifications": {
    "Email": {
      "Provider": "SendGrid",
      "SendGrid": {
        "ApiKey": "SG.xxxxx",
        "FromEmail": "noreply@example.com",
        "FromName": "Modular Monolith"
      },
      "Smtp": {
        "Host": "smtp.gmail.com",
        "Port": 587,
        "Username": "user@gmail.com",
        "Password": "password",
        "EnableSsl": true
      },
      "AwsSes": {
        "Region": "us-east-1",
        "AccessKey": "xxxxx",
        "SecretKey": "xxxxx"
      }
    },
    "Sms": {
      "Provider": "Twilio",
      "Twilio": {
        "AccountSid": "ACxxxxx",
        "AuthToken": "xxxxx",
        "FromNumber": "+1234567890"
      },
      "AwsSns": {
        "Region": "us-east-1",
        "AccessKey": "xxxxx",
        "SecretKey": "xxxxx"
      }
    },
    "Push": {
      "Provider": "Firebase",
      "Firebase": {
        "ServerKey": "xxxxx",
        "SenderId": "xxxxx"
      },
      "OneSignal": {
        "AppId": "xxxxx",
        "ApiKey": "xxxxx"
      }
    },
    "InApp": {
      "Enabled": true,
      "SignalR": {
        "HubPath": "/notifications"
      }
    },
    "Webhook": {
      "Enabled": true,
      "TimeoutSeconds": 30,
      "RetryCount": 3
    }
  }
}
```

## Resilience & Failure Handling

### Resilience Strategy

```
┌─────────────────────────────────────────────────────────────┐
│                    Resilience Layers                         │
├─────────────────────────────────────────────────────────────┤
│  1. Timeout         → Prevent hanging operations             │
│  2. Retry           → Handle transient failures              │
│  3. Circuit Breaker → Prevent cascading failures             │
│  4. Bulkhead        → Isolate failures                       │
│  5. Fallback        → Graceful degradation                   │
│  6. Outbox Pattern  → Guaranteed event delivery              │
│  7. Saga Pattern    → Distributed transaction handling       │
└─────────────────────────────────────────────────────────────┘
```

### Polly Policies

```csharp
public static class ResiliencePolicies
{
    // 1. Timeout Policy
    public static IAsyncPolicy<HttpResponseMessage> TimeoutPolicy =>
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));

    // 2. Retry Policy with Exponential Backoff
    public static IAsyncPolicy<HttpResponseMessage> RetryPolicy =>
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .OrResult(r => !r.IsSuccessStatusCode && (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Log.Warning(
                        "Retry {RetryCount} after {Delay}ms due to {Exception}",
                        retryCount, timespan.TotalMilliseconds, outcome.Exception?.Message);
                });

    // 3. Circuit Breaker Policy
    public static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy =>
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    Log.Warning("Circuit breaker opened for {Duration}s", duration.TotalSeconds);
                },
                onReset: () =>
                {
                    Log.Information("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    Log.Information("Circuit breaker half-open");
                });

    // 4. Bulkhead Policy (Limit concurrent executions)
    public static IAsyncPolicy<HttpResponseMessage> BulkheadPolicy =>
        Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: 10,
            maxQueuingActions: 20,
            onBulkheadRejectedAsync: context =>
            {
                Log.Warning("Bulkhead rejected execution");
                return Task.CompletedTask;
            });

    // 5. Combined Policy
    public static IAsyncPolicy<HttpResponseMessage> CombinedPolicy =>
        Policy.WrapAsync(
            BulkheadPolicy,
            CircuitBreakerPolicy,
            RetryPolicy,
            TimeoutPolicy);
}

// Usage in HttpClient
builder.Services.AddHttpClient<IExternalService, ExternalService>()
    .AddPolicyHandler(ResiliencePolicies.CombinedPolicy);
```

### MediatR Resilience Behavior

```csharp
public class ResilienceBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ResilienceBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var policy = Policy
            .Handle<Exception>(ex => !(ex is ValidationException))
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(100 * retryAttempt),
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Retry {RetryCount} for {RequestName}",
                        retryCount,
                        typeof(TRequest).Name);
                });

        return await policy.ExecuteAsync(() => next());
    }
}
```

### Outbox Pattern Implementation

```csharp
// Outbox Message Entity
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Content { get; set; }
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}

// Outbox Interceptor
public class OutboxInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context as DbContext;
        if (dbContext == null) return result;

        // Get domain events from aggregate roots
        var domainEvents = dbContext.ChangeTracker
            .Entries<IAggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // Convert to outbox messages
        var outboxMessages = domainEvents
            .Select(e => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = e.GetType().AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(e, e.GetType()),
                OccurredOn = DateTime.UtcNow
            })
            .ToList();

        // Save to outbox table
        dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
        await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }
}

// Outbox Processor (Background Service)
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OutboxProcessor> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

                // Get unprocessed messages
                var messages = await dbContext.Set<OutboxMessage>()
                    .Where(m => m.ProcessedOn == null && m.RetryCount < 3)
                    .OrderBy(m => m.OccurredOn)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        // Deserialize event
                        var eventType = Type.GetType(message.Type)!;
                        var @event = JsonSerializer.Deserialize(message.Content, eventType);

                        // Publish to message bus
                        await _publishEndpoint.Publish(@event!, stoppingToken);

                        // Mark as processed
                        message.ProcessedOn = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        message.RetryCount++;
                        message.Error = ex.Message;
                        _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
```

### Saga Pattern for Distributed Transactions

```csharp
public interface ISaga
{
    Guid CorrelationId { get; }
    SagaState State { get; }
    Task ExecuteAsync(CancellationToken cancellationToken = default);
    Task CompensateAsync(CancellationToken cancellationToken = default);
}

// Example: Order Creation Saga
public class CreateOrderSaga : ISaga
{
    private readonly ISender _mediator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateOrderSaga> _logger;

    public Guid CorrelationId { get; }
    public SagaState State { get; private set; }

    private Guid? _customerId;
    private Guid? _orderId;
    private List<Guid> _reservedProducts = new();

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            State = SagaState.Running;

            // Step 1: Validate customer
            _logger.LogInformation("Saga {CorrelationId}: Validating customer", CorrelationId);
            var customerValid = await _mediator.Send(
                new ValidateCustomerQuery(_customerId!.Value),
                cancellationToken);

            if (!customerValid)
                throw new SagaException("Customer validation failed");

            // Step 2: Reserve products
            _logger.LogInformation("Saga {CorrelationId}: Reserving products", CorrelationId);
            foreach (var productId in _reservedProducts)
            {
                await _publishEndpoint.Publish(
                    new ReserveProductCommand(productId),
                    cancellationToken);
            }

            // Step 3: Create order
            _logger.LogInformation("Saga {CorrelationId}: Creating order", CorrelationId);
            _orderId = await _mediator.Send(
                new CreateOrderCommand(_customerId.Value, _reservedProducts),
                cancellationToken);

            // Step 4: Process payment
            _logger.LogInformation("Saga {CorrelationId}: Processing payment", CorrelationId);
            await _publishEndpoint.Publish(
                new ProcessPaymentCommand(_orderId.Value),
                cancellationToken);

            State = SagaState.Completed;
            _logger.LogInformation("Saga {CorrelationId}: Completed successfully", CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga {CorrelationId}: Failed, starting compensation", CorrelationId);
            State = SagaState.Compensating;
            await CompensateAsync(cancellationToken);
        }
    }

    public async Task CompensateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Compensate in reverse order

            // Cancel payment
            if (_orderId.HasValue)
            {
                _logger.LogInformation("Saga {CorrelationId}: Canceling payment", CorrelationId);
                await _publishEndpoint.Publish(
                    new CancelPaymentCommand(_orderId.Value),
                    cancellationToken);
            }

            // Delete order
            if (_orderId.HasValue)
            {
                _logger.LogInformation("Saga {CorrelationId}: Deleting order", CorrelationId);
                await _mediator.Send(
                    new DeleteOrderCommand(_orderId.Value),
                    cancellationToken);
            }

            // Release product reservations
            foreach (var productId in _reservedProducts)
            {
                _logger.LogInformation("Saga {CorrelationId}: Releasing product {ProductId}",
                    CorrelationId, productId);
                await _publishEndpoint.Publish(
                    new ReleaseProductCommand(productId),
                    cancellationToken);
            }

            State = SagaState.Compensated;
            _logger.LogInformation("Saga {CorrelationId}: Compensation completed", CorrelationId);
        }
        catch (Exception ex)
        {
            State = SagaState.Failed;
            _logger.LogError(ex, "Saga {CorrelationId}: Compensation failed", CorrelationId);
            throw;
        }
    }
}

public enum SagaState
{
    Pending,
    Running,
    Completed,
    Compensating,
    Compensated,
    Failed
}
```

### Module Failure Handling

When a module fails, the system should:

1. **Isolate the Failure**: Circuit breaker prevents cascading failures
2. **Retry**: Transient failures are retried with exponential backoff
3. **Fallback**: Provide degraded functionality
4. **Compensate**: Use saga pattern for distributed transactions
5. **Alert**: Log and monitor failures
6. **Queue**: Store events in outbox for retry

```csharp
// Example: Module failure handling
public class OrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly ICircuitBreakerPolicy _circuitBreaker;
    private readonly IFallbackService _fallback;

    public async Task<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Try primary path with circuit breaker
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                // Normal order creation
                return await CreateOrderAsync(request, cancellationToken);
            });
        }
        catch (BrokenCircuitException)
        {
            // Circuit is open, use fallback
            _logger.LogWarning("Circuit breaker open, using fallback");
            return await _fallback.CreateOrderAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log and handle unexpected errors
            _logger.LogError(ex, "Failed to create order");
            throw;
        }
    }
}
```

## Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(configuration.GetConnectionString("Database")!, name: "database")
    .AddRabbitMQ(configuration["RabbitMQ:Host"]!, name: "rabbitmq")
    .AddRedis(configuration["Redis:ConnectionString"]!, name: "redis")
    .AddUrlGroup(new Uri(configuration["Keycloak:Authority"]!), name: "keycloak")
    .AddSeaweedFS(configuration["SeaweedFS:MasterUrl"]!, name: "seaweedfs")
    .AddClamAV(configuration["ClamAV:Host"]!, configuration.GetValue<int>("ClamAV:Port"), name: "clamav")
    .AddCheck<CustomersModuleHealthCheck>("customers-module")
    .AddCheck<OrdersModuleHealthCheck>("orders-module")
    .AddCheck<NotificationsModuleHealthCheck>("notifications-module");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

## Configuration Example

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Database=modular_monolith;Username=postgres;Password=postgres"
  },
  "Keycloak": {
    "Authority": "https://keycloak.domain.com/realms/modular-monolith",
    "Audience": "modular-monolith-api",
    "ClientId": "modular-monolith-client",
    "ClientSecret": "secret",
    "RequireHttpsMetadata": true,
    "TokenExpirationMinutes": 60
  },
  "RabbitMQ": {
    "Host": "localhost",
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "SeaweedFS": {
    "MasterUrl": "http://localhost:9333",
    "Filer": "http://localhost:8888"
  },
  "ClamAV": {
    "Host": "localhost",
    "Port": 3310,
    "Enabled": true
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Seq",
        "Args": { "serverUrl": "http://localhost:5341" }
      }
    ]
  },
  "Modules": {
    "Identity": { "Enabled": true },
    "Storage": { "Enabled": true },
    "Notifications": { "Enabled": true },
    "Customers": { "Enabled": true },
    "Orders": { "Enabled": true },
    "Catalog": { "Enabled": true }
  }
}
```

## Development Workflow (Updated)

### Phase 1: Foundation (Week 1-2)
1. Create solution structure
2. Setup shared abstractions and infrastructure
3. Configure PostgreSQL, RabbitMQ, Redis
4. Setup Keycloak
5. Implement resilience patterns (Polly, Outbox, Saga)
6. Setup API host with middleware

### Phase 2: Core Modules (Week 2-4)
1. **Identity Module** - Keycloak integration
2. **Storage Module** - SeaweedFS + ClamAV
3. **Notifications Module** - Multi-channel support

### Phase 3: Business Modules (Week 4-6)
1. Customers module
2. Orders module
3. Catalog module
4. Cross-module integration

### Phase 4: Testing & Hardening (Week 6-8)
1. Unit tests for all modules
2. Integration tests with Testcontainers
3. Architecture tests
4. Chaos engineering tests (failure scenarios)
5. Performance testing
6. Security testing

## Next Steps

1. ✅ Review and approve enhanced architecture plan
2. ⬜ Setup development environment (PostgreSQL, RabbitMQ, Redis, Keycloak, SeaweedFS, ClamAV)
3. ⬜ Create solution structure with .NET 9
4. ⬜ Implement shared infrastructure with resilience patterns
5. ⬜ Build Identity module with Keycloak
6. ⬜ Build Storage module with SeaweedFS & ClamAV
7. ⬜ Build Notifications module with multi-channel support
8. ⬜ Build business modules (Customers, Orders, Catalog)
9. ⬜ Implement comprehensive testing
10. ⬜ Add monitoring and observability
