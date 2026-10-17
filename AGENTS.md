# Agents Guidance

These instructions apply to the entire repository. A more specific `AGENTS.md` in a subdirectory takes precedence for files in that directory.

## Working Agreement

- Inspect the working tree before editing and preserve unrelated user changes.
- Keep changes focused on the requested outcome; avoid opportunistic rewrites or dependency upgrades.
- Do not stage or commit changes unless the user explicitly asks.
- Never add secrets, credentials, connection strings, or real customer data.
- Do not edit generated output or dependency directories such as `bin/`, `obj/`, `dist/`, or `node_modules/`.
- Do not hard-wrap Markdown prose. Keep each paragraph and list item on one physical line and let the editor or renderer handle visual wrapping.
- Use the relevant commands below to validate changed code. Report any command that was not run or any failure that appears unrelated to the change.
- Tests are part of the definition of done for every task. Add or update focused tests for every behavioural change and bug fix, and run all relevant existing test suites before handoff. For documentation-only or other non-behavioural work, still run the relevant suite when practical and state why no test was added or changed.
- Never delete, skip, or weaken a test merely to make a change pass. Update a test only when the intended behaviour has genuinely changed.

## Repository Layout

- `src/frontend/`: React 19, TypeScript, Vite, Tailwind CSS 4, and shadcn/ui.
- `src/backend/Server.Api/`: ASP.NET Core controller-based API targeting .NET 10.
- `src/Accounting.slnx`: backend solution file.
- `docs/change-requests/`: requirement documents and their template. When a change request is named, treat its functional and non-functional requirements as the source of truth. If work is initiated through a change request, ensure that a branch is created to support that.

## Frontend

- Work from `src/frontend/` for npm commands. Use the committed lockfile and run `npm ci` when dependencies need restoring.
- Keep TypeScript strict. Do not suppress type or lint errors with `any`, `@ts-ignore`, or disabled rules without a documented reason.
- Prefer function components, small composable modules, and derived state over duplicated state or effects.
- Use the `@/` alias for imports from `src/`. Use `import type` where an import is type-only.
- Reuse existing shadcn/ui primitives from `src/components/ui/` and the `cn` helper from `@/lib/utils`. Use Lucide for icons instead of hand-authored SVGs.
- Follow the existing Tailwind theme tokens and CSS variables. Avoid hard-coded colours when a semantic token exists, and preserve light/dark theme support.
- Make UI changes responsive, keyboard accessible, and usable with visible focus states and semantic HTML. Give icon-only controls an accessible name.
- Follow the existing Prettier style; format only files touched by the task.
- Every frontend task must preserve or improve automated coverage. Add or update Vitest and Testing Library tests alongside changed components, hooks, utilities, and user-visible behaviour. Bug fixes require a regression test that fails without the fix.
- Test observable behaviour and accessibility through the way a user interacts with the interface; avoid asserting component implementation details.
- Place frontend tests in a `__tests__/` directory adjacent to the code or feature they cover. Keep shared test utilities and mocks in `src/testing/`.
- Name unit and integration test files after the subject in kebab-case, using `*.test.ts` for non-JSX code and `*.test.tsx` for React code. For example, `format-currency.test.ts` and `invoice-form.test.tsx`.
- Keep end-to-end tests under `e2e/tests/` and name them with the kebab-case `*.spec.ts` suffix, such as `create-invoice.spec.ts`. Do not use `.spec` for Vitest unit or integration tests.
- Name frontend cases with `test("should <observable behaviour>", ...)`. Keep the text lowercase after `should`, specific about the outcome, and meaningful when shown alone in test output. Use `describe` only when grouping related scenarios makes the output clearer.

Validate frontend changes from `src/frontend/`:

```powershell
npm run lint
npm run typecheck
npm test
npm run build
```

## Backend

- Follow standard ASP.NET Core conventions and the nullable annotations enabled by the project.
- Keep HTTP concerns in controllers and business logic in injected services as functionality grows. Register dependencies in the application composition root rather than constructing them inside controllers.
- Use asynchronous APIs for I/O and pass `CancellationToken` through request, service, and persistence boundaries.
- Validate input at the API boundary. Return intentional status codes and stable error shapes; do not expose exception details or sensitive data.
- Keep public request/response contracts separate from persistence models. When a contract changes, update its frontend consumer in the same change.
- Put environment-independent defaults in `appsettings.json`, local overrides in `appsettings.Development.json`, and secrets in environment variables or user secrets.
- Keep OpenAPI metadata accurate when adding or changing endpoints.
- Every backend task must preserve or improve automated coverage. Add or update unit tests for domain and service behaviour and integration tests for HTTP, persistence, serialization, authentication, or other system boundaries. Bug fixes require a regression test that fails without the fix.
- Keep tests deterministic and isolated. Do not depend on execution order, shared mutable state, wall-clock timing, or uncontrolled randomness.
- Name backend test classes after the subject with a `Tests` suffix. Name test methods with the behaviour-first form `ShouldExpectedBehaviour_WhenCondition` (omit the condition when it adds no value), for example, `ShouldReturnNotFound_WhenInvoiceDoesNotExist`.

Validate backend changes from the repository root:

```powershell
dotnet build src/Accounting.slnx
dotnet test src/Accounting.slnx
```

Do not silently alter project or solution references merely to bypass a pre-existing restore, build, or test failure.

## Database

PostgreSQL is the required database for this repository. Do not introduce a different production database provider. No persistence project or migration tool is configured yet, so confirm deployment constraints before adding the PostgreSQL persistence layer.

When a database layer is added:

- Use the PostgreSQL provider appropriate to the selected .NET data-access stack (for example, Npgsql for EF Core) and keep provider versions compatible with the repository's .NET version.
- Use a real, isolated PostgreSQL instance for integration tests when database behaviour matters; do not substitute EF Core InMemory or SQLite for tests of PostgreSQL queries, constraints, transactions, or migrations.
- Keep schema changes in version-controlled migrations and review generated migrations before applying them.
- Never rewrite a migration that may already have been applied; add a new migration instead.
- Use explicit PostgreSQL `numeric(precision, scale)` columns for monetary values, never floating-point types. Define nullability, keys, foreign keys, indexes, constraints, and delete behaviour explicitly.
- Store timestamps consistently in UTC using `timestamptz` where appropriate, and keep audit/history requirements in mind for accounting records.
- Use transactions for multi-step financial writes and make retryable operations idempotent where practical.
- Do not run destructive migrations or commands against a shared database without explicit user approval and a recovery plan.
