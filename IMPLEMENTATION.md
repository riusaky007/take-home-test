# Implementation Notes

This document describes how the take-home test was implemented, the decisions
made along the way, and what I would improve with more time.

## What was built

### Backend (.NET 8, ASP.NET Core)
- A layered Loan Management API: `Controller -> Service -> EF Core DbContext -> SQL Server`.
- All four required endpoints: create, get-by-id, list, and apply-payment.
- `Loan` entity (`Id`, `Amount`, `CurrentBalance`, `ApplicantName`, `Status`) with an
  EF Core migration and seed data matching the frontend mockup.
- DTOs with data-annotation validation, decoupled from the persistence entity.
- Business rules enforced in `LoanService` (payment > 0, no overpay, auto-mark `paid`).
- Global exception-handling middleware producing consistent `ProblemDetails` responses.
- Structured logging with Serilog.
- xUnit unit tests (service rules) and integration tests (full HTTP pipeline).

### Frontend (Angular 19)
- Loans table wired to `GET /loans` via an `HttpClient`-based `LoanService`.
- Loading, error/retry, and empty states.
- Status rendered as a colored badge; layout follows the provided mockup.

### DevOps
- Multi-stage `Dockerfile` for the API and a `docker-compose.yml` that brings up
  SQL Server + the API, with a health-check gate and automatic migration on boot.
- GitHub Actions workflow that restores, builds, and tests the backend.

## Key decisions

- **Upgraded net6.0 → net8.0.** The scaffold targeted .NET 6, which is end-of-life.
  .NET 8 is the current LTS, has stable Docker images, and is the more defensible
  choice for a deliverable. The original `Startup`/`Program` architecture was kept
  and extended rather than replaced (per the README's request to respect it).
- **DTOs over entities at the API boundary** to keep the contract stable and avoid
  over-posting (e.g. clients cannot set `Status` or `CurrentBalance` directly).
- **Business logic in a service, not the controller**, so it is unit-testable in
  isolation and the controller stays thin.
- **Migration applied on startup with retry** so `docker compose up` works on a
  cold SQL Server without manual migration steps.

## Challenges

- The local machine only had the **.NET 10** runtime installed while the projects
  target **.NET 8**. Running the integration tests via roll-forward surfaced a
  `PipeWriter.UnflushedBytes` incompatibility (net8 TestHost on the net10
  runtime). Resolved by installing a local .NET 8 SDK and running the suite on the
  matching runtime; CI pins .NET 8 so this does not affect the pipeline.

## What I'd do with more time

- **Authentication/authorization** (JWT) — listed as a bonus; skipped to keep the
  table-only frontend in scope.
- **Pagination & filtering** on `GET /loans` for larger datasets.
- **Payment history** as a child entity instead of only mutating the balance.
- **Frontend tests** (component + service) and a create/payment UI.
- **Concurrency handling** (row version / optimistic concurrency) on payments.
- **Money as a dedicated type** and currency awareness rather than raw `decimal`.
