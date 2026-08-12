# EduTrack — Student Academic Management System (Backend API)

CSW 306 — Backend Development · Group project (6 members).
ASP.NET Core Web API · EF Core (Code-First) · SQL Server · Identity + JWT · xUnit.

## Solution structure (Clean Architecture)

| Project | Responsibility |
|---|---|
| `EduTrack.Api` | Controllers, middleware, Swagger, DI, auth config |
| `EduTrack.Application` | Services (business rules), DTOs, validators, mapping, `ApiResponse` |
| `EduTrack.Domain` | Entities + enums (no dependencies) |
| `EduTrack.Infrastructure` | EF Core `AppDbContext`, `AppUser` (Identity), repositories, migrations |
| `EduTrack.Tests` | Unit + integration tests |

Dependency direction: **Api → Application → Domain**, and **Infrastructure → Application/Domain**.
Response envelope for every endpoint: `{ success, data, message, statusCode }`.

## Prerequisites

- **.NET 8 SDK (LTS)** — install: `winget install Microsoft.DotNet.SDK.8`
- SQL Server 2022 / LocalDB + SSMS
- Visual Studio 2022 (or `dotnet` CLI)

> `global.json` pins this repo to the .NET 8 SDK, so `dotnet` commands here always
> use 8.x even if a newer SDK (e.g. .NET 10) is also installed on your machine.

## Getting started

```bash
git clone <repo-url>
cd EduTrack
dotnet restore
dotnet build
dotnet run --project EduTrack.Api      # open the Swagger URL it prints, try GET /api/health
```

### Set the JWT secret (do NOT commit it)

```bash
cd EduTrack.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<a-long-random-secret>"
```

### Database (once the Lead wires up the DbContext in Program.cs)

```bash
dotnet ef migrations add InitialCreate --project EduTrack.Infrastructure --startup-project EduTrack.Api
dotnet ef database update              --project EduTrack.Infrastructure --startup-project EduTrack.Api
```

## Git workflow

- `main` = stable · `develop` = integration. Never push straight to either.
- One task per branch: `feature/<module>-<task>` (e.g. `feature/enrollment-capacity`).
- Branch → commit → push → **Pull Request into `develop`** → 1 review → merge.
- Run `dotnet test` before opening a PR.

## Module ownership

| Member | Module |
|---|---|
| Nguyen Sy Hoang (Lead) | Project Setup & Auth |
| Duy Tran | User & Profile Management |
| Ngoc Hai | Student & Teacher Management |
| The Phuong | Course, Class & Enrollment |
| Nguyen Hoang | Grades & Security |
| Thành Nguyên | Documentation, Testing & DevOps |

See [`docs/EduTrack_Project_Document.docx`](docs/EduTrack_Project_Document.docx) for the full plan (schema, API contracts, per-member sheets).

## .NET version

Targets **.NET 8 (LTS)**, per the team's decision. All 5 projects build and test clean on
`net8.0`. Everyone should install the **.NET 8 SDK** (Visual Studio 2022 17.8+ includes it,
or run `winget install Microsoft.DotNet.SDK.8`).
