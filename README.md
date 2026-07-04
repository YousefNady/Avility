# Avility

Avility is an inclusive recruitment platform connecting job seekers with
disabilities to companies committed to inclusive hiring. This repository
contains the backend API, built as a Clean Architecture solution in
ASP.NET Core.

## Tech stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core (SQLite)
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

The database is SQLite, so `ConnectionStrings:DefaultConnection` in
`appsettings.json`/`appsettings.Development.json` is just a local file
path (`avility.db` / `avility.dev.db`), not a credential - unlike a SQL
Server connection string, there's nothing here that needs `dotnet
user-secrets`. If a future milestone introduces a secret-bearing setting
(e.g. a JWT signing key), that one specifically should go into user
secrets rather than appsettings.

## Architecture Decision Records

See [`docs/adr/`](docs/adr/) for the reasoning behind key architectural
choices (e.g. why there's no generic repository layer, why AutoMapper isn't
used by default).
