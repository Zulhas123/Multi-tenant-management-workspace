# Multi-tenant Management Workspace

A production-oriented **multi-tenant project management** backend built with **ASP.NET Core Web API** and **Clean Architecture**, with a **minimal, professional operations console UI** served directly from the API (plain HTML/CSS/JS).

This repository is designed to be:

- **Clean and testable** (clear boundaries, dependency inversion, unit tests)
- **Lightweight** (only essential services/libraries; generated artifacts are not committed)
- **Usable by default** (Swagger + built-in UI for demo/manual testing)
- **Multi-tenant by design** (tenant scoping enforced across layers)

---

## Architecture (Clean Architecture)

The solution is intentionally structured around strict separation of concerns:

- **Domain (`src/MultiTenantWorkspace.Domain`)**
  - Entities, enums, and core business rules
  - No external dependencies
- **Application (`src/MultiTenantWorkspace.Application`)**
  - Use cases, DTOs, and interfaces (ports)
  - Cross-cutting abstractions (current user, date/time, unit of work)
- **Infrastructure (`src/MultiTenantWorkspace.Infrastructure`)**
  - SQL Server persistence (EF Core), repositories, caching, identity utilities
  - Implements Application interfaces
- **Web host (`src/MultiTenantWorkspace.Web`)**
  - HTTP endpoints (controllers), middleware, authentication wiring, Swagger
  - Serves the static operations console from `wwwroot/`

**Dependency direction**

`Web` → `Application` → `Domain`  
`Infrastructure` → `Application` → `Domain`

The Domain never references other layers.

---

## Built-in UI (Minimal, Professional Console)

The API serves a minimal operations console intended for **clarity, usability, and consistency**:

- Clean top bar + sidebar navigation
- Dashboard + workflow-driven panels (auth, workspace, projects/tasks)
- Responsive layout and consistent component patterns (cards, panels, tables)
- Hash navigation (e.g. `#overview`) to ensure sections load reliably

UI source:

- `src/MultiTenantWorkspace.Web/wwwroot/index.html`
- `src/MultiTenantWorkspace.Web/wwwroot/css/styles.css`
- `src/MultiTenantWorkspace.Web/wwwroot/js/app.js`

---

## Repository Structure

```text
MultiTenantWorkspace.sln
src/
  MultiTenantWorkspace.Domain/
  MultiTenantWorkspace.Application/
  MultiTenantWorkspace.Infrastructure/
  MultiTenantWorkspace.Shared/
  MultiTenantWorkspace.Web/
  ProjectManagementSaaS.Api/        # Legacy/alternate host (not in the solution)
tests/
  MultiTenantWorkspace.Application.Tests/
  MultiTenantWorkspace.Infrastructure.Tests/
```

---

## Key Features

- Multi-tenant organization registration (tenant + bootstrap user)
- JWT authentication + refresh token flow
- Role-based authorization (`Admin`, `Manager`, `Member`)
- Tenant isolation (organization-scoped access)
- Projects
  - Create/list/update/delete
- Tasks
  - Create/update/delete
  - Status updates
  - Comments
- Activity logging for tenant operations
- API versioning (`/api/v1/...`)
- Swagger/OpenAPI
- Built-in operations console UI for manual testing/demo

---

## Application Flow (End-to-end)

1. **Register an organization** → creates tenant + bootstrap admin
2. **Login** → returns JWT + refresh token
3. **Use JWT** in `Authorization: Bearer <token>`
4. **Load workspace** (organization details/users/activity logs)
5. **Create projects**
6. **Create tasks under projects**
7. **Update task status / add comments** → activity logs are recorded

The built-in UI mirrors this flow to validate the API quickly without external tools.

---

## API Surface (v1)

### Authentication

- `POST /api/v1/auth/register-organization`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`

### Workspace (Organization)

- `GET /api/v1/organization`
- `GET /api/v1/organization/users`
- `GET /api/v1/organization/activity-logs`

### Projects

- `GET /api/v1/projects`
- `POST /api/v1/projects`
- `PUT /api/v1/projects/{projectId}`
- `DELETE /api/v1/projects/{projectId}`

### Tasks

- `GET /api/v1/tasks/{taskId}`
- `POST /api/v1/tasks`
- `PUT /api/v1/tasks/{taskId}`
- `DELETE /api/v1/tasks/{taskId}`
- `PATCH /api/v1/tasks/{taskId}/status`
- `POST /api/v1/tasks/{taskId}/comments`

---

## Tech Stack / Dependencies

Core technologies used across the solution:

- ASP.NET Core Web API (`Microsoft.NET.Sdk.Web`)
- Entity Framework Core + SQL Server provider
- JWT Bearer authentication
- API Versioning (`Asp.Versioning.Mvc`)
- Swagger/OpenAPI (`Swashbuckle.AspNetCore`)
- Serilog (console + rolling file sink)
- BCrypt for password hashing
- In-memory caching (`Microsoft.Extensions.Caching.Memory`)

---

## Setup (Local Development)

### Prerequisites

- **.NET SDK 9.x** (projects target `net9.0`)
- **SQL Server** (SQL Express / LocalDB / full SQL Server) *or* Docker

### Configuration

The host uses `src/MultiTenantWorkspace.Web/appsettings.json` and supports environment variable overrides.

Recommended: set the connection string per machine:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=ProjectManagementSaaSDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False"
```

JWT configuration keys to override outside dev:

- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__ExpiryMinutes`

### Run (API + UI)

```powershell
dotnet run --project src/MultiTenantWorkspace.Web
```

Open:

- UI: `http://localhost:5115/` (or the port from `launchSettings.json`)
- Swagger: `http://localhost:5115/swagger`

### Database initialization & seed

On startup, the host currently:

- Ensures the database exists (`EnsureCreated()`)
- Seeds demo data if the database is empty

Entry point: `src/MultiTenantWorkspace.Web/Program.cs`

### Tests

```powershell
dotnet test MultiTenantWorkspace.sln
```

---

## Run with Docker

Start:

```powershell
docker compose up --build
```

Endpoints:

- API + UI + Swagger: `http://localhost:8080`
- SQL Server: `localhost:1433`

Stop:

```powershell
docker compose down
```

---

## Step-by-step Refactoring Roadmap (to production-ready)

The intent is to harden the system **progressively**, while preserving behavior and testability:

1. **Analyze the current structure**
   - Verify boundaries are respected and duplication is identified.
2. **Redesign the Application layer around use cases**
   - Keep request/response models minimal, centralize validation, reduce surface area.
3. **Harden tenant isolation**
   - Ensure every read/write is tenant-scoped; enforce centrally via the current-user context.
4. **Production-grade persistence**
   - Add EF Core migrations, avoid `EnsureCreated()` outside dev, make seeding environment-aware.
5. **Keep dependencies lean**
   - Remove unused packages/services; keep only what supports core workflows.
6. **Improve observability**
   - Structured logging, correlation IDs, consistent error payloads.
7. **UI polish without bloat**
   - Continue using reusable layout primitives; avoid “feature creep” in the console UI.
8. **Increase confidence with targeted tests**
   - Prioritize auth flows, tenant boundaries, project/task lifecycle.

---

## Notes

- `src/ProjectManagementSaaS.Api` exists as an alternate/legacy API host. The solution (`MultiTenantWorkspace.sln`) uses `src/MultiTenantWorkspace.Web` as the primary host.

