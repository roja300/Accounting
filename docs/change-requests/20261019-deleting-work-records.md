# Create Invoices Persistence

*Date: 19/10/2026* <br>
*Author: Rojaware*<br>

## Overview

Editing work records.

### Functional Specification (FS)

1. Deleting work records.

### Non-functional Specification (NFS)

1. The same patterns should be followed as with creating work records.
2. Use the same status pattern as when work is marked as invoiced, however, they will be hidden from view.

## Notes

- Use the same status function as when work is marked as invoiced.
- There is already a button for this.

### Codex Response

#### [2026-10-24 17:06] Implemented Work Record Deletion

Added status-based soft deletion for work records, including explicit uninvoiced, invoiced, and deleted states, a PostgreSQL migration, and filtering that prevents deleted records from appearing in list and detail queries. Added `DELETE /invoices/{id}` with not-found handling and reused the status-update service pattern so records remain retained for accounting history.

Enabled the existing Delete card action with an accessible confirmation dialog, progress and error handling, success feedback, and refreshed pagination after deletion. Added service, endpoint, and end-to-end coverage, and corrected Playwright discovery so all existing browser suites run together.

##### Comments

1. Deletion is reversible at the data level through a future controlled status transition, but no restore action is exposed in this change.
2. The repository-wide Biome check still reports pre-existing formatting and import-order diagnostics in untouched frontend files; all files changed by this request pass scoped Biome validation.
