# Real Modular Monolith Architecture

## Core Principle: TRUE Module Independence

This is a **REAL modular monolith**, not just folders. Each module is:
- ✅ **Self-contained** - Has everything it needs to function
- ✅ **Independent** - No direct references to other modules
- ✅ **Microservice-ready** - Can be extracted and deployed separately with minimal changes

## Module Structure

Each module follows this structure:

```
Modules/
└── {ModuleName}/
    ├── {ModuleName}.Domain/          # Domain entities, value objects, domain events
    │   ├── Entities/
    │   ├── ValueObjects/
    │   ├── Events/
    │   └── Exceptions/
    │
    ├── {ModuleName}.Application/     # Use cases, commands, queries
    │   ├── Commands/
    │   │   ├── CreateXCommand.cs
    │   │   └── CreateXCommandHandler.cs
    │   ├── Queries/
    │   │   ├── GetXQuery.cs
    │   │   └── GetXQueryHandler.cs
    │   └── IntegrationEvents/        # Events this module publishes
    │       └── XCreatedEvent.cs
    │
    ├── {ModuleName}.Infrastructure/  # Data access, external services
    │   ├── Persistence/
    │   │   ├── {ModuleName}DbContext.cs
    │   │   ├── Configurations/
    │   │   └── Migrations/
    │   ├── Repositories/
    │   └── EventHandlers/            # Handles events from OTHER modules
    │       └── YCreatedEventHandler.cs
    │
    └── {ModuleName}.Contracts/       # Public contracts (DTOs, events)
        ├── Requests/
        ├── Responses/
        └── Events/                   # Event contracts (shared)
```

## Dependency Rules

### ✅ ALLOWED Dependencies

```
Domain          → (nothing)
Application     → Domain, Shared.Abstractions
Infrastructure  → Domain, Application, Shared.Abstractions, Shared.Infrastructure
Contracts       → (nothing, or only Shared.Abstractions)
```

### ❌ FORBIDDEN Dependencies

```
Module A → Module B (any layer)
```

**NEVER:**
- Add project reference from one module to another
- Share domain entities between modules
- Call another module's services directly

## Module Communication: Integration Events Only

### Publishing Events

```csharp
// In Customers module
public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly ICustomerRepository _repository;
    private readonly IEventBus _eventBus;

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(request.Name, request.Email);
        await _repository.AddAsync(customer);

        // Publish integration event for OTHER modules
        await _eventBus.PublishAsync(new CustomerCreatedIntegrationEvent
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerEmail = customer.Email,
            TenantId = customer.TenantId
        }, cancellationToken);

        return customer.Id;
    }
}
```

### Consuming Events

```csharp
// In Orders module - listens to CustomerCreatedIntegrationEvent
public class CustomerCreatedEventHandler : IConsumer<CustomerCreatedIntegrationEvent>
{
    private readonly OrdersDbContext _dbContext;

    public async Task Consume(ConsumeContext<CustomerCreatedIntegrationEvent> context)
    {
        var @event = context.Message;

        // Create a local read model or cache customer info
        var customerReadModel = new CustomerReadModel
        {
            CustomerId = @event.CustomerId,
            Name = @event.CustomerName,
            Email = @event.CustomerEmail,
            TenantId = @event.TenantId
        };

        _dbContext.CustomerReadModels.Add(customerReadModel);
        await _dbContext.SaveChangesAsync();
    }
}
```

## Data Isolation: Each Module Owns Its Data

### Separate Schemas in PostgreSQL

```sql
-- Customers module
CREATE SCHEMA customers;
CREATE TABLE customers.customers (...);

-- Orders module
CREATE SCHEMA orders;
CREATE TABLE orders.orders (...);
CREATE TABLE orders.customer_read_models (...);  -- Denormalized from events!

-- Catalog module
CREATE SCHEMA catalog;
CREATE TABLE catalog.products (...);
```

### Module-Specific DbContext

```csharp
// CustomersDbContext.cs
public class CustomersDbContext : BaseDbContext
{
    public CustomersDbContext(
        DbContextOptions<CustomersDbContext> options,
        ITenantContext tenantContext,
        IDateTimeProvider dateTimeProvider)
        : base(options, tenantContext, dateTimeProvider)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customers");  // Separate schema!
        base.OnModelCreating(modelBuilder);
    }
}
```

### Independent Migrations

```bash
# Each module has its own migrations
dotnet ef migrations add InitialCreate \
    --project src/Modules/Customers/Customers.Infrastructure \
    --context CustomersDbContext \
    --output-dir Persistence/Migrations

dotnet ef migrations add InitialCreate \
    --project src/Modules/Orders/Orders.Infrastructure \
    --context OrdersDbContext \
    --output-dir Persistence/Migrations
```

## Module Registration

Each module implements `IModule`:

```csharp
// CustomersModule.cs
public class CustomersModule : IModule
{
    public string Name => "Customers";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register module-specific services
        services.AddModuleDbContext<CustomersDbContext>(configuration, "customers");
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Register MediatR handlers from this module
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CustomersModule).Assembly));

        // Register MassTransit consumers (event handlers) from this module
        services.AddMassTransit(x =>
        {
            x.AddConsumersFromNamespaceContaining<CustomersModule>();
        });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/customers")
            .RequireAuthorization()
            .WithTags("Customers");

        group.MapPost("/", CreateCustomer);
        group.MapGet("/{id}", GetCustomer);
        group.MapGet("/", GetCustomers);
        group.MapPut("/{id}", UpdateCustomer);
        group.MapDelete("/{id}", DeleteCustomer);
    }

    public async Task InitializeAsync(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    // Endpoint handlers
    private static async Task<IResult> CreateCustomer(
        [FromBody] CreateCustomerCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.IsSuccess
            ? Results.Created($"/api/customers/{result.Value}", result.Value)
            : Results.BadRequest(result.Error);
    }

    // ... other endpoints
}
```

## API Host: Module Loader

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add shared infrastructure
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddKeycloakAuthentication(builder.Configuration);

// Discover and register all modules
var modules = DiscoverModules();
foreach (var module in modules)
{
    if (IsModuleEnabled(module, builder.Configuration))
    {
        module.RegisterServices(builder.Services, builder.Configuration);
    }
}

var app = builder.Build();

// Configure middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// Map module endpoints
foreach (var module in modules)
{
    if (IsModuleEnabled(module, app.Configuration))
    {
        module.MapEndpoints(app);
        await module.InitializeAsync(app);
    }
}

app.Run();

// Helper methods
List<IModule> DiscoverModules()
{
    return new List<IModule>
    {
        new IdentityModule(),
        new StorageModule(),
        new NotificationsModule(),
        new CustomersModule(),
        new OrdersModule(),
        new CatalogModule()
    };
}

bool IsModuleEnabled(IModule module, IConfiguration configuration)
{
    return configuration.GetValue<bool>($"Modules:{module.Name}:Enabled", true);
}
```

## Configuration: Enable/Disable Modules

```json
{
  "Modules": {
    "Identity": { "Enabled": true },
    "Storage": { "Enabled": true },
    "Notifications": { "Enabled": true },
    "Customers": { "Enabled": true },
    "Orders": { "Enabled": true },
    "Catalog": { "Enabled": false }  // Disable module without code changes!
  },
  "ConnectionStrings": {
    "Database": "Host=localhost;Database=modular_monolith;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

## Migration to Microservices

### Current State (Monolith)

```
Single Deployment
├── API Host (Program.cs)
├── Module: Customers
├── Module: Orders
└── Module: Catalog

PostgreSQL (Single Database)
├── Schema: customers
├── Schema: orders
└── Schema: catalog

RabbitMQ (Shared)
├── customers.customer-created
├── orders.order-created
└── catalog.product-created
```

### Future State (Microservices)

```
Customers Service
├── Program.cs (from module)
├── CustomersModule
└── PostgreSQL customers schema

Orders Service
├── Program.cs (from module)
├── OrdersModule
└── PostgreSQL orders schema

Catalog Service
├── Program.cs (from module)
├── CatalogModule
└── PostgreSQL catalog schema

RabbitMQ (Shared - same exchanges)
├── customers.customer-created
├── orders.order-created
└── catalog.product-created
```

### Extraction Steps

1. **Copy module folder** to new repository
   ```bash
   cp -r src/Modules/Customers customer-service/
   cp -r src/Shared customer-service/src/Shared
   ```

2. **Add Program.cs** (minimal changes)
   ```csharp
   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddSharedInfrastructure(builder.Configuration);
   builder.Services.AddKeycloakAuthentication(builder.Configuration);

   var module = new CustomersModule();
   module.RegisterServices(builder.Services, builder.Configuration);

   var app = builder.Build();

   app.UseMiddleware<ExceptionHandlingMiddleware>();
   app.UseAuthentication();
   app.UseAuthorization();

   module.MapEndpoints(app);
   await module.InitializeAsync(app);

   app.Run();
   ```

3. **Update connection string** (optional - separate database)
   ```json
   {
     "ConnectionStrings": {
       "Database": "Host=customers-db;Database=customers;..."
     }
   }
   ```

4. **Deploy independently**
   - Build Docker image
   - Deploy to Kubernetes/Cloud
   - Update API Gateway routes

**Time estimate:** 1-2 days per module (mostly DevOps, not code changes)

## Benefits of This Architecture

### ✅ Development

- **Clear boundaries** - Developers know exactly where code belongs
- **Parallel development** - Teams work on different modules independently
- **Easy onboarding** - New developers work on one module at a time
- **Reduced conflicts** - Changes in one module don't affect others

### ✅ Testing

- **Module isolation** - Test modules independently
- **Fast feedback** - Run tests for changed module only
- **Integration tests** - Test via events, not direct calls

### ✅ Deployment

- **Independent releases** - Update one module without touching others
- **Gradual migration** - Move to microservices one module at a time
- **Rollback safety** - Roll back one module independently

### ✅ Scalability

- **Selective scaling** - Scale busy modules independently (when microservices)
- **Performance** - Optimize module databases separately
- **Caching** - Module-specific caching strategies

### ✅ Team Autonomy

- **Module ownership** - Teams own their modules
- **Technology choice** - (With microservices) Teams choose their stack
- **Independent roadmap** - Modules evolve independently

## Anti-Patterns to Avoid

### ❌ Direct Module References

```csharp
// ❌ WRONG - Orders module references Customers module
public class OrdersModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>(); // From Customers module!
    }
}
```

### ❌ Shared Domain Entities

```csharp
// ❌ WRONG - Sharing Customer entity between modules
public class Order
{
    public Customer Customer { get; set; } // Domain entity from Customers module!
}
```

### ❌ Cross-Module Database Queries

```csharp
// ❌ WRONG - Orders module querying Customers table
public class OrderService
{
    public async Task<Order> GetOrderWithCustomer(Guid orderId)
    {
        var order = await _ordersDbContext.Orders.FindAsync(orderId);
        var customer = await _customersDbContext.Customers.FindAsync(order.CustomerId); // Wrong!
        // ...
    }
}
```

### ✅ Correct Approach: Events + Read Models

```csharp
// ✅ CORRECT - Orders module has its own customer read model
public class OrderService
{
    public async Task<Order> GetOrderWithCustomer(Guid orderId)
    {
        var order = await _ordersDbContext.Orders.FindAsync(orderId);
        var customer = await _ordersDbContext.CustomerReadModels.FindAsync(order.CustomerId); // Own data!
        // ...
    }
}

// CustomerReadModel is kept in sync via events:
public class CustomerCreatedEventHandler : IConsumer<CustomerCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CustomerCreatedIntegrationEvent> context)
    {
        var customer = new CustomerReadModel { /* map from event */ };
        await _ordersDbContext.CustomerReadModels.AddAsync(customer);
        await _ordersDbContext.SaveChangesAsync();
    }
}
```

## Summary

This is a **REAL modular monolith** where:

1. ✅ Modules are **truly independent**
2. ✅ Communication via **integration events only**
3. ✅ Each module has its **own database schema**
4. ✅ No direct references between modules
5. ✅ Easy to extract to **microservices** (copy folder → deploy)
6. ✅ Can enable/disable modules via **configuration**
7. ✅ Tenant isolation **automatically applied** at every layer

**Moving to microservices = Copy folder + Add Program.cs + Deploy**

No refactoring needed! 🎉
