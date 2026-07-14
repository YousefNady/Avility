<div align="center">

# Avility

**An inclusive recruitment platform connecting job seekers with disabilities to companies committed to accessible, inclusive hiring.**

This repository contains the backend API — a Clean Architecture / CQRS solution built on ASP.NET Core.

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20%2B%20CQRS-2ea44f)]()
[![Tests](https://img.shields.io/badge/tests-xUnit-orange)]()
[![Deploy](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-2088FF?logo=githubactions&logoColor=white)](.github/workflows/deploy.yml)

</div>

---

Job seekers can disclose accessibility needs on their profile; companies can declare which accommodations a role supports; the platform uses that shared, structured data to rank and filter opportunities — not as an afterthought bolted onto a generic job board, but as a first-class part of the domain model.

> 📘 **Want to test the API end-to-end?**
> A complete, step-by-step Swagger testing guide is available under [`docs/api/API_TESTING_GUIDE.md`](docs/api/API_TESTING_GUIDE.md).

---

## Table of Contents

- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Features](#features)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Testing](#testing)
- [Real-Time Messaging](#real-time-messaging)
- [Deployment](#deployment)
- [Project Structure](#project-structure)
- [API Documentation](#api-documentation)
- [Roadmap](#roadmap)

---

## Tech Stack

| Concern | Technology |
|---|---|
| **Runtime** | .NET 10 / ASP.NET Core Web API |
| **Architecture** | Clean Architecture, CQRS via MediatR |
| **Persistence** | Entity Framework Core, SQLite |
| **Validation** | FluentValidation |
| **Auth** | ASP.NET Identity, JWT (access + rotating refresh tokens) |
| **Real-time** | SignalR |
| **Logging** | Serilog (console + rolling file, structured, correlation-ID enriched) |
| **API Docs** | Swagger / OpenAPI (Swashbuckle), Development only |
| **Testing** | xUnit, FluentValidation.TestHelper, WebApplicationFactory |
| **CI/CD** | GitHub Actions → MonsterASP.NET (WebDeploy) |

---

## Architecture

Clean Architecture, four layers, strict inward dependency:

```
API → Infrastructure / Application → Domain
```

| Layer | Responsibility |
|---|---|
| **Domain** | Entities, value objects, enums, domain exceptions. Zero external dependencies, including zero reference to ASP.NET Identity. |
| **Application** | CQRS command/query handlers, DTOs, validators, pipeline behaviors (logging → validation → performance). Depends on Domain only. |
| **Infrastructure** | EF Core, ASP.NET Identity, JWT issuance, SMTP, local file storage, background jobs. Depends on Application + Domain. |
| **API** | Controllers, middleware, SignalR hub, composition root. |

**Key conventions:**

- 🚫 No generic repository — `IApplicationDbContext` (EF Core `DbSet<T>`) is the abstraction; EF Core already *is* the Repository + Unit of Work.
- ✋ Manual DTO mapping via `ToDto()` extension methods — no AutoMapper.
- 📁 Feature-based folder structure (`JobPostings/`, `JobApplications/`, `Messages/`, etc.), not technical folders.
- 🔤 Enums persisted as strings, always.
- 🔐 Ownership/authorization checks happen in Application handlers (or a shared `IJobApplicationAccessGuard` for multi-participant resources), not scattered role attributes alone.
- 📦 A single response envelope (`ApiResponse<T>`) and pagination shape (`PagedResult<T>`) across every endpoint.

> See [`docs/adr/`](docs/adr/) for the reasoning behind specific decisions (why no generic repository, why manual mapping, why CQRS without event sourcing, etc.).

---

## Features

<details open>
<summary><strong>Accounts & Auth</strong></summary>

- Registration/login/logout with JWT access tokens + rotating, hashed refresh tokens
- Forgot/reset password via email
- Policy-lite role authorization (JobSeeker / Company / Admin)
- Rate-limited auth endpoints, plus a baseline global rate limiter across the whole API

</details>

<details open>
<summary><strong>Job Seekers</strong></summary>

- Profile management, resume upload/download
- Optional, self-disclosed accessibility categories + accommodation notes
- Personalized job recommendations, ranked by overlap with disclosed accessibility needs (deterministic, explainable — not a black-box model)

</details>

<details open>
<summary><strong>Companies</strong></summary>

- Profile management, logo upload/download
- Admin verification workflow (Pending → Verified/Rejected — only Verified companies can publish jobs)
- Declared accommodation support per job posting (topic-based categories)

</details>

<details open>
<summary><strong>Job Postings & Applications</strong></summary>

- Full posting lifecycle (Draft → Published → Closed)
- Public search with accessibility-category filtering
- Full application lifecycle (Applied → UnderReview → Accepted/Rejected, or Withdrawn)
- Automatic email notification to the applicant on accept/reject

</details>

<details open>
<summary><strong>Messaging</strong></summary>

- Per-application message thread between the JobSeeker and the Company
- Real-time delivery via SignalR, with REST endpoints as history/fallback (see [Real-Time Messaging](#-real-time-messaging))

</details>

<details open>
<summary><strong>Resource Center</strong></summary>

- Admin-managed learning resources, categorized by topic (career advice, interview prep, workplace accommodations, etc.)

</details>

<details open>
<summary><strong>Admin</strong></summary>

- Platform statistics dashboard (users, companies by verification status, postings by lifecycle stage, applications by outcome)
- User activation/deactivation, company verification, force-closing postings

</details>

<details open>
<summary><strong>Platform / Ops</strong></summary>

- Background email delivery (in-process queue, no external broker)
- Scheduled expired refresh-token cleanup
- Correlation ID on every request/response, threaded through all logs
- Liveness (`/health/live`) and readiness (`/health/ready`) checks, plus a combined `/health`
- Security headers, CORS, response compression, unified error envelope (including model-binding failures)
- Fail-fast startup guard against a misconfigured/default JWT secret
- Bootstraps an initial Admin account from configuration — no manual DB edits needed on a fresh deployment

</details>

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Setup

```bash
git clone <this-repo>
cd Avility

dotnet restore
dotnet build
```

> ⚠️ **Required:** set the JWT secret before running — the app **will not start** without a real one (see [Configuration](#-configuration)):

```bash
cd src/Avility.API
dotnet user-secrets set "Jwt:Secret" "a-real-random-value-at-least-32-characters-long"
```

Run:

```bash
dotnet run --project src/Avility.API
```

Swagger UI is available at `/swagger` in the Development environment.

The database is SQLite — `ConnectionStrings:DefaultConnection` is a local file path (`avility.dev.db` in Development), not a credential. Migrations apply automatically or via:

```bash
dotnet ef database update --project src/Avility.Infrastructure --startup-project src/Avility.API
```

---

## Configuration

All configuration follows standard ASP.NET Core precedence (`appsettings.json` → `appsettings.{Environment}.json` → user secrets (dev) → environment variables). **Secrets are never committed** — every sensitive key ships as an empty placeholder in `appsettings.json`.

| Section | Keys | Notes |
|---|---|---|
| `Jwt` | `Secret`, `Issuer`, `Audience`, token expirations | `Secret` must be 32+ chars and not the placeholder — enforced at startup |
| `Cors` | `AllowedOrigins` (array) | Empty by default (fails closed); required for any browser-based frontend |
| `Smtp` | `Host`, `Port`, `Username`, `Password`, `FromAddress`, `FromName` | If `Host` is unset, email sending no-ops with a warning rather than crashing |
| `Seed` | `AdminEmail`, `AdminPassword` | Bootstraps one Admin account on startup if both are set and no Admin exists yet; safe to leave unset |
| `FileStorage` | `LocalRootPath` | Local disk storage for resumes/logos |
| `BackgroundJobs` | `RefreshTokenCleanupIntervalHours` | Defaults to 24 |

| Environment | Where secrets live |
|---|---|
| 💻 **Local development** | `dotnet user-secrets` for anything above marked as a secret |
| ☁️ **Production (MonsterASP.NET)** | Hosting Control Panel's environment variables (`Section__Key` syntax, e.g. `Jwt__Secret`, `Seed__AdminEmail`) |

---

## Testing

```bash
dotnet test
```

Three test projects, matching the CQRS layering:

| Project | Scope |
|---|---|
| **`Avility.Domain.UnitTests`** | Entity invariants and state machines, no mocking (Domain has zero dependencies). |
| **`Avility.Application.UnitTests`** | FluentValidation validator tests. |
| **`Avility.API.IntegrationTests`** | Full black-box HTTP tests via `WebApplicationFactory`, EF Core InMemory provider, real registration/login/role-promotion flows. Handler-level behavior is verified here, not through isolated unit tests. |

Test doubles (`FakeEmailSender`, `FakeMessageNotifier`) replace real SMTP/SignalR delivery in the test host so no external service is ever contacted during a test run.

---

## Real-Time Messaging

Each `JobApplication` has a message thread between its JobSeeker and Company. Two ways to interact with it, both backed by the same validation/authorization/persistence path:

**REST** (history + fallback)
```
GET  /api/v1/jobapplications/{id}/messages
POST /api/v1/jobapplications/{id}/messages
```

**SignalR** (live delivery) — connect to `/hubs/messages` with a JWT (via `Authorization` header, or `?access_token=` query string — required for browser WebSocket connections), then:

- `JoinThread(jobApplicationId)` — verifies you're a participant, joins the thread's group
- `SendMessage(jobApplicationId, body)` — delegates to the same command REST uses
- `LeaveThread(jobApplicationId)`
- Listen for the `MessageReceived` event

> Only the two actual participants (the applicant and the hiring company) can join a thread or send/receive on it.

---

## Deployment

Hosted on **MonsterASP.NET** (free ASP.NET Core hosting, IIS-based, free Let's Encrypt HTTPS, no Docker/reverse proxy needed). Deployment is fully automated via GitHub Actions on every push to `development`:

1. Restore, build, and run the full test suite (`windows-latest` runner)
2. Publish `Avility.API` (`win-x86`, framework-dependent, per the host's requirement)
3. Deploy via WebDeploy

See [`.github/workflows/deploy.yml`](.github/workflows/deploy.yml). Deployment credentials are stored as GitHub repository secrets, never in source.

---

## Project Structure

```
Avility.sln
├── src/
│   ├── Avility.Domain          # Entities, value objects, enums. Zero dependencies.
│   ├── Avility.Application     # CQRS handlers, DTOs, validators.
│   ├── Avility.Infrastructure  # EF Core, Identity, JWT, SMTP, file storage, background jobs.
│   └── Avility.API             # Controllers, middleware, SignalR hub, composition root.
├── tests/
│   ├── Avility.Domain.UnitTests
│   ├── Avility.Application.UnitTests
│   └── Avility.API.IntegrationTests
├── docs/
│   ├── adr/                    # Architecture Decision Records
│   └── api/
│       └── API_TESTING_GUIDE.md
└── .github/workflows/           # CI/CD
```

---

## API Documentation

Swagger UI (`/swagger`) is available in the Development environment and reflects the live API surface — versioned from `/api/v1/`, with a consistent `ApiResponse<T>` envelope and `PagedResult<T>` pagination shape across every list endpoint.

For a full, ordered, endpoint-by-endpoint walkthrough with expected status codes, see [`docs/api/API_TESTING_GUIDE.md`](docs/api/API_TESTING_GUIDE.md).

---

## Roadmap

Deferred, in rough priority order:

- [ ] Real payment/subscription tier billing (currently no paid-service dependency in this project by design)
- [ ] Localization / multi-language support
- [ ] Cloud file storage (currently local disk)
- [ ] Push/SMS notifications (email notifications are already live)

---

<div align="center">

Contributions and issue reports welcome.

</div>
