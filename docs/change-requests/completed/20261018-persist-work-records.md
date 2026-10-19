# Create Invoices Persistence

*Date: 18/10/2026* <br>
*Author: Rojaware*<br>

## Overview

Persist records created when creating a new work record.

### Functional Specification (FS)

1. The system should enable persisting work records across reloads of the app.
2. The webs service should have matching server-side validation to the client.

### Non-functional Specification (NFS)

1. The client should send a request to the backend with a DTO.
2. The DTO class received by the backend should be an immutable `record` type.
3. The database will need to be created. Use migrations.
4. The database should be PostgreSQL.
5. The work flow for the endpoint should be controller, service, context, database.
6. The code should be managed through the controller endpoint.
7. The module should be responsible for its own DI.
8. The validation should be unit tested.
9. The service should be unit tested.
10. Unit tests that require the database, should be mocked. Do not use an in-memory database.
11. Follow PostgreSQL naming conventions.

## Notes

- Some of the structure for the backend has been created for you.
- Configurations may be required.

### Codex Response

[//]: # (Remove these two, this is just as an example of how you should format your responses.)

#### [2026-11-07 16:04] You Did a Thing

Fill me in...

##### Comments

1. N/A

#### [2026-11-07 17:29] You Did a Thing Again

Fill me in with your updates.

##### Comments

1. N/A
