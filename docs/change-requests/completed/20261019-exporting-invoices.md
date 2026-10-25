# Create Invoices Persistence

*Date: 19/10/2026* <br>
*Author: Rojaware*<br>

## Overview

This should be a new page for generating invoices.

### Functional Specification (FS)

1. The system should generate professional invoices.
2. The system should use the work records to generate invoices.
3. The system should provide a state to work records, that are marked as exported.
4. The system should keep a record of invoices exported.
5. The system should allow the invoice to be exported by client.
6. The system should optionally allow specific projects to be exported.
7. The system should present the user with options for the invoice as a dialogue.
8. The system should provide a page for viewing invoices that have been exported.
9. The system should provide each invoice in the aforementioned page as a card.
10. The system should provide a nice UI.
11. The system should enable filtering similar to the work records page (where applicable).
12. The system should enable users to mark an invoice as payment received (or something).
13. The system should enable marking invoices as deleted.
14. The system should provide a dialogue warning then user when an invoice is going to be removed.

### Non-functional Specification (NFS)

1. The same patterns should be followed as with creating work records.
2. The invoices should be generated on the server.
3. A table will need to be added support storing information of an invoice that was exported, and, it's associated work records.

## Notes

- There should be a state associated with each invoice. You may choose to use different wording for the following,
  - deleted;
  - exported;
  - sent; and
  - payment received.

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
