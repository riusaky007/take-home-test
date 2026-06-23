# Fundo Loan Management API

A .NET 8 (C#) RESTful API for managing loan applications, built with ASP.NET Core,
Entity Framework Core, and SQL Server.

## Architecture

The project keeps the original `Startup`-based bootstrap and extends it with a
layered structure:

```
Controllers/   -> HTTP endpoints (thin, no business logic)
Services/      -> Business rules (ILoanService / LoanService) + domain exceptions
Data/          -> LoanDbContext, EF Core migrations, seed data
Models/        -> Loan entity + LoanStatus enum
Dtos/          -> Request/response contracts (decoupled from the entity)
Middleware/    -> Global exception handling (ProblemDetails responses)
```

## Endpoints

| Method | Route                   | Description                       | Responses            |
| ------ | ----------------------- | --------------------------------- | -------------------- |
| GET    | `/loans`                | List all loans                    | 200                  |
| GET    | `/loans/{id}`           | Get a single loan                 | 200, 404             |
| POST   | `/loans`                | Create a loan                     | 201, 400             |
| POST   | `/loans/{id}/payment`   | Apply a payment to a loan         | 200, 400, 404        |

Swagger UI is available at `/swagger` when the API is running.

### Business rules

- A new loan starts with `currentBalance == amount` and `status = active`.
- A payment must be greater than zero and cannot exceed the current balance.
- When the balance reaches zero the loan is automatically marked `paid`.
- Payments against an already-paid loan are rejected.

## Running with Docker (recommended)

From the **repository root**:

```sh
docker compose up --build
```

This starts SQL Server and the API. Migrations are applied automatically on
startup (with retry while SQL Server warms up), including seed data.

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

## Running locally (without Docker)

Requires the **.NET 8 SDK** and a reachable SQL Server. Update the connection
string in `Fundo.Applications.WebApi/appsettings.json` (or via the
`ConnectionStrings__DefaultConnection` environment variable) if needed.

```sh
cd Fundo.Applications.WebApi
dotnet run
```

The API listens on http://localhost:5000 and applies migrations + seed data on
startup.

## Tests

```sh
dotnet test
```

- **Unit tests** (`Fundo.Services.Tests/Unit`) cover `LoanService` business rules
  using the EF Core in-memory provider.
- **Integration tests** (`Fundo.Services.Tests/Integration`) exercise the full
  HTTP pipeline via `WebApplicationFactory` against an in-memory database.

## Notes

- Target framework is **.NET 8 (LTS)** — upgraded from the original net6.0, which
  is end-of-life. Stable Docker base images and broad runtime availability make
  this the safer deliverable target.
- Structured logging is provided by **Serilog** (console sink).
- Unhandled domain/runtime exceptions are translated into consistent
  `application/problem+json` responses by `ExceptionHandlingMiddleware`.
