# Database Migration Instructions

## Prerequisites

1. Ensure NuGet package restore is working:
   ```bash
   dotnet restore
   ```

2. Install EF Core tools (if not already installed):
   ```bash
   dotnet tool restore
   ```

## Creating the Initial Migration

Run the following command from the project root to create the initial database migration for the Customers module:

```bash
dotnet ef migrations add InitialCreate \
  --project src/Modules/Customers/ModularMonolith.Modules.Customers.Infrastructure \
  --startup-project src/API/ModularMonolith.API \
  --context CustomersDbContext \
  --output-dir Persistence/Migrations
```

This will generate migration files in the `Persistence/Migrations` directory within the Customers Infrastructure project.

## Applying Migrations

### Automatic (Recommended for Development)

The `CustomersModule.InitializeAsync()` method automatically runs migrations on startup:

```csharp
public async Task InitializeAsync(IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
    await dbContext.Database.MigrateAsync();  // Auto-apply migrations
}
```

### Manual (Production)

For production environments, apply migrations manually:

```bash
dotnet ef database update \
  --project src/Modules/Customers/ModularMonolith.Modules.Customers.Infrastructure \
  --startup-project src/API/ModularMonolith.API \
  --context CustomersDbContext
```

## Adding Future Migrations

When you modify the Customer domain model, create a new migration:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Customers/ModularMonolith.Modules.Customers.Infrastructure \
  --startup-project src/API/ModularMonolith.API \
  --context CustomersDbContext \
  --output-dir Persistence/Migrations
```

## Database Schema

The Customers module uses the `customers` schema in PostgreSQL with the following tables:

- `customers.customers` - Main customer table
- `customers.customer_addresses` - Customer addresses (owned entity)

All tables include `tenant_id` for multi-tenant isolation with automatic query filtering.

## Network Issues Note

If you encounter network connectivity issues during package restore or migration creation, ensure:
- NuGet package sources are accessible
- Proxy settings are configured correctly if required
- The `dotnet-ef` tool is installed (see `.config/dotnet-tools.json`)
