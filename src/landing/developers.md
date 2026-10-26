---
layout: default
title: Developers
page_key: developers
description: Architecture, local development, validation, debugging, and database guidance for Accounting contributors.
---

<div class="shell docs-layout">
  <aside class="contents" aria-label="On this page"><p>On this page</p><a href="#technology">Technology</a><a href="#repository-layout">Repository layout</a><a href="#frontend-development">Frontend</a><a href="#backend-development">Backend</a><a href="#local-https">Local HTTPS</a><a href="#validation">Validation</a><a href="#debugging">Debugging</a><a href="#database-development">Database</a></aside>
  <article class="prose">
    <h2 id="technology">Technology</h2>
    <ul><li><strong>Frontend:</strong> React 19, TypeScript, Vite, Tailwind CSS 4, shadcn/ui, and React Router.</li><li><strong>Backend:</strong> ASP.NET Core on .NET 10 with controller-based HTTP APIs and modular application services.</li><li><strong>Data:</strong> PostgreSQL 17 with Entity Framework Core and Npgsql migrations.</li><li><strong>Tooling:</strong> npm, Biome, Vitest, Playwright, xUnit, Docker Compose, and GitHub Actions.</li></ul>

    <h2 id="repository-layout">Repository layout</h2>
    <ul><li><code>src/frontend/</code> contains the React application, frontend tests, and Vite configuration.</li><li><code>src/backend/Server.Api/</code> contains the ASP.NET Core host, health checks, OpenAPI, and composition root.</li><li><code>src/backend/Server.Invoices/</code>, <code>Server.Payments/</code>, and <code>Server.Tax/</code> contain the application modules.</li><li><code>src/backend/Server.*.Tests/</code> contains backend unit and HTTP integration tests.</li><li><code>e2e/tests/</code> and <code>src/frontend/e2e/tests/</code> contain Playwright coverage.</li><li><code>src/compose.yaml</code> defines the local development stack, while <code>src/compose.production.yaml</code> defines the isolated production topology launched by <code>start-production.ps1</code>.</li></ul>

    <h2 id="frontend-development">Frontend development</h2>
    <p>Install locked dependencies and start Vite from the frontend directory.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>Set-Location src/frontend
npm ci
npm run dev</code></pre></div>
    <p>Vite serves the frontend at <code>http://localhost:5173</code>. Its <code>/api</code> proxy targets <code>http://localhost:8080</code> by default.</p>

    <h2 id="backend-development">Backend development</h2>
    <p>Start PostgreSQL with Compose, then run the API from the repository root.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>Set-Location src
docker compose --env-file .env.development up -d postgresql
Set-Location ..
dotnet dev-certs https --trust
dotnet run --project src/backend/Server.Api --launch-profile https</code></pre></div>
    <p>The HTTPS profile uses <code>https://localhost:7199</code> and redirects from <code>http://localhost:5200</code>. Set <code>ConnectionStrings__PostgreSQL</code> when running outside Compose.</p>

    <h2 id="local-https">Local HTTPS</h2>
    <p>To point Vite at the API's HTTPS profile, configure the proxy before starting the development server.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>$env:VITE_API_PROXY_TARGET = "https://localhost:7199"
$env:API_PROXY_VERIFY_TLS = "false"
npm run dev</code></pre></div>
    <p>To serve Vite itself over HTTPS, set <code>HTTPS_CERTIFICATE_PATH</code> and <code>HTTPS_KEY_PATH</code> to PEM files. Disable proxy certificate verification only for a trusted local self-signed certificate.</p>
    <p>Firefox uses its own certificate store by default. Enable <code>security.enterprise_roots.enabled</code> in <code>about:config</code> and restart Firefox, or import <code>certificates/tls.crt</code> under its certificate settings. Edge and Chrome on Windows use the certificate trusted by <code>dotnet dev-certs https --trust</code>.</p>
    <div class="warning"><strong>Protect credentials</strong><p>Never commit certificates, private keys, connection strings, production credentials, or customer data.</p></div>

    <h2 id="validation">Validation</h2>
    <p>Run backend validation from the repository root.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>dotnet build src/Accounting.slnx
dotnet test src/Accounting.slnx</code></pre></div>
    <p>Run frontend validation from <code>src/frontend/</code>.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>npm run check
npm run typecheck
npm test
npm run build
npx playwright install chromium
npm run test:e2e</code></pre></div>
    <p>Use <code>npm run test:watch</code> for an interactive Vitest session. Playwright configuration starts Vite automatically.</p>

    <h2 id="debugging">Debugging</h2>
    <p>Use the browser developer tools with Vite for frontend debugging. Requests under <code>/api</code> remain same-origin and are forwarded to the configured API target.</p>
    <p>Start <code>Server.Api</code> with its HTTPS launch profile to debug controllers, application services, and persistence. Inspect OpenAPI at <code>https://localhost:7199/openapi/v1.json</code>.</p>
    <p>Use <code>/health/live</code> to confirm the API process is responding and <code>/health/ready</code> to check PostgreSQL connectivity. For persistence failures, inspect the API logs and run <code>docker compose ps</code>.</p>

    <h2 id="database-development">Database development</h2>
    <p>PostgreSQL is the supported production database. Keep schema changes in version-controlled Entity Framework migrations and review generated migrations before applying them. Never use local development credentials outside a local environment or run destructive migrations against shared data without explicit approval and a recovery plan.</p>

    <div class="next-card"><p class="eyebrow">Ready to contribute?</p><h2>Explore the source.</h2><p>Review the repository guidance before making a focused, tested change.</p><a class="text-link" href="{{ site.github_url }}">Open the repository <span aria-hidden="true">↗</span></a></div>
  </article>
</div>
