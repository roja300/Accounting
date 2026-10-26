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
