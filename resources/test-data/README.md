# Payment import test data

These files contain fictional data for manually exercising the Payments CSV import. No names, references, or transactions represent real customers or accounts.

- `payments-standard.csv` contains twelve valid payments using date, time, description, counterparty, reference, signed amount, balance, and currency columns.
- `payments-credit-debit.csv` contains ten valid payments using the separate debit and credit columns common in UK bank exports. Currency defaults to GBP because the export does not contain a currency column.
- `payments-invalid.csv` contains deliberately invalid rows for checking that the server reports every row error and saves none of the file.

Import each file separately through the Payments page. The two valid files should add 22 payments in total; the invalid file should be rejected atomically.
