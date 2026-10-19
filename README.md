# Accounting

Accounting is a work-record and invoicing application. It provides a React web interface for recording and reviewing completed work, an ASP.NET Core API for validation and business logic, and PostgreSQL for durable storage.

## Technology stack

- Frontend: React 19, TypeScript, Vite, Tailwind CSS 4, shadcn/ui, and React Router.
- Backend: ASP.NET Core targeting .NET 10, with controller-based HTTP APIs and modular application services.
- Data: PostgreSQL 17 with Entity Framework Core and Npgsql migrations.
- Tooling: npm, Biome, Vitest, Playwright, xUnit, and Docker Compose.

## Run the application

### Prerequisites

Install Docker Desktop with Docker Compose, Node.js 20 or later, and the .NET 10 SDK. Docker is the simplest way to run the complete application because it supplies PostgreSQL and configures the API and frontend together. HTTPS also requires a trusted development certificate or a certificate and private key supplied by your deployment environment.

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

To run the complete stack over HTTPS, trust and export the .NET development certificate as PEM files. The certificate directory is ignored by Git and must never contain committed production keys.

```powershell
Set-Location src
New-Item -ItemType Directory -Force certificates
dotnet dev-certs https --trust
dotnet dev-certs https --export-path certificates/tls.crt --format Pem --no-password
docker compose --env-file .env.development -f compose.yaml -f compose.https.yaml up --build
```

The HTTPS overlay expects `tls.crt` and `tls.key` in `HTTPS_CERTIFICATE_DIRECTORY`, which defaults to `src/certificates`. It enables TLS for the browser-facing frontend and the directly exposed API. The frontend terminates browser TLS and proxies API requests over the isolated Compose network.

Firefox uses its own certificate store by default. To trust this local certificate in Firefox, either set `security.enterprise_roots.enabled` to `true` in `about:config` and restart Firefox, or import `certificates/tls.crt` under Settings > Privacy & Security > Certificates > View Certificates > Servers and trust it for websites. Edge/Chrome on Windows use the certificate trusted by `dotnet dev-certs https --trust` automatically.

- HTTPS frontend: `https://localhost:3000`
- HTTPS API and OpenAPI document: `https://localhost:8443/openapi/v1.json`
- HTTPS readiness check: `https://localhost:8443/health/ready`

Production deployments should replace the development certificate with a certificate issued for the deployed host, protect the private key outside the repository, and terminate TLS at the application gateway or configure the supplied certificate paths through the deployment environment.

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

Vite serves the frontend at `http://localhost:5173`. By default, its `/api` proxy targets `http://localhost:8080`; use the HTTPS API launch profile by overriding the target and allowing its trusted development certificate:

```powershell
$env:VITE_API_PROXY_TARGET = "https://localhost:7199"
$env:API_PROXY_VERIFY_TLS = "false"
npm run dev
```

To serve Vite itself over HTTPS, set `HTTPS_CERTIFICATE_PATH` and `HTTPS_KEY_PATH` to PEM certificate and key files before running `npm run dev`. Keep `API_PROXY_VERIFY_TLS` enabled for certificates issued by a trusted authority; disabling it is only intended for the local self-signed development certificate.

### Run the backend locally

PostgreSQL must be available before using persistence-backed API endpoints. Start only the database with Compose, then run the API from the repository root:

```powershell
Set-Location src
docker compose --env-file .env.development up -d postgresql
Set-Location ..
dotnet dev-certs https --trust
dotnet run --project src/backend/Server.Api --launch-profile https
```

The HTTPS launch profile uses `https://localhost:7199` and redirects requests from `http://localhost:5200`. Supply `ConnectionStrings__PostgreSQL` through the environment when running outside Compose; do not commit credentials, connection strings, certificates, or private keys.

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

For backend debugging, start `Server.Api` with the `https` launch profile from Visual Studio, Visual Studio Code, or `dotnet run`. Set breakpoints in controllers, application services, or persistence code and inspect the API through OpenAPI at `https://localhost:7199/openapi/v1.json` when the development environment is active.

Use `/health/live` to check that the API process is responding and `/health/ready` to check PostgreSQL connectivity. When diagnosing persistence startup, inspect the API logs for migration or connection errors and verify that PostgreSQL is healthy with `docker compose ps`.

### Database development

PostgreSQL is the supported production database. Schema changes belong in version-controlled Entity Framework migrations; review generated migrations before applying them. Never use the default development credentials outside a local environment, and do not run destructive migrations against shared data without explicit approval and a recovery plan.
