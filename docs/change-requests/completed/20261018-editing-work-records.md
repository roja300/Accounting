# Create Invoices Persistence

*Date: 18/10/2026* <br>
*Author: Rojaware*<br>

## Overview

Editing work records.

### Functional Specification (FS)

1. The system should provide the option to edit work records.
2. The system should prepopulate the form with the work record.
3. The system should follow the previous validation on the work records.

### Non-functional Specification (NFS)

1. The same patterns should be followed as with creating work records.

## Notes

- N/A

### Codex Response

#### [2026-10-19 19:42] Implemented Work Record Editing

Added a validated `PUT /invoices/{id}` endpoint that updates an existing work record and its client or project relationships while preserving its creation timestamp and returning not found for an unknown record. The work-record card menu now links to an edit route that loads and prepopulates the existing form, reuses the create-work validation rules, handles loading and retry states, submits changes, and confirms success before returning to the invoices page.

Added backend service and endpoint coverage for successful, invalid, and missing-record updates. Added browser coverage for the enabled card action, prepopulated fields, submitted update payload, and shared validation behaviour.

##### Comments

1. No database migration was required because editing uses the existing invoice, client, and project schema.
