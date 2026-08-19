# System Architecture

The **Bursa City Service Management System (BCSMS)** is built following the principles of **Clean Architecture** (Ports & Adapters / Hexagonal Architecture). This design strictly enforces separation of concerns and dependency inversion, ensuring core business logic remains independent of UI frameworks, databases, and third-party libraries.

---

## 1. High-Level Architecture Diagram

```mermaid
graph TD
    subgraph Client ["Client Layer"]
        Browser["Web Browser (SPA Client)"]
    end

    subgraph ContainerRuntime ["Docker Compose Environment"]
        subgraph WebContainer ["Web Service (Nginx :80 -> :3000)"]
            StaticAssets["React 18 + TS Static Bundle (Vite Build)"]
            NginxProxy["Nginx Reverse Proxy (/api/ & /health)"]
        end

        subgraph ApiContainer ["API Service (.NET 8 Runtime :8080 -> :5123)"]
            subgraph PresentationLayer ["BCSMS.API (Presentation / Composition Root)"]
                Controllers["Controllers (Auth, Citizen, Manager, Employee, Reference)"]
                Middleware["ExceptionHandling & ProblemDetails Middleware"]
                HealthEndpoint["Health Check (/health)"]
            end

            subgraph ApplicationLayer ["BCSMS.Application (Use Cases & Contracts)"]
                Commands["Commands & Handlers (Create, Assign, Resolve, Close...)"]
                Queries["Queries & DTOs (GetById, GetMyRequests, GetMunicipal...)"]
                Interfaces["Interfaces (Persistence, Security, Time)"]
            end

            subgraph DomainLayer ["BCSMS.Domain (Enterprise Core)"]
                Entities["Entities (ServiceRequest, User, Department, Category...)"]
                ValueObjects["Value Objects (ContactInfo, FullName, Location)"]
                Enums["Enums (RequestStatus, Priority, UserRole)"]
                DomainRules["Domain Invariants & State Rules"]
            end

            subgraph InfrastructureLayer ["BCSMS.Infrastructure (External Concerns)"]
                DbContext["BcsmsDbContext (EF Core 8)"]
                Repositories["Repositories (ServiceRequest, User, Dept, Cat)"]
                SecurityServices["PasswordHasher (PBKDF2) & JwtTokenGenerator"]
                Seeder["DbSeeder (Deterministic Demo Seed Data)"]
            end
        end

        subgraph DbContainer ["Database Service (SQL Server 2022 :1433)"]
            SqlServer[("SQL Server (BcsmsDb)")]
            SqlVolume[("Named Volume: bcsms_sql_data")]
        end
    end

    Browser -->|"HTTP :3000 (Direct / SPA Routes)"| StaticAssets
    Browser -->|"HTTP :3000/api/* (Relative AJAX)"| NginxProxy
    NginxProxy -->|"Reverse Proxy :8080"| Controllers
    Controllers --> Commands
    Controllers --> Queries
    Commands --> Interfaces
    Queries --> Interfaces
    Commands --> Entities
    Queries --> Entities
    InfrastructureLayer -.->|"Implements"| Interfaces
    DbContext --> Repositories
    Repositories -->|"EF Core Migrations & Queries"| SqlServer
    SqlServer --- SqlVolume
```

---

## 2. Layer Responsibilities

### `BCSMS.Domain`
- **Zero External Dependencies**: Contains pure C# domain entities, value objects, domain exceptions, and enums.
- **Enterprise Invariants**: Encapsulates entity lifecycles, validation rules, and status transition constraints.
- **Key Types**: `ServiceRequest`, `User`, `Department`, `Category`, `StatusHistoryEntry`, `Comment`, `Attachment`, `FullName`, `ContactInfo`, `RequestStatus`, `Priority`, `UserRole`.

### `BCSMS.Application`
- **Use Case Orchestration**: Implements business workflows and commands (`CreateServiceRequest`, `AssignRequest`, `StartWork`, `ResolveRequest`, `CloseRequest`, etc.).
- **Boundary Abstractions**: Declares repository contracts (`IServiceRequestRepository`, `IUserRepository`, `IDepartmentRepository`, `ICategoryRepository`), time abstractions (`IClock`), and security contracts (`IPasswordHasher`, `IJwtTokenGenerator`).
- **DTOs and Mappings**: Encapsulates query models (`ServiceRequestDetailDto`, `PagedResult<T>`) without exposing raw database entities.

### `BCSMS.Infrastructure`
- **Persistence Layer**: Entity Framework Core 8 implementation (`BcsmsDbContext`), fluent entity configurations, migrations, and repository implementations.
- **Security Implementations**: PBKDF2 with HMAC-SHA512 password hashing (`PasswordHasher`) and signed HMAC-SHA256 JWT generation (`JwtTokenGenerator`).
- **Database Seeding**: Idempotent development data seeder (`DbSeeder`).

### `BCSMS.API`
- **Composition Root**: Configures ASP.NET Core dependency injection container, authentication schemes, CORS, and routing.
- **Controllers & Endpoints**: Exposes RESTful HTTP endpoints grouped by municipal role (`AuthController`, `ServiceRequestsController`, `ManagerServiceRequestsController`, `EmployeeServiceRequestsController`, `ReferenceDataController`).
- **Error Handling**: RFC 7807 `ProblemDetails` translation via `ExceptionHandlingMiddleware`.
- **Operational Endpoints**: Built-in `/health` check.

### `frontend/web` (Client Layer)
- **Framework & Language**: React 18, TypeScript, Vite.
- **UI & Layout System**: Material UI (MUI v5) tailored with municipal visual hierarchy.
- **Server State & Routing**: TanStack Query v5 for cached server synchronization and React Router v6 with `ProtectedRoute` role guards.
- **Deployment**: Static build served via Nginx with SPA fallback (`try_files`) and `/api/` reverse proxy.

---

## 3. Dependency Rule

```
BCSMS.API ───────► BCSMS.Application ◄─────── BCSMS.Infrastructure
                           │
                           ▼
                      BCSMS.Domain
```

All dependencies point inwards toward the Domain model. Neither `BCSMS.Domain` nor `BCSMS.Application` depends on EF Core, ASP.NET Core, or SQL Server.
