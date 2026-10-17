# Accounting

## Docker Compose

The Compose stack runs the React frontend, ASP.NET Core API, and PostgreSQL. The frontend proxies `/api/*` to the backend, and the backend readiness check executes `SELECT 1` against PostgreSQL.

The Compose file and its development environment variables live in `src/`.

```powershell
Set-Location src
docker compose --env-file .env.development up --build
```

- Frontend: `http://localhost:3000`
- Backend OpenAPI document: `http://localhost:8080/openapi/v1.json`
- Backend PostgreSQL readiness: `http://localhost:8080/health/ready`
- PostgreSQL: `localhost:5432`

Use `http://localhost:3000/api/WeatherForecast` to verify the complete browser-to-Nginx-to-API path.

```powershell
docker compose --env-file .env.development down
```
