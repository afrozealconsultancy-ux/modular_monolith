# Multi-Tenant Architecture

## Overview

This modular monolith is designed as a **multi-tenant SaaS application** with:
- **Row-level tenant isolation** using `TenantId` in all tenant-scoped tables
- **Dual Keycloak realm strategy** for staff and tenant users
- **Fine-grained permission system** with role-based access control (RBAC)
- **Flexible role and permission management** per tenant

## Multi-Tenancy Strategy

### Tenant Isolation: Row-Level with TenantId

**Approach:** Every tenant-scoped entity includes a `TenantId` column.

**Benefits:**
- ✅ Cost-effective (shared infrastructure)
- ✅ Easy to manage (single database)
- ✅ Fast tenant provisioning
- ✅ Scalable for thousands of tenants
- ✅ Can migrate to separate databases later if needed

**Implementation:**
```csharp
// Base class for tenant-scoped entities
public abstract class TenantAggregateRoot : AggregateRoot, IHasTenant
{
    public Guid TenantId { get; protected init; }
}

// EF Core global query filter
modelBuilder.Entity<Customer>()
    .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
```

**Database Design:**
```sql
CREATE TABLE customers (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,  -- Every table has this
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    -- Indexes
    INDEX idx_tenant_id (tenant_id),
    INDEX idx_tenant_email (tenant_id, email)
);
```

### Non-Tenant Entities

Some entities are **not tenant-scoped** (e.g., system configuration, staff users):
- Use the base `Entity` or `AggregateRoot` classes (no `TenantId`)
- Not affected by tenant query filters
- Accessible only by staff users

## Keycloak Dual Realm Strategy

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Keycloak Server                          │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────────┐  ┌──────────────────────────┐  │
│  │  Staff Realm            │  │  Tenant Realm            │  │
│  │  (modular-monolith-     │  │  (modular-monolith-      │  │
│  │   staff)                │  │   tenants)               │  │
│  ├─────────────────────────┤  ├──────────────────────────┤  │
│  │ • System Admins         │  │ • Tenant Owners          │  │
│  │ • Support Staff         │  │ • Tenant Admins          │  │
│  │ • Developers            │  │ • Tenant Members         │  │
│  │                         │  │ • Custom Roles           │  │
│  │ Roles:                  │  │                          │  │
│  │ - SystemAdmin           │  │ Roles (per tenant):      │  │
│  │ - Support               │  │ - TenantOwner            │  │
│  │ - Developer             │  │ - TenantAdmin            │  │
│  │                         │  │ - TenantMember           │  │
│  │ Permissions:            │  │ - SalesManager           │  │
│  │ - system.tenants.*      │  │ - InventoryManager       │  │
│  │ - system.users.*        │  │ - SupportAgent           │  │
│  │ - system.logs.view      │  │ - ... (custom)           │  │
│  │                         │  │                          │  │
│  │ Can:                    │  │ Permissions:             │  │
│  │ - View all tenants      │  │ - customers.*            │  │
│  │ - Impersonate users     │  │ - orders.*               │  │
│  │ - Access system logs    │  │ - catalog.*              │  │
│  │ - Manage system config  │  │ - files.*                │  │
│  └─────────────────────────┘  │ - (tenant-scoped)        │  │
│                                └──────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Staff Realm (`modular-monolith-staff`)

**Purpose:** Internal staff and administrators

**Users:**
- System administrators
- Support staff
- Developers
- DevOps engineers

**Characteristics:**
- System-wide access (not tenant-scoped)
- Can view and manage all tenants
- Can impersonate tenant users for support
- Higher security requirements (2FA mandatory)
- Audit logging for all actions

**Roles:**
- `SystemAdmin` - Full system access
- `Support` - Customer support, limited access
- `Developer` - Dev/staging environments

**Key Permissions:**
- `system.tenants.viewall` - View all tenants
- `system.tenants.manage` - Create, update, delete tenants
- `system.users.impersonate` - Impersonate tenant users
- `system.logs.view` - View system logs
- `system.settings.manage` - Manage system configuration

### Tenant Realm (`modular-monolith-tenants`)

**Purpose:** All tenant users (customers)

**Users:**
- Tenant owners (who signed up)
- Tenant administrators
- Tenant members/employees
- Custom roles defined by tenant

**Characteristics:**
- Tenant-scoped access only
- Users belong to specific tenant(s)
- Tenant-defined roles and permissions
- Self-service user management (within tenant)

**Built-in Roles:**
- `TenantOwner` - Full access to tenant (billing, delete, etc.)
- `TenantAdmin` - User management, settings (no billing)
- `TenantMember` - Basic access (customizable)

**Custom Roles (examples - defined per tenant):**
- `SalesManager` - Manage orders, customers, sales reports
- `InventoryManager` - Manage catalog, inventory
- `SupportAgent` - View customers, manage tickets
- `Viewer` - Read-only access

## Permission System

### Permission Format

**Format:** `{module}.{resource}.{action}`

**Examples:**
- `customers.read` - View customers
- `customers.write` - Create/update customers
- `orders.create` - Create orders
- `catalog.inventory.manage` - Manage inventory
- `reports.financial.view` - View financial reports
- `users.roles.manage` - Manage user roles

### Permission Hierarchy

```
Module
├── Resource
│   ├── Action (CRUD)
│   │   ├── read
│   │   ├── create
│   │   ├── update
│   │   └── delete
│   └── Sub-Resource
│       └── Actions
└── ...
```

### Built-in Permissions

See `Permissions.cs` for complete list:

**System Permissions (Staff Only):**
```csharp
system.tenants.viewall
system.tenants.manage
system.users.impersonate
system.logs.view
system.settings.manage
```

**Tenant Management:**
```csharp
tenants.view
tenants.update
tenants.settings.manage
tenants.billing.manage
tenants.delete
```

**User Management:**
```csharp
users.view
users.invite
users.update
users.delete
users.roles.manage
users.permissions.manage
```

**Business Permissions:**
```csharp
customers.{view|create|update|delete|export|import}
orders.{view|create|update|cancel|process|refund|export}
catalog.{view|create|update|delete|inventory.manage|pricing.manage}
files.{view|upload|download|delete|share}
reports.{sales|financial|inventory|customers}.view
```

### Role-Permission Mapping

**TenantOwner (Full Access):**
- All permissions within tenant
- Including billing, delete, user management

**TenantAdmin:**
- User management (invite, update, delete)
- Settings (except billing)
- All business operations
- View reports

**SalesManager (Example Custom Role):**
```csharp
Permissions:
- customers.view
- customers.create
- customers.update
- orders.view
- orders.create
- orders.update
- reports.sales.view
```

**InventoryManager (Example Custom Role):**
```csharp
Permissions:
- catalog.view
- catalog.create
- catalog.update
- catalog.inventory.manage
- reports.inventory.view
```

## Authentication Flow

### Staff Login Flow

```
1. User → /staff/login
2. Redirect to Keycloak Staff Realm
3. Keycloak authenticates (email/password + 2FA)
4. Returns JWT with:
   - Realm: staff
   - Roles: [SystemAdmin]
   - No TenantId
5. API validates JWT against staff realm
6. User has system-wide access
```

### Tenant User Login Flow

```
1. User → /login (or custom subdomain)
2. Redirect to Keycloak Tenant Realm
3. Keycloak authenticates
4. Returns JWT with:
   - Realm: tenants
   - TenantId: <guid>
   - Roles: [TenantOwner, SalesManager]
   - Permissions: [loaded from database]
5. API validates JWT against tenant realm
6. Extracts TenantId from claims
7. All queries filtered by TenantId automatically
```

### JWT Structure

**Staff Token:**
```json
{
  "sub": "staff-user-id",
  "email": "admin@company.com",
  "realm": "staff",
  "roles": ["SystemAdmin"],
  "permissions": ["system.tenants.viewall", "system.logs.view"],
  "iss": "keycloak-staff-realm"
}
```

**Tenant Token:**
```json
{
  "sub": "user-id",
  "email": "john@acmecorp.com",
  "realm": "tenants",
  "tenant_id": "tenant-uuid",
  "tenant_name": "Acme Corp",
  "roles": ["TenantOwner", "SalesManager"],
  "permissions": ["customers.view", "orders.create", ...],
  "iss": "keycloak-tenant-realm"
}
```

## Tenant Context Resolution

### Resolution Order

1. **JWT Token** (preferred) - `tenant_id` claim
2. **Custom Header** - `X-Tenant-Id: <guid>`
3. **Subdomain** - `acmecorp.app.com` → lookup tenant by identifier
4. **Staff Override** - Staff can specify tenant context

### Implementation

```csharp
public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public string? TenantName { get; private set; }
    public bool IsStaff { get; private set; }

    public async Task ResolveAsync(HttpContext httpContext)
    {
        // 1. Check if staff user
        var realm = httpContext.User.FindFirst("realm")?.Value;
        IsStaff = realm == "staff";

        if (IsStaff)
        {
            // Staff can specify tenant via header (for impersonation)
            var tenantIdHeader = httpContext.Request.Headers["X-Tenant-Id"];
            if (!string.IsNullOrEmpty(tenantIdHeader))
            {
                TenantId = Guid.Parse(tenantIdHeader);
            }
            return; // Staff doesn't need tenant for most operations
        }

        // 2. Get tenant from JWT
        var tenantIdClaim = httpContext.User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantIdClaim))
        {
            TenantId = Guid.Parse(tenantIdClaim);
            TenantName = httpContext.User.FindFirst("tenant_name")?.Value;
            return;
        }

        // 3. Get tenant from subdomain
        var host = httpContext.Request.Host.Host;
        var subdomain = ExtractSubdomain(host);
        if (!string.IsNullOrEmpty(subdomain))
        {
            var tenant = await _tenantRepository.GetByIdentifierAsync(subdomain);
            if (tenant != null)
            {
                TenantId = tenant.TenantId;
                TenantName = tenant.Name;
                return;
            }
        }

        throw new UnauthorizedException("Tenant context could not be resolved");
    }
}
```

## Permission Checking

### In Code

```csharp
// Check single permission
if (!_currentUser.HasPermission("customers.create"))
{
    throw new ForbiddenException("You don't have permission to create customers");
}

// Check multiple permissions (all required)
if (!_currentUser.HasAllPermissions("orders.view", "orders.process"))
{
    throw new ForbiddenException("Insufficient permissions");
}

// Check multiple permissions (any required)
if (!_currentUser.HasAnyPermission("orders.view", "orders.create"))
{
    throw new ForbiddenException("Insufficient permissions");
}
```

### With Attributes

```csharp
[Authorize(Policy = "RequirePermission:customers.create")]
public async Task<IActionResult> CreateCustomer(CreateCustomerCommand command)
{
    // ...
}

[Authorize(Policy = "RequireAllPermissions:orders.view,orders.process")]
public async Task<IActionResult> ProcessOrder(Guid orderId)
{
    // ...
}
```

### In Queries (Authorization Filter)

```csharp
public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedList<CustomerDto>>
{
    public async Task<PagedList<CustomerDto>> Handle(...)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Customers.View))
        {
            throw new ForbiddenException("You don't have permission to view customers");
        }

        // Query automatically filtered by TenantId
        var query = _dbContext.Customers
            .Where(c => c.Name.Contains(request.SearchTerm));

        // EF Core global filter applies: .Where(c => c.TenantId == _tenantContext.TenantId)
    }
}
```

## Tenant Onboarding Flow

### New Tenant Registration

```
1. User visits /signup
2. Fills registration form:
   - Company name
   - Email (becomes owner)
   - Password
   - Subdomain (e.g., "acmecorp")
3. System creates:
   a. Tenant record in database
   b. User in Keycloak tenant realm
   c. Assign TenantOwner role
   d. Generate tenant_id claim
4. Send verification email
5. User logs in → full access to their tenant
```

### Invite Additional Users

```
1. Tenant owner/admin → Invite user
2. Specify:
   - Email
   - Roles (TenantAdmin, SalesManager, etc.)
   - Permissions (optional, custom)
3. System:
   a. Creates user in Keycloak
   b. Assigns roles and permissions
   c. Links user to tenant
   d. Sends invitation email
4. User accepts → logs in with tenant access
```

## Data Isolation Guarantees

### EF Core Global Query Filters

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply to all entities that implement IHasTenant
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
        {
            var method = SetGlobalQueryMethod.MakeGenericMethod(entityType.ClrType);
            method.Invoke(this, new object[] { modelBuilder });
        }
    }
}

private void SetGlobalQueryForEntity<TEntity>(ModelBuilder modelBuilder)
    where TEntity : class, IHasTenant
{
    modelBuilder.Entity<TEntity>().HasQueryFilter(
        e => e.TenantId == _tenantContext.TenantId);
}
```

**Effect:** All queries automatically include `WHERE tenant_id = <current_tenant_id>`

### Preventing Cross-Tenant Access

```csharp
// ❌ WRONG - Direct ID access without tenant check
public async Task<Customer> GetCustomerAsync(Guid customerId)
{
    return await _dbContext.Customers.FindAsync(customerId);
    // Problem: Could return customer from another tenant!
}

// ✅ CORRECT - Global filter applies automatically
public async Task<Customer?> GetCustomerAsync(Guid customerId)
{
    return await _dbContext.Customers
        .FirstOrDefaultAsync(c => c.Id == customerId);
    // EF Core adds: AND c.TenantId == currentTenantId
}

// ✅ CORRECT - Explicit tenant check (defense in depth)
public async Task<Customer?> GetCustomerAsync(Guid customerId)
{
    var customer = await _dbContext.Customers
        .FirstOrDefaultAsync(c => c.Id == customerId);

    if (customer != null && customer.TenantId != _tenantContext.TenantId)
    {
        throw new ForbiddenException("Access denied");
    }

    return customer;
}
```

## Migration Path to Microservices

### Current (Monolith)
- Single deployment
- Separate PostgreSQL schemas per module
- Tenant data in shared tables with TenantId

### Future (Microservices)
- Independent services per module
- Each service has its own database
- Tenant data still uses TenantId (row-level)
- OR: Database per tenant (if needed)

**Migration Steps:**
1. Extract module to separate service
2. Copy/move database schema
3. Update Keycloak clients
4. Update API gateway routes
5. Deploy independently

**Estimate:** 2-4 weeks per module

## Security Considerations

1. **JWT Validation:**
   - Validate signature with Keycloak public key
   - Check token expiration
   - Validate issuer (correct realm)
   - Validate audience

2. **Tenant Isolation:**
   - NEVER trust client-sent TenantId
   - Always extract from validated JWT
   - Use EF Core global query filters
   - Defense in depth: explicit checks

3. **Permission Checks:**
   - Check permissions on every operation
   - Use authorization attributes
   - Cache permissions in token (refresh periodically)

4. **Audit Logging:**
   - Log all tenant data access
   - Include: user, tenant, action, timestamp
   - Store in separate audit log (non-tenant-scoped)

5. **Data Export:**
   - Tenants can export their data
   - Staff can export any tenant data (with audit)

## Summary

✅ **Multi-Tenant:** Row-level isolation with TenantId
✅ **Dual Realms:** Staff (system) + Tenants (customers)
✅ **Fine-Grained Permissions:** Module.Resource.Action format
✅ **Flexible Roles:** Built-in + custom per tenant
✅ **Automatic Filtering:** EF Core global query filters
✅ **Secure:** JWT validation, permission checks, audit logging
✅ **Scalable:** Thousands of tenants, shared infrastructure
✅ **Microservice-Ready:** Clear boundaries, easy extraction
