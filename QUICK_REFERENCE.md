# Quick Reference Guide

## Architecture at a Glance

```
┌─────────────────────────────────────────────────────────────────┐
│                          API Gateway                             │
│                    (ModularMonolith.API)                         │
└───────────────┬──────────────┬──────────────┬───────────────────┘
                │              │              │
        ┌───────▼──────┐ ┌────▼─────┐ ┌──────▼──────┐
        │  Customers   │ │  Orders  │ │   Catalog   │
        │    Module    │ │  Module  │ │   Module    │
        └──────┬───────┘ └────┬─────┘ └──────┬──────┘
               │              │              │
               └──────────────┼──────────────┘
                              │
                    ┌─────────▼─────────┐
                    │   RabbitMQ Bus    │
                    │ (Integration      │
                    │  Events)          │
                    └───────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                         Database                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │   Schema:    │  │   Schema:    │  │   Schema:    │          │
│  │  customers   │  │   orders     │  │   catalog    │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└─────────────────────────────────────────────────────────────────┘
```

## Module Structure Template

```
ModularMonolith.Modules.{ModuleName}/
│
├── Domain/                          (No dependencies)
│   ├── Entities/
│   │   └── {Entity}.cs             (Aggregate roots)
│   ├── ValueObjects/
│   │   └── {ValueObject}.cs        (Immutable values)
│   ├── Events/
│   │   └── {Event}DomainEvent.cs   (Domain events)
│   └── Exceptions/
│       └── {Exception}.cs          (Domain exceptions)
│
├── Application/                     (References: Domain, Shared.Abstractions)
│   ├── Commands/
│   │   └── {Action}/
│   │       ├── {Action}Command.cs
│   │       ├── {Action}CommandHandler.cs
│   │       └── {Action}CommandValidator.cs
│   ├── Queries/
│   │   └── {Query}/
│   │       ├── {Query}Query.cs
│   │       ├── {Query}QueryHandler.cs
│   │       └── {Dto}.cs
│   ├── IntegrationEvents/
│   │   └── Handlers/
│   └── DomainEventHandlers/
│
├── Infrastructure/                  (References: Application, Domain, Shared.Infrastructure)
│   ├── Persistence/
│   │   ├── {Module}DbContext.cs
│   │   ├── Configurations/
│   │   │   └── {Entity}Configuration.cs
│   │   └── Repositories/
│   │       └── {Entity}Repository.cs
│   ├── EventBus/
│   │   └── IntegrationEventPublisher.cs
│   └── DependencyInjection.cs
│
└── Contracts/                       (No dependencies - Pure DTOs)
    └── Events/
        └── {Event}IntegrationEvent.cs
```

## Code Patterns

### 1. Creating an Aggregate Root

```csharp
public class Order : AggregateRoot
{
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // For EF Core

    public Order(CustomerId customerId)
    {
        CustomerId = customerId;
        Status = OrderStatus.Pending;

        AddDomainEvent(new OrderCreatedDomainEvent(Id, customerId));
    }

    public void AddItem(ProductId productId, int quantity, decimal price)
    {
        var item = new OrderItem(productId, quantity, price);
        _items.Add(item);

        AddDomainEvent(new OrderItemAddedDomainEvent(Id, productId, quantity));
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedDomainEvent(Id));
    }
}
```

### 2. Creating a Value Object

```csharp
public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required");

        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");

        return new Money(Amount + other.Amount, Currency);
    }
}
```

### 3. Command Pattern

```csharp
// Command
public record CreateOrderCommand(
    Guid CustomerId,
    List<OrderItemDto> Items) : ICommand<Guid>;

// Handler
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var customerId = new CustomerId(request.CustomerId);
        var order = new Order(customerId);

        foreach (var item in request.Items)
        {
            order.AddItem(
                new ProductId(item.ProductId),
                item.Quantity,
                item.Price);
        }

        await _repository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}

// Validator
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();

        RuleFor(x => x.Items)
            .NotEmpty()
            .Must(items => items.Count > 0)
            .WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemDtoValidator());
    }
}
```

### 4. Query Pattern

```csharp
// Query
public record GetOrderQuery(Guid OrderId) : IQuery<OrderDto?>;

// Handler
public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly OrdersDbContext _context;

    public async Task<OrderDto?> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Where(o => o.Id == request.OrderId)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                Status = o.Status.ToString(),
                Items = o.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

### 5. Domain Event Pattern

```csharp
// Domain Event
public record OrderConfirmedDomainEvent(Guid OrderId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Domain Event Handler
public class OrderConfirmedDomainEventHandler
    : INotificationHandler<OrderConfirmedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task Handle(
        OrderConfirmedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        // Publish integration event for other modules
        await _publishEndpoint.Publish(
            new OrderConfirmedIntegrationEvent(notification.OrderId),
            cancellationToken);
    }
}
```

### 6. Integration Event Pattern

```csharp
// Integration Event (in Contracts project)
public record OrderConfirmedIntegrationEvent(Guid OrderId) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

// Consumer (in another module)
public class OrderConfirmedIntegrationEventConsumer
    : IConsumer<OrderConfirmedIntegrationEvent>
{
    private readonly ILogger<OrderConfirmedIntegrationEventConsumer> _logger;
    private readonly CustomersDbContext _context;

    public async Task Consume(
        ConsumeContext<OrderConfirmedIntegrationEvent> context)
    {
        _logger.LogInformation(
            "Processing order confirmation for order {OrderId}",
            context.Message.OrderId);

        // Update customer statistics
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == context.Message.CustomerId);

        if (customer != null)
        {
            customer.IncrementOrderCount();
            await _context.SaveChangesAsync();
        }
    }
}
```

### 7. EF Core Configuration

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", "orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => new OrderId(value));

        builder.Property(o => o.CustomerId)
            .HasConversion(
                id => id.Value,
                value => new CustomerId(value))
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.OwnsMany(o => o.Items, items =>
        {
            items.ToTable("OrderItems", "orders");
            items.WithOwner().HasForeignKey("OrderId");
            items.HasKey("Id");

            items.Property(i => i.ProductId)
                .HasConversion(
                    id => id.Value,
                    value => new ProductId(value));

            items.Property<decimal>("Price")
                .HasPrecision(18, 2);
        });

        // Ignore domain events (not persisted)
        builder.Ignore(o => o.DomainEvents);
    }
}
```

### 8. Repository Pattern

```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdWithItemsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Order>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByIdWithItemsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        Order entity,
        CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(entity, cancellationToken);
    }

    public void Update(Order entity)
    {
        _context.Orders.Update(entity);
    }

    public void Remove(Order entity)
    {
        _context.Orders.Remove(entity);
    }
}
```

### 9. Controller Pattern

```csharp
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly ISender _mediator;

    public OrdersController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.Items);

        var orderId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = orderId },
            new { id = orderId });
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(id);
        var order = await _mediator.Send(query, cancellationToken);

        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// Confirm an order
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmOrderCommand(id);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
```

### 10. Module Registration (DependencyInjection.cs)

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddOrdersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Database"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        "orders");
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                }));

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<OrdersDbContext>());

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(CreateOrderCommand).Assembly);

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();

        // AutoMapper
        services.AddAutoMapper(typeof(CreateOrderCommand).Assembly);

        // MassTransit Consumers
        services.AddMassTransitConsumers();

        return services;
    }

    private static IServiceCollection AddMassTransitConsumers(
        this IServiceCollection services)
    {
        services.AddScoped<CustomerCreatedIntegrationEventConsumer>();
        services.AddScoped<ProductPriceChangedIntegrationEventConsumer>();

        return services;
    }
}
```

## Common Commands

### Create Migration
```bash
dotnet ef migrations add <MigrationName> \
  --project src/Modules/<Module>/ModularMonolith.Modules.<Module>.Infrastructure \
  --startup-project src/API/ModularMonolith.API \
  --context <Module>DbContext
```

### Apply Migration
```bash
dotnet ef database update \
  --project src/Modules/<Module>/ModularMonolith.Modules.<Module>.Infrastructure \
  --startup-project src/API/ModularMonolith.API \
  --context <Module>DbContext
```

### Run Application
```bash
dotnet run --project src/API/ModularMonolith.API
```

### Run Tests
```bash
dotnet test
```

## Configuration Checklist

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
  "Serilog": {
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
        "Name": "File",
        "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" }
      }
    ]
  }
}
```

## Module Communication Rules

1. ✅ **Allowed**: Module A publishes integration event → Module B consumes it
2. ✅ **Allowed**: Module uses MediatR internally for commands/queries
3. ✅ **Allowed**: Module references Shared.Abstractions and Shared.Infrastructure
4. ❌ **Forbidden**: Module A directly references Module B's Domain/Application/Infrastructure
5. ❌ **Forbidden**: Module A directly queries Module B's database
6. ❌ **Forbidden**: Synchronous calls between modules (use async events)

## Migration to Microservices Checklist

When ready to extract a module:

- [ ] Module has no direct project references to other modules
- [ ] All cross-module communication uses integration events
- [ ] Module has its own database schema
- [ ] Module can be built independently
- [ ] Integration tests pass in isolation
- [ ] Create new solution for the microservice
- [ ] Copy module projects to new solution
- [ ] Add standalone API host
- [ ] Update RabbitMQ configuration for distributed mode
- [ ] Migrate database schema to separate database
- [ ] Deploy to container orchestration platform
- [ ] Update API gateway routing

## Key Benefits

1. **Modularity**: Clear boundaries, easy to understand
2. **Testability**: Each module can be tested independently
3. **Scalability**: Can extract modules to microservices
4. **Maintainability**: Changes isolated to specific modules
5. **Team Autonomy**: Teams can work on different modules
6. **Performance**: In-process communication (no network overhead)
7. **Consistency**: Shared infrastructure and patterns
