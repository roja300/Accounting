# Accounting

Accounting is a self-hosted workspace for work records, invoices, bank payments, and practical UK tax estimates.

## Run the production stack

Install Docker Desktop with Docker Compose, then run the launcher from the repository root:

```powershell
./start-production.ps1
```

On the first run, the launcher asks only for a database username and hidden password, then creates the Git-ignored `src/.env.production` file with the default `accounting` database and frontend port 3000. It builds and starts PostgreSQL, the ASP.NET Core API, and the nginx-hosted React application, waits for every service to become healthy, and opens the application in your default browser. Edit `src/.env.production` to change the saved configuration, or pass `-NoBrowser` when running headlessly. Only nginx is exposed to the host; terminate TLS with a trusted reverse proxy before exposing the application outside a trusted network.

Stop the production containers without deleting their PostgreSQL data:

```powershell
docker compose --env-file src/.env.production --file src/compose.production.yaml down
```

The existing development workflow remains available from `src/`:

```powershell
docker compose --env-file .env.development up --build
```

The project documentation is published on GitHub Pages:

- [Overview](https://roja300.github.io/Accounting/)
- [Getting started](https://roja300.github.io/Accounting/getting-started/)
- [User guide](https://roja300.github.io/Accounting/guide/)
- [Developer guide](https://roja300.github.io/Accounting/developers/)

The developer guide covers the architecture, local development, HTTPS, validation commands, debugging, and database conventions.
