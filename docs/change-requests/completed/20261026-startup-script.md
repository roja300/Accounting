# Add a production startup script

*Date: 26/10/2026* <br>
*Author: Rojaware*<br>

## Overview

Create a startup script that runs a production container orchestration for the application. Supporting documentation should also be updated.

### Functional Specification (FS)

1. The system should support running the entire production app through a .ps1 or bash script.
2. The system should update the documentation to reflect this.
3. The aforementioned script should run production containers.
4. The development containers, and running them as I currently do should be preserved.

### Non-functional Specification (NFS)

1.

## Notes

- N/A

### Codex Response

#### [2026-10-26 18:45] Added the production container stack

Added a root-level PowerShell launcher and an isolated production Compose definition. The production topology builds the React application into an nginx image, runs the API with the ASP.NET Core Production environment, requires the PostgreSQL password through an ignored environment file, persists PostgreSQL data, and exposes only nginx to the host. Preserved the existing development Compose definition and its Vite workflow. Added an automated topology check to continuous integration and corrected the backend Docker restore layer to include all referenced module projects.

##### Comments

1. The example production environment file contains no real credentials and must be copied to the ignored `src/.env.production` file before startup.
2. The launcher and both production and development Compose configurations pass the automated topology check.

#### [2026-10-26 18:49] Documented and validated production startup

Updated the repository README and published getting-started and developer guides with production setup, startup, health-check, shutdown, port-selection, data-retention, and TLS-boundary guidance while retaining the existing development and local HTTPS commands. Added documentation regression coverage and made the existing landing front-matter assertions accept valid LF and CRLF checkouts.

##### Comments

1. End-to-end validation built the production images, brought PostgreSQL, the API, and nginx to healthy state, returned HTTP 200 for the application, and returned `Healthy` from the API readiness endpoint through nginx. The test containers, network, and empty test volume were removed afterward; existing development containers were left running.
2. Backend build and all 88 backend tests pass. Frontend type-checking, all 14 unit tests, and the production build pass. The container topology test passes, and the landing site passes 8 tests with 52 assertions plus a strict production build.
3. The repository-wide Biome check remains blocked by pre-existing formatting and import-style diagnostics in untouched frontend files; no unrelated frontend files were rewritten.

#### [2026-10-26 18:53] Resolved the local production port conflict

Changed the ignored local production environment file to publish nginx on port 3001 because the preserved development frontend was already using port 3000. Resumed the interrupted Compose launch and confirmed that all production services became healthy.

##### Comments

1. The shared production default remains port 3000; only the machine-local, Git-ignored `src/.env.production` setting changed.
2. Validation returned HTTP 200 for `http://localhost:3001/` and `Healthy` from `http://localhost:3001/api/health/ready` while the development stack remained running on port 3000.

#### [2026-10-26 18:54] Opened the application after startup

Updated the production launcher to discover nginx's published host port after the stack becomes healthy, print the resulting application URL, and open it in the default browser. Added a `-NoBrowser` option for headless and automated launches, and updated the operator documentation and its regression assertion.

##### Comments

1. Browser-launch failures produce a warning with the URL instead of incorrectly reporting the already-healthy production stack as failed.
2. Validated both the headless and normal launch paths against the healthy production stack on port 3001. The topology regression passes, and the landing site passes 8 tests with 54 assertions plus a strict production build.
