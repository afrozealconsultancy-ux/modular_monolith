# Modular Monolith Architecture Plan

## Overview
This document outlines the architecture for a .NET Core modular monolith designed for easy migration to microservices.

## Core Principles

1. **Module Independence**: Each module is self-contained with clear boundaries
2. **Separate Schemas**: Each module owns its data in a separate database schema
3. **Async Communication**: Modules communicate via events (RabbitMQ)
4. **CQRS Pattern**: MediatR for command/query separation
5. **Microservice-Ready**: Clear contracts and no direct dependencies between modules

## Technology Stack

- **.NET 8.0** (latest LTS)
- **MediatR** - In-process messaging and CQRS
- **RabbitMQ + MassTransit** - Async messaging between modules
- **Entity Framework Core** - Data access with separate schemas
- **FluentValidation** - Request validation
- **Serilog** - Structured logging
- **Polly** - Resilience and transient fault handling
- **Swagger/OpenAPI** - API documentation
- **xUnit** - Testing framework

## Solution Structure

```
ModularMonolith/
├── src/
│   ├── API/
│   │   └── ModularMonolith.API/                    # API Gateway/Host
│   │       ├── Controllers/
│   │       ├── Middleware/
│   │       └── Program.cs
│   │
│   ├── Modules/
│   │   ├── Customers/
│   │   │   ├── ModularMonolith.Modules.Customers.Domain/
│   │   │   │   ├── Entities/
│   │   │   │   ├── ValueObjects/
│   │   │   │   ├── Events/
│   │   │   │   └── Exceptions/
│   │   │   │
│   │   │   ├── ModularMonolith.Modules.Customers.Application/
│   │   │   │   ├── Commands/
│   │   │   │   ├── Queries/
│   │   │   │   ├── DTOs/
│   │   │   │   ├── Validators/
│   │   │   │   ├── Mappings/
│   │   │   │   └── IntegrationEvents/
│   │   │   │
│   │   │   ├── ModularMonolith.Modules.Customers.Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   │   ├── CustomersDbContext.cs
│   │   │   │   │   ├── Configurations/
│   │   │   │   │   └── Repositories/
│   │   │   │   ├── EventBus/
│   │   │   │   └── DependencyInjection.cs
│   │   │   │
│   │   │   └── ModularMonolith.Modules.Customers.Contracts/
│   │   │       └── Events/                         # Public events for other modules
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
│       │   └── Persistence/
│       │       ├── IRepository.cs
│       │       └── IUnitOfWork.cs
│       │
│       └── ModularMonolith.Shared.Infrastructure/
│           ├── EventBus/
│           │   ├── RabbitMQ/
│           │   └── InMemory/                       # For development/testing
│           ├── Messaging/
│           │   └── MediatR/
│           ├── Persistence/
│           │   └── EFCore/
│           └── Behaviors/
│               ├── LoggingBehavior.cs
│               ├── ValidationBehavior.cs
│               └── TransactionBehavior.cs
│
└── tests/
    ├── ModularMonolith.Modules.Customers.Tests/
    ├── ModularMonolith.Modules.Orders.Tests/
    └── ModularMonolith.IntegrationTests/
```

## Module Design Pattern

Each module follows the **Clean Architecture** pattern with four layers:

### 1. Domain Layer
- **Purpose**: Core business logic, entities, value objects
- **Dependencies**: None (pure domain logic)
- **Contents**:
  - Entities (with business rules)
  - Value Objects
  - Domain Events
  - Domain Exceptions
  - Specifications

### 2. Application Layer
- **Purpose**: Use cases, orchestration, DTOs
- **Dependencies**: Domain Layer, Shared.Abstractions
- **Contents**:
  - Commands (via MediatR IRequest)
  - Queries (via MediatR IRequest)
  - Command/Query Handlers
  - DTOs (Data Transfer Objects)
  - Validators (FluentValidation)
  - Integration Events (for publishing to other modules)
  - AutoMapper Profiles

### 3. Infrastructure Layer
- **Purpose**: Technical implementations (DB, messaging, external services)
- **Dependencies**: Application, Domain, Shared.Infrastructure
- **Contents**:
  - EF Core DbContext (with schema configuration)
  - Entity Configurations
  - Repositories
  - Event Bus implementations
  - External Service integrations
  - DependencyInjection.cs (module registration)

### 4. Contracts Layer
- **Purpose**: Public API for other modules
- **Dependencies**: None (pure DTOs and interfaces)
- **Contents**:
  - Integration Events (for other modules to consume)
  - Public DTOs
  - Interfaces for cross-module communication

## Database Strategy

### Separate Schemas Per Module

Each module uses its own database schema within a shared database:

```csharp
// Customers Module
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema("customers");
    // ... configurations
}

// Orders Module
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema("orders");
    // ... configurations
}
```

### Migration Path
- **Phase 1 (Monolith)**: All modules in one database, separate schemas
- **Phase 2 (Transition)**: Move each schema to separate database
- **Phase 3 (Microservices)**: Deploy each module independently

### Data Consistency Rules
1. **Within Module**: Use transactions (UnitOfWork pattern)
2. **Cross-Module**: Use eventual consistency via events
3. **No Direct DB Access**: Modules never query other module's data directly

## Communication Patterns

### 1. Internal Communication (Within Module)
**Use**: MediatR for Commands and Queries

```csharp
// Command
public record CreateCustomerCommand(string Name, string Email) : ICommand<Guid>;

// Handler
public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    // Implementation
}

// Usage
var customerId = await _mediator.Send(new CreateCustomerCommand(name, email));
```

### 2. Cross-Module Communication
**Use**: RabbitMQ via MassTransit for Integration Events

```csharp
// 1. Define event in Contracts project
public record CustomerCreatedEvent(Guid CustomerId, string Email);

// 2. Publish from Customers module
await _publishEndpoint.Publish(new CustomerCreatedEvent(customerId, email));

// 3. Consume in Orders module
public class CustomerCreatedEventConsumer : IConsumer<CustomerCreatedEvent>
{
    public async Task Consume(ConsumeContext<CustomerCreatedEvent> context)
    {
        // Handle event
    }
}
```

### 3. Event Types

- **Domain Events**: Internal to module, handled in same transaction
- **Integration Events**: Cross-module, async via RabbitMQ
- **Public Events**: Defined in Contracts project for external consumption

## Key Infrastructure Components

### 1. MediatR Pipeline Behaviors

```csharp
// Logging
LoggingBehavior<TRequest, TResponse>

// Validation
ValidationBehavior<TRequest, TResponse>

// Transaction Management
TransactionBehavior<TRequest, TResponse>

// Exception Handling
ExceptionBehavior<TRequest, TResponse>
```

### 2. Event Bus Abstraction

```csharp
public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : IIntegrationEvent;
}

// Implementations:
// - RabbitMQEventBus (production)
// - InMemoryEventBus (development/testing)
```

### 3. Module Registration

Each module provides a DependencyInjection extension:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddCustomersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext with separate schema
        services.AddDbContext<CustomersDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Database")));

        // Register MediatR handlers
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Register validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
```

## API Layer Design

### Modular Endpoints

Each module exposes endpoints via its own controller:

```csharp
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ISender _mediator;

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(CreateCustomerRequest request)
    {
        var command = new CreateCustomerCommand(request.Name, request.Email);
        var customerId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCustomer), new { id = customerId }, customerId);
    }
}
```

### API Versioning
- Use URL versioning: `/api/v1/customers`
- Prepare for service extraction

## Testing Strategy

### 1. Unit Tests
- Domain logic
- Command/Query handlers
- Validators

### 2. Integration Tests
- API endpoints
- Database operations
- Event publishing/consuming

### 3. Architecture Tests
- Enforce dependency rules
- Verify module boundaries
- Check for coupling

```csharp
// Using NetArchTest
[Fact]
public void Domain_Should_Not_Depend_On_Infrastructure()
{
    var result = Types.InAssembly(DomainAssembly)
        .Should()
        .NotHaveDependencyOn("Infrastructure")
        .GetResult();

    Assert.True(result.IsSuccessful);
}
```

## Migration to Microservices

### Design Decisions Supporting Migration

1. **Module Independence**
   - No direct project references between modules
   - Communication via events only

2. **Separate Data Schemas**
   - Each module owns its data
   - Easy to move to separate database

3. **API Gateway Ready**
   - Controllers organized by module
   - Can be extracted to separate services

4. **Service Discovery Ready**
   - Use configuration for module endpoints
   - Easy to replace with actual service URLs

### Migration Steps (Future)

1. **Extract Module**
   - Copy module projects to new solution
   - Add API host project
   - Update RabbitMQ configuration

2. **Update Event Bus**
   - Change from in-process to remote
   - Update service registration

3. **Database Migration**
   - Export schema to new database
   - Update connection strings

4. **Deploy Independently**
   - Container orchestration (Kubernetes/Docker)
   - Service mesh (optional)

## Development Workflow

### Phase 1: Foundation (Week 1-2)
1. Create solution structure
2. Setup shared abstractions
3. Setup shared infrastructure
4. Configure RabbitMQ + MassTransit
5. Setup API host with middleware

### Phase 2: First Module - Customers (Week 2-3)
1. Domain entities and value objects
2. Application commands and queries
3. Infrastructure with EF Core
4. API controllers
5. Unit and integration tests

### Phase 3: Additional Modules (Week 3-5)
1. Orders module
2. Catalog module
3. Cross-module integration events
4. End-to-end scenarios

### Phase 4: Enhancements (Week 5-6)
1. Add resilience patterns
2. Implement caching
3. Add monitoring and observability
4. Performance optimization

## Configuration Example

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Database": "Server=localhost;Database=ModularMonolith;Trusted_Connection=true"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  },
  "Modules": {
    "Customers": {
      "Enabled": true
    },
    "Orders": {
      "Enabled": true
    },
    "Catalog": {
      "Enabled": true
    }
  }
}
```

## Sample Modules

### Customers Module
- Customer registration
- Customer profile management
- Customer events (created, updated, deleted)

### Orders Module
- Order creation and management
- Order status tracking
- Integration with Customers (listen to customer events)
- Integration with Catalog (product availability)

### Catalog Module
- Product management
- Inventory tracking
- Product events (price changed, stock updated)

## Best Practices

1. **Module Boundaries**
   - Define clear bounded contexts
   - Avoid shared entities between modules
   - Use integration events for cross-module data needs

2. **Event Design**
   - Events are immutable
   - Events contain minimal necessary data
   - Use correlation IDs for tracing

3. **Error Handling**
   - Domain exceptions for business rule violations
   - Global exception handler in API layer
   - Proper HTTP status codes

4. **Performance**
   - Use async/await throughout
   - Implement caching where appropriate
   - Use projections for read models (CQRS)

5. **Security**
   - Authentication/Authorization at API gateway
   - Validate all inputs
   - Never trust data from other modules

## Monitoring & Observability

- **Logging**: Serilog with structured logging
- **Tracing**: OpenTelemetry for distributed tracing
- **Metrics**: Application metrics and health checks
- **Correlation**: Correlation IDs across module boundaries

## Next Steps

1. Review and approve this architecture plan
2. Setup development environment (SQL Server, RabbitMQ)
3. Create initial solution structure
4. Implement shared infrastructure
5. Build first module (Customers) as template
6. Replicate pattern for additional modules
