*Date: 11/10/2026* <br>
*Author: Rojaware*<br>

## Overview

This work item should include information for calculating tax.

### Functional Specification (FS)

1. The system should provide a means for calculating tax per year.
2. The system should provide a page for displaying tax per year, as cards.
3. The tax calculation should be ongoing as work records are completed, and invoices are paid.
4. The system should display both expected pay, and the amount that needs to be paid, and total earned for the year, on each card.
5. The system should calculate tax per financial year, as cards.
6. The system should use a UK based tax calculation, which would each require information to be further turned into a tax return.
7. The system should provide each tax year as draft, and then be configured for the necessary information.
8. The tax calculation should support working out certain allowances for people that are employed, pension contributions, etc.
9. The system should not support VAT returns yet.

### Non-functional Specification (NFS)

1. The same patterns found throughout the application.

## Notes

- 

### Codex Response

#### [2026-10-25 13:32] Added Ongoing UK Tax-Year Estimates

Added a dedicated tax module, PostgreSQL persistence, and API endpoints for listing and configuring tax years. Tax years run from 6 April to 5 April and are created automatically from GBP work-record and paid-invoice activity, with the current year always available as a draft. Expected pay is derived from active work records, while total earned is derived from invoices marked as payment received.

Implemented an editable UK estimate using the 2026/27 England, Wales and Northern Ireland defaults for Income Tax and Class 4 National Insurance. Each tax year can be configured with employment income, employment tax already paid, pension contributions, allowable business expenses, other allowances, and editable rates and thresholds. Configured values are stored with explicit PostgreSQL monetary precision. VAT is excluded and the interface explains that the result is an estimate rather than a submitted return.

Replaced the placeholder Tax page with responsive, accessible tax-year cards showing expected pay, total earned, the estimated amount to pay, and an Income Tax and National Insurance breakdown. Added a configuration dialog and clear draft/configured states.

Added calculator and service unit tests, API endpoint tests, and browser tests covering card totals and tax-year configuration. The backend build and all 87 backend tests pass; frontend type-checking, all 14 unit tests, the production build, and all 20 browser tests pass. The repository-wide Biome check remains blocked by pre-existing formatting and import-style diagnostics in untouched files; focused Biome checks pass for all tax files changed here.

##### Comments

1. Defaults were verified against HMRC's published 2026/27 Income Tax and National Insurance rates. Scottish Income Tax, VAT, payments on account, student loans, capital gains, dividends, and return submission remain outside this change request.

