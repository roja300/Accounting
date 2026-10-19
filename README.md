# Accounting

Accounting is a work-record and invoicing application. It provides a React web interface for recording and reviewing completed work, an ASP.NET Core API for validation and business logic, and PostgreSQL for durable storage.

## Technology stack

- Frontend: React 19, TypeScript, Vite, Tailwind CSS 4, shadcn/ui, and React Router.
- Backend: ASP.NET Core targeting .NET 10, with controller-based HTTP APIs and modular application services.
- Data: PostgreSQL 17 with Entity Framework Core and Npgsql migrations.
- Tooling: npm, Biome, Vitest, Playwright, xUnit, and Docker Compose.

## Run the application

### Prerequisites

Install Docker Desktop with Docker Compose, Node.js 20 or later, and the .NET 10 SDK. Docker is the simplest way to run the complete application because it supplies PostgreSQL and configures the API and frontend together.

### Docker Compose

The Compose stack runs PostgreSQL, the ASP.NET Core API, and the React frontend. Development defaults are in `src/.env.development` and should only be used for local development.

```powershell
Set-Location src
docker compose --env-file .env.development up --build
```

Open the frontend at `http://localhost:3000`. The frontend proxies `/api` requests to the backend, which is available at `http://localhost:8080`.

- Frontend: `http://localhost:3000`
- OpenAPI document: `http://localhost:8080/openapi/v1.json`
- Live readiness check: `http://localhost:8080/health/ready`
- PostgreSQL: `localhost:5432`

The API applies pending Entity Framework migrations when it starts with a PostgreSQL connection string. Stop the stack with:

```powershell
docker compose --env-file .env.development down
```

Add `-v` only when you intentionally want to remove the local PostgreSQL volume and all development data.

## Development

### Repository layout

- `src/frontend/`: React application, frontend tests, and Vite configuration.
- `src/backend/Server.Api/`: ASP.NET Core host, health checks, OpenAPI, and composition root.
- `src/backend/Server.Invoices/`: work-record domain, API contracts, services, persistence, and migrations.
- `src/backend/Server.*.Tests/`: backend unit and HTTP integration tests.
- `e2e/tests/`: Playwright tests that exercise the frontend as a user.
- `src/compose.yaml`: local PostgreSQL, backend, and frontend services.

### Run the frontend locally

```powershell
Set-Location src/frontend
npm ci
npm run dev
```

Vite serves the frontend at `http://localhost:5173`. By default, its `/api` proxy targets `http://localhost:8080`; override this when the API is running on another port:

```powershell
$env:VITE_API_PROXY_TARGET = "http://localhost:5200"
npm run dev
```

### Run the backend locally

PostgreSQL must be available before using persistence-backed API endpoints. Start only the database with Compose, then run the API from the repository root:

```powershell
Set-Location src
docker compose --env-file .env.development up -d postgresql
Set-Location ..
dotnet run --project src/backend/Server.Api --launch-profile http
```

The HTTP launch profile uses `http://localhost:5200`. Supply `ConnectionStrings__PostgreSQL` through the environment when running outside Compose; do not commit credentials or connection strings.

### Tests and quality checks

Run backend checks from the repository root:

```powershell
dotnet build src/Accounting.slnx
dotnet test src/Accounting.slnx
```

Run frontend checks from `src/frontend/`:

```powershell
npm run check
npm run typecheck
npm test
npm run build
```

Run the browser-level tests with Playwright. Install the browser once per machine, then run the suite; the Playwright config starts a Vite server automatically.

```powershell
npx playwright install chromium
npm run test:e2e
```

Use `npm run test:watch` for an interactive Vitest session. Frontend user-visible behavior belongs in `e2e/tests/`; the Vitest suite is for existing non-browser unit coverage.

### Debugging

For frontend debugging, run Vite with `npm run dev` and use the browser developer tools. API requests appear under `/api`; the Vite proxy keeps browser requests same-origin during local development.

For backend debugging, start `Server.Api` with the `http` launch profile from Visual Studio, Visual Studio Code, or `dotnet run`. Set breakpoints in controllers, application services, or persistence code and inspect the API through OpenAPI at `http://localhost:5200/openapi/v1.json` when the development environment is active.

Use `/health/live` to check that the API process is responding and `/health/ready` to check PostgreSQL connectivity. When diagnosing persistence startup, inspect the API logs for migration or connection errors and verify that PostgreSQL is healthy with `docker compose ps`.

### Database development

PostgreSQL is the supported production database. Schema changes belong in version-controlled Entity Framework migrations; review generated migrations before applying them. Never use the default development credentials outside a local environment, and do not run destructive migrations against shared data without explicit approval and a recovery plan.
