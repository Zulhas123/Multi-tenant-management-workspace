# ProjectManagementSaaS

Multi-tenant SaaS Project Management System built with ASP.NET Core Web API, Clean Architecture, SQL Server, Entity Framework Core, and JWT authentication.

This project demonstrates a professional backend structure with a built-in presentation layer. Organizations can register their own workspace, authenticate with JWT, create projects, create tasks, and inspect activity logs from a browser-based operations console served directly by the API.

## Application Description

The application is designed as a multi-tenant project management platform where each organization works inside its own isolated workspace. Users belong to one organization, and all project, task, and activity data is scoped to that tenant.

The backend follows Clean Architecture and is split into:

- `Domain`: core entities, enums, and business model
- `Application`: use cases, service contracts, business rules, and abstractions
- `Infrastructure`: EF Core persistence, repositories, caching, password hashing, JWT generation
- `Api`: controllers, middleware, authentication setup, Swagger, and built-in UI

## Main Features

- Multi-tenant organization registration
- JWT-based authentication
- Refresh token rotation
- Logout-based refresh token invalidation
- Role-based authorization with `Admin`, `Manager`, and `Member`
- Organization workspace isolation
- Project creation and listing
- Project editing and deletion
- Task creation, editing, and deletion
- Task status updates
- Task comments
- Activity log tracking
- Global exception handling middleware
- In-memory caching for project lists
- API versioning
- Swagger/OpenAPI support
- Built-in browser UI with top navigation and left sidebar layout
- SQL Server persistence using Entity Framework Core

## Roles and Permissions

- `Admin`
  - Register organization
  - Login
  - View organization details
  - View users
  - View activity logs
  - Create projects
  - Create tasks
  - Update task status
  - Add comments

- `Manager`
  - Login
  - View organization details
  - View users
  - View activity logs
  - Create projects
  - Create tasks
  - Update task status
  - Add comments

- `Member`
  - Login
  - View organization details
  - View activity logs
  - Update status only for tasks assigned to them
  - Add comments

## Implemented Functional Areas

### 1. Authentication

- Register a new organization and bootstrap the first admin user
- Login with email and password
- Issue JWT token with:
  - user id
  - organization id
  - email
  - role

### 2. Organization Management

- Get current organization details
- Get organization users
- Get organization activity logs

### 3. Project Management

- Create a new project
- List projects for the current organization
- Cache project list results for faster repeated access

### 4. Task Management

- Create task under a project
- Assign task to a user
- Update task status
- Add comments to tasks

### 5. Activity Logging

Activity is logged for:

- organization registration
- project creation
- task creation
- task status changes
- task comment creation

## Project Structure

```text
MultiTenantWorkspace.sln
src/
  MultiTenantWorkspace.Domain/
    Common/
    Entities/
    Enums/
    ValueObjects/
    Events/
    Interfaces/
  MultiTenantWorkspace.Application/
    Common/
    Features/
      Authentication/
        Commands/
        DTOs/
        Interfaces/
        Services/
      Workspaces/
        DTOs/
        Interfaces/
        Services/
      Projects/
        Commands/
        DTOs/
        Interfaces/
        Services/
      Tasks/
        Commands/
        DTOs/
        Interfaces/
        Services/
    DTOs/
    Interfaces/
    Validators/
  MultiTenantWorkspace.Infrastructure/
    Caching/
    Persistence/
      Configurations/
      Seed/
      Migrations/
    Repositories/
    Identity/
    Services/
    DependencyInjection/
  MultiTenantWorkspace.Web/
    Controllers/
    Views/
    ViewModels/
    Middleware/
    Filters/
    Extensions/
    wwwroot/
tests/
  MultiTenantWorkspace.Application.Tests/
  MultiTenantWorkspace.Infrastructure.Tests/
```

## Important Files

- `src/MultiTenantWorkspace.Web/Program.cs`
  - application startup
  - dependency injection
  - JWT configuration
  - Swagger setup
  - static UI hosting
  - database creation on startup

- `src/MultiTenantWorkspace.Web/wwwroot/index.html`
  - built-in operations console UI

- `src/MultiTenantWorkspace.Web/wwwroot/css/styles.css`
  - layout, top bar, left sidebar, responsive styling

- `src/MultiTenantWorkspace.Web/wwwroot/js/app.js`
  - frontend behavior and API requests

- `src/MultiTenantWorkspace.Infrastructure/Persistence/ApplicationDbContext.cs`
  - EF Core entity mapping

- `src/MultiTenantWorkspace.Application/Features/Authentication/Services/AuthService.cs`
  - registration and login logic

- `src/MultiTenantWorkspace.Application/Features/Projects/Services/ProjectService.cs`
  - project creation and listing

- `src/MultiTenantWorkspace.Application/Features/Tasks/Services/TaskService.cs`
  - task creation, status updates, comments

## Technology Stack

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server / LocalDB
- JWT Bearer Authentication
- BCrypt password hashing
- Swagger / OpenAPI
- API Versioning
- xUnit for tests
- HTML, CSS, JavaScript for the built-in UI

## How the Application Works

### Registration Flow

1. A user registers a new organization.
2. The system creates the organization record.
3. The system creates the first admin user for that organization.
4. The password is hashed using BCrypt.
5. A JWT token is generated and returned.
6. An activity log entry is created.

### Login Flow

1. User enters email and password.
2. The system validates the credentials.
3. The password is checked against the stored hash.
4. A JWT token is generated and returned.
5. The token is used to access protected endpoints.

### Refresh Token Flow

1. Login or registration returns both an access token and a refresh token.
2. The refresh token is stored server-side as a hash.
3. When the access token expires, the client can call the refresh endpoint.
4. The existing refresh token is revoked and rotated.
5. A new access token and refresh token are returned.

### Logout Flow

1. The client sends the current refresh token to the logout endpoint.
2. The server revokes that refresh token.
3. The browser clears the local session state.
4. The revoked refresh token can no longer be used.

### Multi-Tenant Data Flow

1. The JWT includes the `organizationId`.
2. The current user context is read from claims.
3. Application services use that tenant id to scope data access.
4. Users only see data from their own organization.

### Built-in UI Flow

1. Start the API project.
2. Open the browser.
3. Use the Authentication section to register or login.
4. Use the Workspace section to load organization details.
5. Use the Projects and Tasks section to create project/task data.
6. Use the Request Console to inspect the latest API response.

## API Endpoints

Base version: `/api/v1`

### Auth

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`

### Organization

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

## Database

Default connection string:

```json
"DefaultConnection": "Server=DESKTOP-2DOKIE3\\SQLEXPRESS;Database=ProjectManagementSaaSDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
```

Current behavior:

- the app is configured for the local SQL Server instance `DESKTOP-2DOKIE3\\SQLEXPRESS`
- the schema is created automatically on startup with `Database.EnsureCreated()`
- EF migrations are configured in the solution but an initial migration has not yet been generated

## How to Run

### Prerequisites

- .NET 9 SDK installed
- SQL Server LocalDB available on the machine

### Run the API and UI

```powershell
dotnet run --project src/MultiTenantWorkspace.Web
```

Default URLs from launch settings:

- `http://localhost:5115`
- `https://localhost:7192`

### Run with Docker

The repository now includes Docker support for both the API and SQL Server.

Requirements:

- Docker Desktop or Docker Engine with Compose support

Start the stack:

```powershell
docker compose up --build
```

Container endpoints:

- API UI and Swagger: `http://localhost:8080`
- SQL Server: `localhost,1433`

Notes:

- the API uses `ConnectionStrings__DefaultConnection` to connect to the `sqlserver` service
- SQL connection retries are enabled for container startup timing
- API logs are persisted in the `api-logs` Docker volume
- SQL data is persisted in the `sqlserver-data` Docker volume

Stop the stack:

```powershell
docker compose down
```

### Demo Seed Data

On the first run, the application seeds a demo workspace automatically if the database is empty.

Seeded organization:

- `Contoso Delivery Hub`
- slug: `contoso-delivery`

Seeded users:

- Admin: `admin@contoso-demo.com`
- Manager: `manager@contoso-demo.com`
- Member: `member@contoso-demo.com`

Shared password:

- `Password123!`

Seeded content includes:

- 2 demo projects
- 3 demo tasks
- 2 task comments
- initial activity logs

### Build the Solution

```powershell
dotnet build MultiTenantWorkspace.sln
```

### Run Tests

```powershell
dotnet test MultiTenantWorkspace.sln --no-build
```

## GitHub Actions

The repository now includes a GitHub Actions pipeline at `.github/workflows/ci.yml`.

Workflow behavior:

- runs on pushes to `main`
- runs on pull requests targeting `main`
- supports manual runs with `workflow_dispatch`
- restores, builds, and tests the .NET solution in `Release`
- uploads `.trx` test results as a workflow artifact
- validates the Docker image build using the root `Dockerfile`

## Built-in UI Layout

The application includes a built-in static UI hosted by the API.

### Layout Sections

- top navigation bar
- left sidebar menu
- overview dashboard
- authentication panel
- organization workspace panel
- projects and tasks panel
- request console

### UI Purpose

The UI is intended for:

- demo presentation
- interview showcase
- portfolio display
- quick manual testing without Postman

## Security Notes

- Passwords are hashed with BCrypt
- JWT authentication is enabled for protected endpoints
- Role checks are applied on sensitive operations
- Tenant scoping is enforced through claims-based current user context

## Testing

Included tests cover application-layer behavior for:

- successful organization registration
- invalid login behavior
- allowed task status update for assigned member
- forbidden task status update for unrelated member

## Current Limitations

- No frontend framework; UI uses plain HTML/CSS/JavaScript
- No initial EF migration file generated yet
- No advanced reporting/dashboard widgets yet
- No file attachments support yet

## Suggested Next Improvements

- Add EF Core migrations
- Add seed/demo data
- Add project details page and task tables/cards
- Add richer user management screens
- Add Serilog or structured logging
- Add Docker support
- Add GitHub Actions pipeline

## Why This Project Is Good for Portfolio or Interviews

This project demonstrates:

- backend architecture design
- real-world multi-tenant thinking
- secure authentication and authorization
- layered separation of concerns
- repository and unit of work patterns
- EF Core with SQL Server
- API versioning and Swagger
- a runnable UI for demonstration

It is a solid showcase project for .NET backend, enterprise application design, and interview presentation.
