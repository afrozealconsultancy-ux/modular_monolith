# .NET Core Modular Monolith

A production-ready modular monolith architecture built with .NET 8, designed for easy migration to microservices.

## 🎯 Project Goals

Build a scalable, maintainable application with:
- **Modular Design**: Self-contained modules with clear boundaries
- **Separate Schemas**: Each module owns its data
- **Async Messaging**: RabbitMQ for cross-module communication
- **CQRS Pattern**: MediatR for command/query separation
- **Microservice Ready**: Easy extraction to independent services

## 📚 Documentation

### Getting Started
1. **[Architecture Plan](ARCHITECTURE_PLAN.md)** - Understand the overall architecture, design principles, and technology stack
2. **[Implementation Roadmap](IMPLEMENTATION_ROADMAP.md)** - Follow step-by-step implementation guide with code samples
3. **[Quick Reference](QUICK_REFERENCE.md)** - Common patterns and code templates

### Key Documents
- `ARCHITECTURE_PLAN.md` - Complete architectural overview
- `IMPLEMENTATION_ROADMAP.md` - Detailed implementation steps
- `QUICK_REFERENCE.md` - Code patterns and templates

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                       API Gateway                            │
│                  (Single Entry Point)                        │
└─────────────┬──────────────┬────────────┬───────────────────┘
              │              │            │
      ┌───────▼──────┐ ┌────▼────┐ ┌─────▼──────┐
      │  Customers   │ │ Orders  │ │  Catalog   │
      │   Module     │ │ Module  │ │  Module    │
      └──────┬───────┘ └────┬────┘ └─────┬──────┘
             │              │            │
             └──────────────┼────────────┘
                            │
                 ┌──────────▼──────────┐
                 │    RabbitMQ Bus     │
                 │ (Integration Events)│
                 └─────────────────────┘

Database: Single Database, Separate Schemas per Module
```

## 🛠️ Technology Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | .NET 8.0 |
| **Messaging** | MediatR (in-process), MassTransit + RabbitMQ (cross-module) |
| **Data Access** | Entity Framework Core |
| **Validation** | FluentValidation |
| **Logging** | Serilog |
| **Resilience** | Polly |
| **API Documentation** | Swagger/OpenAPI |
| **Testing** | xUnit, FluentAssertions, Moq |

## 📁 Solution Structure

```
ModularMonolith/
├── src/
│   ├── API/
│   │   └── ModularMonolith.API/              # API Gateway
│   │
│   ├── Modules/
│   │   ├── Customers/                        # Customer Module
│   │   │   ├── Domain/                       # Business logic
│   │   │   ├── Application/                  # Use cases
│   │   │   ├── Infrastructure/               # DB, messaging
│   │   │   └── Contracts/                    # Public API
│   │   │
│   │   ├── Orders/                           # Orders Module
│   │   └── Catalog/                          # Catalog Module
│   │
│   └── Shared/
│       ├── Abstractions/                     # Common interfaces
│       └── Infrastructure/                   # Shared implementations
│
└── tests/
    ├── Customers.Tests/
    ├── Orders.Tests/
    └── IntegrationTests/
```

## 🎨 Design Principles

### Module Independence
- Each module is self-contained
- No direct project references between modules
- Communication only via integration events

### Database Per Module
- Separate schema for each module
- Each module owns its data
- No cross-schema queries

### CQRS Pattern
- Commands: Write operations (CreateOrderCommand)
- Queries: Read operations (GetOrderQuery)
- Separation of concerns

### Clean Architecture
Each module follows:
```
Domain → Application → Infrastructure
  ↑                        ↓
  └────────────────────────┘
```

### Event-Driven Communication
- **Domain Events**: Internal to module
- **Integration Events**: Cross-module via RabbitMQ

## 🚀 Quick Start

### Prerequisites
```bash
# .NET 8 SDK
dotnet --version  # Should be 8.0.x

# Docker (for RabbitMQ and SQL Server)
docker --version
```

### 1. Start Infrastructure

```bash
# Start RabbitMQ
docker run -d --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3-management

# Start SQL Server
docker run -d --name sqlserver \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Follow Implementation Roadmap

See [IMPLEMENTATION_ROADMAP.md](IMPLEMENTATION_ROADMAP.md) for detailed step-by-step instructions.

### 3. Run the Application

```bash
# Build solution
dotnet build

# Run migrations
dotnet ef database update --project src/Modules/Customers/Infrastructure

# Run application
dotnet run --project src/API/ModularMonolith.API

# Access Swagger UI
# Navigate to: https://localhost:5001/swagger
```

## 🧪 Testing Strategy

### Unit Tests
Test individual components in isolation:
- Domain entities and value objects
- Command/Query handlers
- Validators

### Integration Tests
Test module interactions:
- API endpoints
- Database operations
- Event publishing and consumption

### Architecture Tests
Enforce architectural rules:
- Module dependency constraints
- Layer boundaries
- Naming conventions

Example:
```csharp
[Fact]
public void Domain_Should_Not_Reference_Infrastructure()
{
    var result = Types.InAssembly(DomainAssembly)
        .Should()
        .NotHaveDependencyOn("Infrastructure")
        .GetResult();

    Assert.True(result.IsSuccessful);
}
```

## 📦 Sample Modules

### Customers Module
**Responsibilities:**
- Customer registration and management
- Customer profile updates
- Email validation and uniqueness

**Events Published:**
- `CustomerCreatedIntegrationEvent`
- `CustomerEmailChangedIntegrationEvent`
- `CustomerDeactivatedIntegrationEvent`

### Orders Module
**Responsibilities:**
- Order creation and management
- Order status tracking
- Order item management

**Events Published:**
- `OrderCreatedIntegrationEvent`
- `OrderConfirmedIntegrationEvent`
- `OrderShippedIntegrationEvent`

**Events Consumed:**
- `CustomerCreatedIntegrationEvent` (maintains customer read model)
- `ProductPriceChangedIntegrationEvent` (updates pricing)

### Catalog Module
**Responsibilities:**
- Product management
- Inventory tracking
- Price management

**Events Published:**
- `ProductCreatedIntegrationEvent`
- `ProductPriceChangedIntegrationEvent`
- `ProductStockUpdatedIntegrationEvent`

## 🔄 Migration to Microservices

The architecture is designed for easy extraction of modules into microservices:

### Current State (Monolith)
- Single deployment
- In-process module communication
- Single database with separate schemas
- Shared infrastructure

### Future State (Microservices)
- Independent deployments
- RabbitMQ for all communication
- Separate databases
- Duplicated infrastructure per service

### Migration Steps
1. Select module to extract
2. Verify module independence (no direct references)
3. Create new solution for microservice
4. Copy module projects
5. Add standalone API host
6. Migrate database schema to new database
7. Update RabbitMQ configuration
8. Deploy independently
9. Update API gateway routing

See [ARCHITECTURE_PLAN.md](ARCHITECTURE_PLAN.md) for detailed migration strategy.

## 🔐 Best Practices

### Module Communication
✅ **DO**
- Use integration events for cross-module communication
- Define events in Contracts project
- Use correlation IDs for tracing
- Handle events idempotently

❌ **DON'T**
- Reference other module's Domain/Application/Infrastructure
- Query other module's database directly
- Use synchronous calls between modules
- Share entities between modules

### Database
✅ **DO**
- Use separate schema per module
- Use migrations per module
- Use repository pattern
- Use Unit of Work for transactions

❌ **DON'T**
- Access other module's tables
- Share database connections
- Use raw SQL for cross-schema queries

### Error Handling
✅ **DO**
- Use domain exceptions for business rules
- Use global exception handler in API
- Log all errors with correlation IDs
- Return proper HTTP status codes

❌ **DON'T**
- Swallow exceptions
- Expose internal error details to clients
- Use generic exception types

## 📊 Monitoring & Observability

### Logging
- Structured logging with Serilog
- Correlation IDs for request tracing
- Log levels: Debug, Information, Warning, Error

### Health Checks
- Database connectivity
- RabbitMQ connectivity
- Module-specific health checks

### Metrics
- Request duration
- Error rates
- Event processing times
- Database query performance

## 🤝 Contributing

### Adding a New Module

1. Create module projects (Domain, Application, Infrastructure, Contracts)
2. Implement domain entities and value objects
3. Create commands, queries, and handlers
4. Configure EF Core with separate schema
5. Add module registration in DependencyInjection.cs
6. Register module in API's Program.cs
7. Create migrations
8. Add tests
9. Document integration events

### Code Review Checklist

- [ ] Module has no direct references to other modules
- [ ] All cross-module communication uses events
- [ ] Validators are implemented for all commands
- [ ] Unit tests cover business logic
- [ ] Integration tests cover API endpoints
- [ ] Migrations are created and tested
- [ ] Documentation is updated

## 📖 Learning Resources

### Patterns & Practices
- Domain-Driven Design (DDD)
- CQRS (Command Query Responsibility Segregation)
- Event-Driven Architecture
- Clean Architecture
- Repository Pattern
- Unit of Work Pattern

### References
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [MediatR](https://github.com/jbogard/MediatR)
- [MassTransit](https://masstransit-project.com/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [RabbitMQ](https://www.rabbitmq.com/documentation.html)

## 📝 License

This project is a template for building modular monoliths. Use it as a starting point for your own projects.

## 🎯 Next Steps

1. ✅ Review [ARCHITECTURE_PLAN.md](ARCHITECTURE_PLAN.md)
2. ✅ Read [IMPLEMENTATION_ROADMAP.md](IMPLEMENTATION_ROADMAP.md)
3. ✅ Setup development environment (Docker, SQL Server, RabbitMQ)
4. ⬜ Follow Phase 1: Create solution structure
5. ⬜ Follow Phase 2: Implement first module (Customers)
6. ⬜ Follow Phase 3: Add additional modules (Orders, Catalog)
7. ⬜ Follow Phase 4: Implement cross-module communication
8. ⬜ Add tests and documentation
9. ⬜ Deploy and monitor

---

**Ready to build?** Start with the [Implementation Roadmap](IMPLEMENTATION_ROADMAP.md)! 🚀
