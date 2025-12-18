# .NET 9 Modular Monolith - Enterprise Edition

A production-ready, enterprise-grade modular monolith architecture built with .NET 9, featuring authentication, file storage, multi-channel notifications, and comprehensive failure handling. Designed for easy migration to microservices.

## 🎯 Project Goals

Build a scalable, secure, resilient application with:
- **Modular Design**: Self-contained modules with clear boundaries
- **Separate Schemas**: Each module owns its data (PostgreSQL)
- **Authentication**: Keycloak OAuth2/OIDC integration
- **File Storage**: SeaweedFS with ClamAV virus scanning
- **Notifications**: Multi-channel (Email, SMS, Push, In-App, Webhook)
- **Resilience**: Circuit breakers, retries, saga patterns
- **CQRS Pattern**: MediatR for command/query separation
- **Microservice Ready**: Easy extraction to independent services

## ⚡ Quick Start

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose

### Start Infrastructure

```bash
# Start all infrastructure services
docker-compose up -d

# Verify all services are healthy
docker-compose ps

# View logs
docker-compose logs -f
```

### Access Services

| Service | URL | Credentials |
|---------|-----|-------------|
| Keycloak | http://localhost:8080 | admin/admin |
| RabbitMQ | http://localhost:15672 | guest/guest |
| Seq Logs | http://localhost:5341 | - |
| Grafana | http://localhost:3000 | admin/admin |
| Prometheus | http://localhost:9090 | - |
| Jaeger | http://localhost:16686 | - |
| pgAdmin | http://localhost:5050 | admin@modularmonolith.com/admin |
| SeaweedFS Filer | http://localhost:8888 | - |

## 📚 Documentation

### Core Documents
1. **[Architecture Plan](ARCHITECTURE_PLAN.md)** - Complete architecture, technology stack, and design patterns
2. **[Implementation Roadmap](IMPLEMENTATION_ROADMAP.md)** - Step-by-step implementation guide (Coming Soon)
3. **[Quick Reference](QUICK_REFERENCE.md)** - Code patterns and templates (Coming Soon)
4. **[Docker Setup](docker-compose.yml)** - Infrastructure as Code

### Quick Links
- **Architecture Overview** - [View detailed architecture](#-architecture-overview)
- **Modules** - [See all modules](#-modules)
- **Technology Stack** - [View tech stack](#-technology-stack)
- **Resilience** - [Failure handling](#-resilience--failure-handling)

## 🏗️ Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                         API Gateway                               │
│         (Authentication, Rate Limiting, Logging)                  │
└────┬────────┬──────────┬──────────┬──────────┬─────────┬─────────┘
     │        │          │          │          │         │
┌────▼───┐ ┌─▼──────┐ ┌─▼────┐ ┌──▼──────┐ ┌─▼─────┐ ┌─▼──────┐
│Identity│ │Storage │ │ Notif│ │Customers│ │ Orders│ │Catalog │
│ Module │ │ Module │ │Module│ │  Module │ │ Module│ │ Module │
└────┬───┘ └─┬──────┘ └─┬────┘ └──┬──────┘ └─┬─────┘ └─┬──────┘
     │       │          │          │          │         │
     └───────┴──────────┴──────────┴──────────┴─────────┘
                             │
                  ┌──────────▼──────────┐
                  │    RabbitMQ Bus     │
                  │ (Integration Events)│
                  │    + Outbox Pattern │
                  └─────────────────────┘
                             │
┌────────────────────────────▼─────────────────────────────┐
│              PostgreSQL Database                          │
│  ┌─────────┬────────┬──────────┬──────┬──────┬────────┐  │
│  │identity │storage │notifications│customers│orders│catalog│
│  │ schema  │ schema │  schema   │schema│schema│schema │  │
│  └─────────┴────────┴───────────┴──────┴──────┴────────┘  │
└───────────────────────────────────────────────────────────┘

External Services:
├── Keycloak (Authentication)
├── SeaweedFS (File Storage)
├── ClamAV (Virus Scanning)
├── Redis (Caching)
└── Seq/Prometheus/Jaeger (Observability)
```

## 🧩 Modules

### Core Infrastructure Modules

#### 1. Identity Module
**Purpose**: Authentication and authorization with Keycloak

**Features**:
- OAuth2/OIDC authentication
- JWT token validation
- Role-based access control (RBAC)
- User context management
- Integration with Keycloak

**Key Components**:
- `ICurrentUser` - Access current user information
- `KeycloakService` - Keycloak integration
- JWT middleware

#### 2. Storage Module
**Purpose**: Distributed file storage with virus scanning

**Features**:
- SeaweedFS integration for distributed storage
- Automatic virus scanning with ClamAV
- File metadata tracking
- Background scanning service
- Support for MinIO as fallback

**Flow**:
```
Upload → SeaweedFS → Queue for Scan → ClamAV → Update Status → Event
```

**Events**:
- `FileUploadedEvent`
- `FileScannedEvent`
- `VirusDetectedEvent`

#### 3. Notifications Module
**Purpose**: Multi-channel notification delivery

**Channels**:
- **Email**: SMTP, SendGrid, AWS SES
- **SMS**: Twilio, AWS SNS
- **Push**: Firebase, OneSignal
- **In-App**: SignalR real-time notifications
- **Webhook**: Custom HTTP endpoints

**Features**:
- Template engine (Liquid, Razor)
- Priority queuing
- Retry mechanism
- Channel fallback
- Notification history
- Configurable providers

**Configuration**:
All providers are configurable via appsettings.json - switch providers without code changes!

### Business Modules

#### 4. Customers Module
- Customer registration and management
- Address management
- Customer segmentation
- Integration with Identity module

#### 5. Orders Module
- Order creation and management
- Order status tracking
- Payment integration (via Saga pattern)
- Integration with Customers and Catalog

#### 6. Catalog Module
- Product management
- Inventory tracking
- Price management
- Category management

## 🛠️ Technology Stack

### Core Framework
| Component | Technology | Version |
|-----------|------------|---------|
| Framework | .NET | 9.0 |
| Language | C# | 13 |
| API Style | Minimal APIs + Controllers | Hybrid |

### Data & Persistence
| Component | Technology | Purpose |
|-----------|------------|---------|
| Database | PostgreSQL | 16+ |
| ORM | Entity Framework Core | 9.0 |
| Query | Dapper | High-performance reads |
| Cache | Redis | 7+ |

### Messaging & Events
| Component | Technology | Purpose |
|-----------|------------|---------|
| In-Process | MediatR | Commands/Queries |
| Cross-Module | RabbitMQ + MassTransit | Integration Events |
| Reliability | Outbox Pattern | At-least-once delivery |
| Distributed TX | Saga Pattern | Compensation |

### Authentication & Security
| Component | Technology | Purpose |
|-----------|------------|---------|
| Identity Provider | Keycloak | OAuth2/OIDC |
| Virus Scanning | ClamAV | File security |
| Storage | SeaweedFS | Distributed files |

### Notifications
| Channel | Providers |
|---------|-----------|
| Email | SMTP, SendGrid, AWS SES |
| SMS | Twilio, AWS SNS |
| Push | Firebase, OneSignal |
| In-App | SignalR |
| Webhook | HTTP |

### Resilience & Reliability
| Pattern | Library | Purpose |
|---------|---------|---------|
| Retry | Polly | Transient faults |
| Circuit Breaker | Polly | Prevent cascading |
| Bulkhead | Polly | Isolation |
| Timeout | Polly | Prevent hanging |
| Outbox | Custom | Event delivery |
| Saga | MassTransit | Distributed TX |

### Observability
| Component | Technology | Purpose |
|-----------|------------|---------|
| Logging | Serilog | Structured logs |
| Log Aggregation | Seq | Log searching |
| Metrics | Prometheus | Time-series metrics |
| Dashboards | Grafana | Visualization |
| Tracing | Jaeger + OpenTelemetry | Distributed tracing |

### Validation & Documentation
| Component | Technology |
|-----------|------------|
| Validation | FluentValidation |
| API Docs | Swagger/OpenAPI |
| API UI | Scalar |

### Testing
| Component | Technology |
|-----------|------------|
| Unit Tests | xUnit |
| Integration | Testcontainers |
| Assertions | FluentAssertions |
| Test Data | Bogus |
| Architecture | NetArchTest |

## 📁 Solution Structure

```
ModularMonolith/
├── docker-compose.yml                    # Infrastructure
├── prometheus.yml                         # Metrics config
├── scripts/
│   └── init-db.sql                       # Database initialization
│
├── src/
│   ├── API/
│   │   └── ModularMonolith.API/          # API Gateway/Host
│   │
│   ├── Modules/
│   │   ├── Identity/                     # Authentication (Keycloak)
│   │   ├── Storage/                      # Files (SeaweedFS + ClamAV)
│   │   ├── Notifications/                # Multi-channel notifications
│   │   ├── Customers/                    # Business module
│   │   ├── Orders/                       # Business module
│   │   └── Catalog/                      # Business module
│   │
│   └── Shared/
│       ├── Abstractions/                 # Interfaces
│       └── Infrastructure/               # Implementations
│           ├── Auth/
│           ├── EventBus/
│           ├── Resilience/               # Polly, Saga, Outbox
│           ├── Behaviors/
│           └── Observability/
│
└── tests/
    ├── Identity.Tests/
    ├── Storage.Tests/
    ├── Notifications.Tests/
    ├── Customers.Tests/
    ├── Orders.Tests/
    ├── ArchitectureTests/                # Enforce rules
    └── IntegrationTests/
```

## 🛡️ Resilience & Failure Handling

### Resilience Layers

```
┌─────────────────────────────────────────────┐
│  1. Timeout         → Prevent hanging       │
│  2. Retry           → Handle transient      │
│  3. Circuit Breaker → Prevent cascading     │
│  4. Bulkhead        → Isolate failures      │
│  5. Fallback        → Graceful degradation  │
│  6. Outbox Pattern  → Reliable events       │
│  7. Saga Pattern    → Distributed TX        │
└─────────────────────────────────────────────┘
```

### When a Module Fails

1. **Isolation**: Circuit breaker opens, preventing cascade
2. **Retry**: Automatic retry with exponential backoff
3. **Fallback**: Degraded functionality if available
4. **Compensation**: Saga pattern rolls back distributed transactions
5. **Alerting**: Logs, metrics, and alerts triggered
6. **Recovery**: Events queued in outbox for retry

### Example: Order Creation Failure

```
┌─────────────────────────────────────────┐
│ Saga: CreateOrder                       │
├─────────────────────────────────────────┤
│ 1. Validate Customer     ✓              │
│ 2. Reserve Products      ✓              │
│ 3. Create Order          ✓              │
│ 4. Process Payment       ✗ (Failed)     │
│                                          │
│ Compensation Started:                   │
│ 4. Cancel Payment        ✓              │
│ 3. Delete Order          ✓              │
│ 2. Release Products      ✓              │
│                                          │
│ Status: Compensated                     │
└─────────────────────────────────────────┘
```

## 🎨 Design Principles

### 1. Module Independence
- Each module is self-contained
- No direct project references between modules
- Communication only via integration events
- Can be extracted to microservice independently

### 2. Database Per Module (Schema Isolation)
- PostgreSQL schemas: `identity`, `storage`, `notifications`, `customers`, `orders`, `catalog`
- Each module owns its data
- No cross-schema queries
- Easy to move to separate databases

### 3. CQRS Pattern
- **Commands**: Write operations that change state
- **Queries**: Read operations with optimized DTOs
- Separation allows different optimization strategies

### 4. Event-Driven Architecture
- **Domain Events**: Internal to module, same transaction
- **Integration Events**: Cross-module, async via RabbitMQ
- **Outbox Pattern**: Guaranteed at-least-once delivery

### 5. Clean Architecture
Each module follows:
```
Domain (Core) → Application (Use Cases) → Infrastructure (Technical)
       ↑                                        ↓
       └────────────────────────────────────────┘
```

### 6. Resilience First
- Every external call protected by Polly policies
- Circuit breakers prevent cascading failures
- Saga pattern for distributed transactions
- Health checks for all dependencies

### 7. Configuration Over Code
- All providers configurable via appsettings.json
- Switch email provider without code changes
- Enable/disable modules via configuration
- Environment-specific settings

## 🚀 Development Workflow

### Phase 1: Foundation
1. ✅ Create solution structure
2. ✅ Setup infrastructure (Docker Compose)
3. ⬜ Setup shared abstractions
4. ⬜ Setup shared infrastructure
5. ⬜ Implement resilience patterns

### Phase 2: Core Modules
1. ⬜ Identity Module (Keycloak)
2. ⬜ Storage Module (SeaweedFS + ClamAV)
3. ⬜ Notifications Module (Multi-channel)

### Phase 3: Business Modules
1. ⬜ Customers Module
2. ⬜ Orders Module
3. ⬜ Catalog Module
4. ⬜ Cross-module integration

### Phase 4: Testing & Hardening
1. ⬜ Unit tests (all modules)
2. ⬜ Integration tests (Testcontainers)
3. ⬜ Architecture tests
4. ⬜ Performance tests
5. ⬜ Security tests
6. ⬜ Chaos engineering

## 🧪 Testing Strategy

### Unit Tests
- Domain logic
- Command/Query handlers
- Validators
- Business rules

### Integration Tests
- API endpoints
- Database operations
- Event publishing/consuming
- External service integrations

### Architecture Tests
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

### Testcontainers
```csharp
var postgres = new PostgreSqlBuilder().Build();
var rabbitmq = new RabbitMqBuilder().Build();
var redis = new RedisBuilder().Build();

await Task.WhenAll(
    postgres.StartAsync(),
    rabbitmq.StartAsync(),
    redis.StartAsync());
```

## 🔐 Security

### Authentication
- Keycloak OAuth2/OIDC
- JWT bearer tokens
- Role-based access control
- Refresh token rotation

### File Security
- All uploads scanned by ClamAV
- Quarantine infected files
- Metadata validation
- Size limits

### API Security
- Rate limiting
- CORS configuration
- HTTPS enforcement
- Input validation (FluentValidation)

### Data Security
- Encrypted connections
- Secrets in environment variables
- Audit logging
- Soft deletes

## 📊 Monitoring & Observability

### Logging
- Structured logging with Serilog
- Log aggregation in Seq
- Correlation IDs for request tracing
- Log levels: Debug, Information, Warning, Error

### Metrics
- Application metrics (Prometheus)
- Business metrics
- Infrastructure metrics
- Custom metrics per module

### Tracing
- Distributed tracing with Jaeger
- OpenTelemetry instrumentation
- Span correlation
- Performance insights

### Dashboards
- Grafana dashboards
- Pre-built panels for:
  - Request rates
  - Error rates
  - Response times
  - Module health

### Health Checks
```bash
# Check overall health
curl http://localhost:5000/health

# Checks include:
# - PostgreSQL connection
# - RabbitMQ connection
# - Redis connection
# - Keycloak availability
# - SeaweedFS availability
# - ClamAV availability
# - Module-specific health
```

## 🔄 Migration to Microservices

### Current: Modular Monolith
- Single deployment
- Separate schemas
- RabbitMQ for events
- Easy to develop and test

### Future: Microservices
- Independent deployments
- Separate databases
- Service mesh (optional)
- Independent scaling

### Migration Steps (Per Module)
1. ✅ Module has no direct dependencies
2. ✅ All communication via events
3. Create new microservice solution
4. Copy module projects
5. Add standalone API host
6. Migrate database schema to new DB
7. Update event bus configuration
8. Deploy independently
9. Update API gateway routes

**Estimate**: 2-4 weeks per module

## 📝 Configuration Example

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Database=modular_monolith;Username=postgres;Password=postgres"
  },
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/modular-monolith",
    "Audience": "modular-monolith-api",
    "ClientId": "modular-monolith-client"
  },
  "Notifications": {
    "Email": {
      "Provider": "SendGrid",
      "SendGrid": {
        "ApiKey": "SG.xxxxx",
        "FromEmail": "noreply@example.com"
      }
    },
    "Sms": {
      "Provider": "Twilio",
      "Twilio": {
        "AccountSid": "ACxxxxx",
        "AuthToken": "xxxxx",
        "FromNumber": "+1234567890"
      }
    }
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

## 🤝 Best Practices

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

### Resilience
✅ **DO**
- Wrap external calls with Polly policies
- Use circuit breakers for external services
- Implement saga pattern for distributed transactions
- Use outbox pattern for reliable events

❌ **DON'T**
- Ignore transient failures
- Skip retry logic
- Forget compensation logic in sagas
- Publish events directly (bypass outbox)

### Testing
✅ **DO**
- Write unit tests for domain logic
- Use Testcontainers for integration tests
- Test failure scenarios
- Verify architecture rules with NetArchTest

❌ **DON'T**
- Mock everything (use real dependencies when possible)
- Skip integration tests
- Ignore edge cases
- Test implementation details

## 📖 Resources

### Documentation
- [.NET 9 Documentation](https://docs.microsoft.com/dotnet/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [SeaweedFS Documentation](https://github.com/seaweedfs/seaweedfs/wiki)

### Libraries
- [MediatR](https://github.com/jbogard/MediatR)
- [MassTransit](https://masstransit-project.com/)
- [Polly](https://github.com/App-vNext/Polly)
- [FluentValidation](https://fluentvalidation.net/)

### Patterns
- Domain-Driven Design (DDD)
- CQRS (Command Query Responsibility Segregation)
- Event-Driven Architecture
- Saga Pattern
- Outbox Pattern

## 🎯 Next Steps

1. ✅ Review architecture plan
2. ✅ Setup development infrastructure
3. ⬜ Read [ARCHITECTURE_PLAN.md](ARCHITECTURE_PLAN.md) in detail
4. ⬜ Start with Phase 1 (Foundation)
5. ⬜ Implement shared infrastructure
6. ⬜ Build first module (Identity)
7. ⬜ Add tests
8. ⬜ Deploy and monitor

---

**Ready to build?** Start by reading the [Architecture Plan](ARCHITECTURE_PLAN.md)! 🚀

**Questions?** Check the documentation or open an issue.

**Contributions welcome!** See contributing guidelines (coming soon).
