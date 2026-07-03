# Avility

Avility is an inclusive recruitment platform connecting job seekers with
disabilities to companies committed to inclusive hiring. This repository
contains the backend API, built as a Clean Architecture solution in
ASP.NET Core.

## Tech stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- Clean Architecture + CQRS via MediatR
- FluentValidation
- ASP.NET Identity + JWT authentication
- Serilog
- Swagger / OpenAPI (Swashbuckle)

## Solution structure

```
Avility.sln
├── src/
│   ├── Avility.Domain          # Entities, enums, value objects. No dependencies.
│   ├── Avility.Application     # CQRS handlers, DTOs, validators. Depends on Domain only.
│   ├── Avility.Infrastructure  # EF Core, Identity, external services.
│   └── Avility.API             # Controllers, middleware, composition root.
├── tests/
│   ├── Avility.Domain.UnitTests
│   ├── Avility.Application.UnitTests
│   └── Avility.API.IntegrationTests
└── docs/
    └── adr/                    # Architecture Decision Records
```

Dependency direction is strictly inward: `API → Application/Infrastructure →
Domain`. `Domain` has zero external dependencies, including no reference to
ASP.NET Identity — see `docs/adr/`.

## Getting started

```bash
dotnet restore
dotnet build
dotnet test
```

Configure your local SQL Server connection string via user secrets rather
than committing it:

```bash
cd src/Avility.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>"
```

## Architecture Decision Records

See [`docs/adr/`](docs/adr/) for the reasoning behind key architectural
choices (e.g. why there's no generic repository layer, why AutoMapper isn't
used by default).
