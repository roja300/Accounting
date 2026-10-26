---
layout: default
title: Getting started
page_key: getting-started
description: Run the production or development Accounting stack with Docker Compose, PostgreSQL, ASP.NET Core, and React.
---

<div class="shell docs-layout">
  <aside class="contents" aria-label="On this page"><p>On this page</p><a href="#prerequisites">Prerequisites</a><a href="#clone-the-repository">Clone the repository</a><a href="#start-production">Start production</a><a href="#open-the-application">Open the application</a><a href="#stop-production">Stop production</a><a href="#development-stack">Development stack</a><a href="#development-https">Development HTTPS</a></aside>
  <article class="prose">
    <h2 id="prerequisites">Prerequisites</h2>
    <p>Install <a href="https://www.docker.com/products/docker-desktop/">Docker Desktop</a> with Docker Compose. For development outside containers, also install Node.js 20 or later and the .NET 10 SDK.</p>
    <div class="note"><strong>Protect production data</strong><p>Never commit production credentials, certificates, private keys, connection strings, or customer data. Back up the PostgreSQL volume before upgrades or other operational changes.</p></div>

    <h2 id="clone-the-repository">Clone the repository</h2>
    <p>Start with a clean checkout of the public source repository.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>git clone https://github.com/roja300/Accounting.git
Set-Location Accounting</code></pre></div>

    <h2 id="start-production">Start production</h2>
    <p>Run the startup script from the repository root. On the first run, it asks only for a database username and hidden password, then creates the Git-ignored <code>src/.env.production</code> file with the default <code>accounting</code> database and frontend port 3000.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>./start-production.ps1</code></pre></div>
    <p>The launcher builds the production images, starts PostgreSQL, the ASP.NET Core API, and the nginx-hosted React application in the background, waits for all services to become healthy, and opens the application in your default browser. The API applies pending Entity Framework migrations during startup. Edit <code>src/.env.production</code> to change the saved configuration, or pass <code>-NoBrowser</code> for a headless launch.</p>

    <h2 id="open-the-application">Open the application</h2>
    <div class="endpoint-list"><a href="http://localhost:3000"><span><strong>Frontend</strong><small>Use the application</small></span><code>localhost:3000</code></a><a href="http://localhost:3000/api/health/ready"><span><strong>Readiness</strong><small>Check API and database health through nginx</small></span><code>/api/health/ready</code></a></div>
    <p>On a new database, begin with your business profile. Then add work or import payments before reviewing invoices and tax estimates.</p>

    <div class="warning"><strong>Add TLS before internet exposure</strong><p>The production stack publishes HTTP through nginx. Put it behind a trusted TLS-terminating reverse proxy before exposing it outside a trusted network. PostgreSQL and the API are available only inside the production Compose network.</p></div>

    <h2 id="stop-production">Stop production</h2>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>docker compose --env-file src/.env.production --file src/compose.production.yaml down</code></pre></div>
    <div class="warning"><strong>Keep your data</strong><p>Do not add <code>--volumes</code> unless you intentionally want to delete the production PostgreSQL volume and all application data.</p></div>

    <h2 id="development-stack">Development stack</h2>
    <p>The existing development workflow remains separate and unchanged. It runs Vite with the frontend source mounted for live updates and publishes PostgreSQL and the API for local tooling.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>Set-Location src
docker compose --env-file .env.development up --build</code></pre></div>
    <p>Stop it without deleting local development data with <code>docker compose --env-file .env.development down</code>.</p>

    <h2 id="development-https">Development HTTPS</h2>
    <p>For trusted HTTPS during local development, create and export a .NET development certificate, then add the Compose HTTPS overlay.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>New-Item -ItemType Directory -Force certificates
dotnet dev-certs https --trust
dotnet dev-certs https --export-path certificates/tls.crt --format Pem --no-password
docker compose --env-file .env.development -f compose.yaml -f compose.https.yaml up --build</code></pre></div>
    <p>The HTTPS frontend is available at <code>https://localhost:3000</code> and the API at <code>https://localhost:8443</code>. See the <a href="{{ '/developers/#local-https' | relative_url }}">developer guide</a> for certificate-store and local proxy guidance.</p>

    <div class="next-card"><p class="eyebrow">Next</p><h2>Take a guided tour</h2><p>Follow the everyday workflow from profile setup to payment and tax review.</p><a class="text-link" href="{{ '/guide/' | relative_url }}">Open the user guide <span aria-hidden="true">→</span></a></div>
  </article>
</div>
