# Bursa City Service Management System (BCSMS)

A municipal citizen service and request management platform for Bursa Metropolitan Municipality.

## Tech Stack

| Layer     | Technology                    |
|-----------|-------------------------------|
| Backend   | ASP.NET Core Web API (.NET 8) |
| Backend   | Java Spring Boot (planned)    |
| Frontend  | React + TypeScript (planned)  |
| Database  | TBD                           |

## Repository Structure

```
bursa-city-service-management/
├── backend/
│   ├── dotnet/          # ASP.NET Core Web API (Clean Architecture)
│   └── java/            # Spring Boot API (planned)
├── frontend/
│   └── web/             # React + TypeScript (planned)
├── docs/                # Architecture, database, and API documentation
├── docker/              # Dockerfiles
└── docker-compose.yml   # Multi-service orchestration
```

## Architecture

The .NET backend follows **Clean Architecture** principles:

- **BCSMS.Domain** — Domain entities, value objects, and business rules (no external dependencies)
- **BCSMS.Application** — Use cases, DTOs, interfaces, and application services
- **BCSMS.Infrastructure** — Data access, external service integrations, and framework concerns
- **BCSMS.API** — HTTP controllers, middleware, and API configuration (Composition Root)

### Dependency Direction

```
API → Application ← Infrastructure
            ↓
          Domain
```

Domain has zero dependencies on other layers.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build

```bash
cd backend/dotnet
dotnet restore BCSMS.sln
dotnet build BCSMS.sln
```

### Run

```bash
cd backend/dotnet/src/BCSMS.API
dotnet run
```

The API will be available at `http://localhost:5159` with Swagger UI at `/swagger`.

### Test

```bash
cd backend/dotnet
dotnet test BCSMS.sln
```

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
