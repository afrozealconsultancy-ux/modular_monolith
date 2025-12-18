# Implementation Roadmap

## Overview
This document provides a step-by-step implementation guide for the Modular Monolith architecture.

## Prerequisites

### Required Tools
- .NET 8 SDK
- Visual Studio 2022 / VS Code / Rider
- SQL Server (LocalDB or Docker)
- RabbitMQ (Docker recommended)
- Git

### Docker Setup (Recommended)

```bash
# RabbitMQ with Management UI
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management

# SQL Server
docker run -d --name sqlserver \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

## Phase 1: Foundation Setup

### Step 1: Create Solution Structure

```bash
# Create solution
dotnet new sln -n ModularMonolith

# Create directory structure
mkdir -p src/API
mkdir -p src/Modules/Customers
mkdir -p src/Modules/Orders
mkdir -p src/Modules/Catalog
mkdir -p src/Shared
mkdir -p tests
```

### Step 2: Create Shared Projects

```bash
# Shared Abstractions
dotnet new classlib -n ModularMonolith.Shared.Abstractions -o src/Shared/ModularMonolith.Shared.Abstractions
dotnet sln add src/Shared/ModularMonolith.Shared.Abstractions

# Shared Infrastructure
dotnet new classlib -n ModularMonolith.Shared.Infrastructure -o src/Shared/ModularMonolith.Shared.Infrastructure
dotnet sln add src/Shared/ModularMonolith.Shared.Infrastructure
```

### Step 3: Create API Host

```bash
dotnet new webapi -n ModularMonolith.API -o src/API/ModularMonolith.API
dotnet sln add src/API/ModularMonolith.API
```

### Step 4: Install Core NuGet Packages

**Shared.Abstractions:**
```bash
cd src/Shared/ModularMonolith.Shared.Abstractions
dotnet add package MediatR.Contracts
```

**Shared.Infrastructure:**
```bash
cd src/Shared/ModularMonolith.Shared.Infrastructure
dotnet add package MediatR
dotnet add package MassTransit.RabbitMQ
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Serilog.AspNetCore
dotnet add package FluentValidation
dotnet add package Polly
```

**API Project:**
```bash
cd src/API/ModularMonolith.API
dotnet add package MediatR
dotnet add package MassTransit.RabbitMQ
dotnet add package Serilog.AspNetCore
dotnet add package Swashbuckle.AspNetCore
```

### Step 5: Implement Shared Abstractions

#### Domain Interfaces

**IEntity.cs:**
```csharp
namespace ModularMonolith.Shared.Abstractions.Domain;

public interface IEntity
{
    Guid Id { get; }
}

public interface IEntity<TId> where TId : notnull
{
    TId Id { get; }
}
```

**IAggregateRoot.cs:**
```csharp
namespace ModularMonolith.Shared.Abstractions.Domain;

public interface IAggregateRoot : IEntity
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```

**IDomainEvent.cs:**
```csharp
using MediatR;

namespace ModularMonolith.Shared.Abstractions.Domain;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
```

#### Messaging Interfaces

**ICommand.cs:**
```csharp
using MediatR;

namespace ModularMonolith.Shared.Abstractions.Messaging;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
```

**IQuery.cs:**
```csharp
using MediatR;

namespace ModularMonolith.Shared.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
```

**IIntegrationEvent.cs:**
```csharp
namespace ModularMonolith.Shared.Abstractions.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
```

#### Repository Interfaces

**IRepository.cs:**
```csharp
namespace ModularMonolith.Shared.Abstractions.Persistence;

public interface IRepository<T> where T : IAggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}
```

**IUnitOfWork.cs:**
```csharp
namespace ModularMonolith.Shared.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### Step 6: Implement Shared Infrastructure

#### Base Entity

**Entity.cs:**
```csharp
namespace ModularMonolith.Shared.Infrastructure.Domain;

public abstract class Entity : IEntity
{
    public Guid Id { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    protected Entity(Guid id)
    {
        Id = id;
    }
}
```

**AggregateRoot.cs:**
```csharp
namespace ModularMonolith.Shared.Infrastructure.Domain;

public abstract class AggregateRoot : Entity, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

#### MediatR Behaviors

**ValidationBehavior.cs:**
```csharp
using FluentValidation;
using MediatR;

namespace ModularMonolith.Shared.Infrastructure.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

**LoggingBehavior.cs:**
```csharp
using MediatR;
using Microsoft.Extensions.Logging;

namespace ModularMonolith.Shared.Infrastructure.Behaviors;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling {RequestName}",
            typeof(TRequest).Name);

        var response = await next();

        _logger.LogInformation(
            "Handled {RequestName}",
            typeof(TRequest).Name);

        return response;
    }
}
```

## Phase 2: First Module Implementation (Customers)

### Step 1: Create Module Projects

```bash
# Domain
dotnet new classlib -n ModularMonolith.Modules.Customers.Domain \
  -o src/Modules/Customers/ModularMonolith.Modules.Customers.Domain
dotnet sln add src/Modules/Customers/ModularMonolith.Modules.Customers.Domain

# Application
dotnet new classlib -n ModularMonolith.Modules.Customers.Application \
  -o src/Modules/Customers/ModularMonolith.Modules.Customers.Application
dotnet sln add src/Modules/Customers/ModularMonolith.Modules.Customers.Application

# Infrastructure
dotnet new classlib -n ModularMonolith.Modules.Customers.Infrastructure \
  -o src/Modules/Customers/ModularMonolith.Modules.Customers.Infrastructure
dotnet sln add src/Modules/Customers/ModularMonolith.Modules.Customers.Infrastructure

# Contracts
dotnet new classlib -n ModularMonolith.Modules.Customers.Contracts \
  -o src/Modules/Customers/ModularMonolith.Modules.Customers.Contracts
dotnet sln add src/Modules/Customers/ModularMonolith.Modules.Customers.Contracts
```

### Step 2: Add Project References

```bash
# Application references Domain
cd src/Modules/Customers/ModularMonolith.Modules.Customers.Application
dotnet add reference ../ModularMonolith.Modules.Customers.Domain
dotnet add reference ../../../Shared/ModularMonolith.Shared.Abstractions

# Infrastructure references Application and Domain
cd ../ModularMonolith.Modules.Customers.Infrastructure
dotnet add reference ../ModularMonolith.Modules.Customers.Application
dotnet add reference ../ModularMonolith.Modules.Customers.Domain
dotnet add reference ../../../Shared/ModularMonolith.Shared.Infrastructure

# API references all modules
cd ../../../API/ModularMonolith.API
dotnet add reference ../../Modules/Customers/ModularMonolith.Modules.Customers.Infrastructure
```

### Step 3: Install Module-Specific Packages

**Domain:** (No additional packages needed)

**Application:**
```bash
dotnet add package MediatR.Contracts
dotnet add package FluentValidation
dotnet add package AutoMapper
```

**Infrastructure:**
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package MassTransit
```

### Step 4: Implement Domain Layer

**Customer.cs (Entity):**
```csharp
namespace ModularMonolith.Modules.Customers.Domain.Entities;

public class Customer : AggregateRoot
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Customer() { } // EF Core

    public Customer(string firstName, string lastName, Email email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;

        AddDomainEvent(new CustomerCreatedDomainEvent(Id, email.Value));
    }

    public void UpdateEmail(Email newEmail)
    {
        Email = newEmail;
        AddDomainEvent(new CustomerEmailChangedDomainEvent(Id, newEmail.Value));
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new CustomerDeactivatedDomainEvent(Id));
    }
}
```

**Email.cs (Value Object):**
```csharp
namespace ModularMonolith.Modules.Customers.Domain.ValueObjects;

public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format", nameof(value));

        Value = value;
    }

    private static bool IsValidEmail(string email)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(
            email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
```

**Domain Events:**
```csharp
namespace ModularMonolith.Modules.Customers.Domain.Events;

public record CustomerCreatedDomainEvent(Guid CustomerId, string Email) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
```

### Step 5: Implement Application Layer

**CreateCustomerCommand.cs:**
```csharp
namespace ModularMonolith.Modules.Customers.Application.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email) : ICommand<Guid>;
```

**CreateCustomerCommandHandler.cs:**
```csharp
public class CreateCustomerCommandHandler
    : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(
        ICustomerRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var email = new Email(request.Email);
        var customer = new Customer(
            request.FirstName,
            request.LastName,
            email);

        await _repository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}
```

**CreateCustomerCommandValidator.cs:**
```csharp
using FluentValidation;

public class CreateCustomerCommandValidator
    : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
```

### Step 6: Implement Infrastructure Layer

**CustomersDbContext.cs:**
```csharp
namespace ModularMonolith.Modules.Customers.Infrastructure.Persistence;

public class CustomersDbContext : DbContext, IUnitOfWork
{
    public DbSet<Customer> Customers { get; set; }

    public CustomersDbContext(DbContextOptions<CustomersDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customers");
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CustomersDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        var domainEvents = ChangeTracker.Entries<IAggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events after saving
        foreach (var entry in ChangeTracker.Entries<IAggregateRoot>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }
}
```

**CustomerConfiguration.cs:**
```csharp
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(255);
        });

        builder.HasIndex(c => c.Email.Value)
            .IsUnique();
    }
}
```

**DependencyInjection.cs:**
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddCustomersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CustomersDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Database")));

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<CustomersDbContext>());

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(CreateCustomerCommand).Assembly));

        services.AddValidatorsFromAssembly(
            typeof(CreateCustomerCommand).Assembly);

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
}
```

### Step 7: Add API Controller

**CustomersController.cs:**
```csharp
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ISender _mediator;

    public CustomersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.FirstName,
            request.LastName,
            request.Email);

        var customerId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = customerId },
            new { id = customerId });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomer(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerQuery(id);
        var customer = await _mediator.Send(query, cancellationToken);

        return customer is null ? NotFound() : Ok(customer);
    }
}
```

### Step 8: Configure Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add MediatR behaviors
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

// Add MassTransit for RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });
    });
});

// Add modules
builder.Services.AddCustomersModule(builder.Configuration);

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## Phase 3: Add Additional Modules

Repeat the module creation process for:
1. **Orders Module** (same structure as Customers)
2. **Catalog Module** (same structure as Customers)

## Phase 4: Implement Cross-Module Communication

### Integration Events

**In Customers.Contracts:**
```csharp
public record CustomerCreatedIntegrationEvent(
    Guid CustomerId,
    string Email) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
```

**Publish from Customers Module:**
```csharp
// In CustomerCreatedDomainEventHandler
public class CustomerCreatedDomainEventHandler
    : INotificationHandler<CustomerCreatedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task Handle(
        CustomerCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await _publishEndpoint.Publish(
            new CustomerCreatedIntegrationEvent(
                notification.CustomerId,
                notification.Email),
            cancellationToken);
    }
}
```

**Consume in Orders Module:**
```csharp
public class CustomerCreatedIntegrationEventConsumer
    : IConsumer<CustomerCreatedIntegrationEvent>
{
    private readonly OrdersDbContext _context;

    public async Task Consume(
        ConsumeContext<CustomerCreatedIntegrationEvent> context)
    {
        // Create customer read model in Orders module
        var customerReadModel = new CustomerReadModel
        {
            CustomerId = context.Message.CustomerId,
            Email = context.Message.Email
        };

        _context.CustomerReadModels.Add(customerReadModel);
        await _context.SaveChangesAsync();
    }
}
```

## Phase 5: Database Migrations

```bash
# Add migration for Customers module
dotnet ef migrations add InitialCreate \
  --project src/Modules/Customers/ModularMonolith.Modules.Customers.Infrastructure \
  --startup-project src/API/ModularMonolith.API \
  --context CustomersDbContext

# Apply migrations
dotnet ef database update \
  --project src/Modules/Customers/ModularMonolith.Modules.Customers.Infrastructure \
  --startup-project src/API/ModularMonolith.API \
  --context CustomersDbContext
```

## Testing

### Unit Test Example

```csharp
public class CreateCustomerCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesCustomer()
    {
        // Arrange
        var repository = new Mock<ICustomerRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new CreateCustomerCommandHandler(
            repository.Object,
            unitOfWork.Object);

        var command = new CreateCustomerCommand(
            "John",
            "Doe",
            "john.doe@example.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        repository.Verify(
            r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

## Summary

This roadmap provides:
1. ✅ Complete solution structure
2. ✅ Shared abstractions and infrastructure
3. ✅ Module template (Customers)
4. ✅ Cross-module communication via RabbitMQ
5. ✅ Separate schemas per module
6. ✅ CQRS with MediatR
7. ✅ Clean architecture per module
8. ✅ Easy migration path to microservices

Follow this roadmap step-by-step to build a production-ready modular monolith!
