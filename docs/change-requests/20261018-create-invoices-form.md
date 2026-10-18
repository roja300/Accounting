# Requirement Title

*Date: 18/10/2026* <br>
*Author: Rojaware*<br>

## Overview

Create me a new form creating new work records that can be used as a form to be later integrated with the backend. There is already a page and start of a form for this. 

### Functional Specification (FS)

1. The form should contain fields for,
   1. description;
   2. value;
   3. currency type;
   4. additional comments on the record;
   5. client that the work is assigned to,
   6. project that the work is assigned to.
2. The form should validate that it is not empty.
3. The form should add maximum text length where abritrary text can be inputted.
4. The form should present the clients field as drop down with previously created items.
5. The form should enable creating new clients by typing in a value that isn't in the dropdown.
6. The form should inform the user if a client is going to be created with text associated to the field.
7. The form should present the projects field as drop down with previously created items.
8. The form should enable creating new projects by typing in a value that isn't in the dropdown.
9. The form should inform the user if a project is going to be created with text associated to the field.
10. The form should present a dialog upon successful validation (in future after the request is successful, pretend that this is the case for now). 
11. The form should navigate a user to the original page upon succesful validation.

### Non-functional Specification (NFS)

1. The form should use whichever framework/library Shadcn UI prefers for validation. *See docs*.
2. The form should be supported by a typed DTO. This will later be sent to the client.
3. The form should associate validation messages alongisde each field.
4. Use any ShadCN components where applicable. This would include the dialogue, dropdowns, fields, etc. Download anything missing.

## Notes

- This is just a client side change.

### Codex Response

#### [2026-10-18 15:05] Created the New Work Form

Replaced the New Work placeholder with a responsive shadcn form for description, value, currency, comments, client, and project. Added React Hook Form and Zod validation, a typed `CreateWorkRecordDto`, character and monetary limits, accessible field-level messages, creatable client and project comboboxes backed by placeholder existing records, a successful-validation dialog, and navigation back to Invoices after acknowledgement.

##### Comments

1. Client and project options are intentionally local placeholder data until backend integration is introduced.
2. Component-level coverage for this form was removed at the user's request; no end-to-end test was added as part of this change.
3. Type checking and the production build pass. The repository-wide Biome check and existing Vitest suite still report unrelated pre-existing failures in untouched frontend files.
