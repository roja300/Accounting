---
layout: default
title: Getting started
page_key: getting-started
description: Install and run Accounting locally with Docker Compose, PostgreSQL, ASP.NET Core, and React.
---

<div class="shell docs-layout">
  <aside class="contents" aria-label="On this page"><p>On this page</p><a href="#prerequisites">Prerequisites</a><a href="#clone-the-repository">Clone the repository</a><a href="#start-the-stack">Start the stack</a><a href="#open-the-application">Open the application</a><a href="#https-optional">HTTPS (optional)</a><a href="#stop-the-stack">Stop the stack</a></aside>
  <article class="prose">
    <h2 id="prerequisites">Prerequisites</h2>
    <p>Install <a href="https://www.docker.com/products/docker-desktop/">Docker Desktop</a> with Docker Compose. For development outside containers, also install Node.js 20 or later and the .NET 10 SDK.</p>
    <div class="note"><strong>Local data only</strong><p>The committed development environment contains local-only defaults. Never reuse them for production or commit certificates, private keys, connection strings, or customer data.</p></div>

    <h2 id="clone-the-repository">Clone the repository</h2>
    <p>Start with a clean checkout of the public source repository.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>git clone https://github.com/roja300/Accounting.git
Set-Location Accounting</code></pre></div>

    <h2 id="start-the-stack">Start the stack</h2>
    <p>Run the Compose stack from the repository's <code>src</code> directory. The first run builds the application images and creates the local PostgreSQL volume.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>Set-Location src
docker compose --env-file .env.development up --build</code></pre></div>
    <p>Wait until PostgreSQL reports healthy and the API and frontend have started. The API applies pending Entity Framework migrations during startup.</p>

    <h2 id="open-the-application">Open the application</h2>
    <div class="endpoint-list"><a href="http://localhost:3000"><span><strong>Frontend</strong><small>Use the application</small></span><code>localhost:3000</code></a><a href="http://localhost:8080/openapi/v1.json"><span><strong>OpenAPI</strong><small>Inspect the HTTP contract</small></span><code>localhost:8080</code></a><a href="http://localhost:8080/health/ready"><span><strong>Readiness</strong><small>Check API and database health</small></span><code>/health/ready</code></a></div>
    <p>On a new database, begin with your business profile. Then add work or import payments before reviewing invoices and tax estimates.</p>

    <h2 id="https-optional">HTTPS (optional)</h2>
    <p>For a trusted local HTTPS setup, create and export a .NET development certificate, then add the Compose HTTPS overlay.</p>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>New-Item -ItemType Directory -Force certificates
dotnet dev-certs https --trust
dotnet dev-certs https --export-path certificates/tls.crt --format Pem --no-password
docker compose --env-file .env.development -f compose.yaml -f compose.https.yaml up --build</code></pre></div>
    <p>The HTTPS frontend is available at <code>https://localhost:3000</code> and the API at <code>https://localhost:8443</code>. See the <a href="{{ '/developers/#local-https' | relative_url }}">developer guide</a> for certificate-store and local proxy guidance.</p>

    <h2 id="stop-the-stack">Stop the stack</h2>
    <div class="code-block"><div class="code-label">PowerShell</div><pre><code>docker compose --env-file .env.development down</code></pre></div>
    <div class="warning"><strong>Keep your data</strong><p>Do not add <code>-v</code> unless you intentionally want to delete the local PostgreSQL volume and all development data.</p></div>

    <div class="next-card"><p class="eyebrow">Next</p><h2>Take a guided tour</h2><p>Follow the everyday workflow from profile setup to payment and tax review.</p><a class="text-link" href="{{ '/guide/' | relative_url }}">Open the user guide <span aria-hidden="true">→</span></a></div>
  </article>
</div>
