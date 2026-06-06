# Inside the Banking App: Architecture, Data Flows, and Design Patterns

This post walks through how the banking learning application is structured — from the browser to SQL Server — and explains the low-level design choices, the clean architecture layers, and the patterns that tie them together.

**Stack:** .NET 9 Web API · Next.js 16 (App Router) · TypeScript · SQL Server · ADO.NET

---

## 1. What We Are Building

The application is a full-stack retail banking experience: customers register, log in, open accounts, move money, pay bills, and view insights. Staff users can look up customers through an admin view. The backend is deliberately built without Entity Framework — every SQL statement is visible — so the data path from HTTP request to INSERT/UPDATE is easy to follow.

The system splits into two deployable surfaces:

| Surface | Project | Role |
| --- | --- | --- |
| API | Banking.Api + Application + Domain + Infrastructure | Business logic, persistence, auth |
| Web | banking-web | React UI, client-side routing, API client |

They communicate over HTTPS with JSON. The SPA stores a JWT and sends it on every authenticated request.

---

## 2. High-Level Design (HLD)

### 2.1 System context

```
┌──────────────┐     HTTPS / JSON          ┌──────────────────┐
│   Browser    │ ◄───────────────────────► │   Banking.Api    │
│ Next.js:3000 │   Authorization: Bearer   │      :5206       │
└──────────────┘                           └────────┬─────────┘
                                                    │
                    ┌───────────────────────────────┼───────────────────────┐
                    │                               │                       │
                    ▼                               ▼                       ▼
            ┌───────────────┐            ┌─────────────────┐     ┌─────────────────────┐
            │  SQL Server   │            │   Mock Email    │     │ Mock Payment Gateway│
            │   BankingDb   │            │    (console)    │     │     (console)       │
            └───────────────┘            └─────────────────┘     └─────────────────────┘
```

External integrations (email receipts, bill payment gateway) are behind interfaces and implemented as mocks for local development. Swapping in a real provider means adding a new Infrastructure class — the Application layer never changes.

### 2.2 Request lifecycle (bird's-eye view)

Every API call follows the same skeleton:

1. HTTP hits an ASP.NET controller.
2. Middleware runs first (CORS, exception handling, authentication).
3. Controller extracts the customer ID from the JWT, builds a Command or Query, and calls a Handler.
4. Handler (Application layer) enforces business rules and orchestrates repositories.
5. Repository (Infrastructure) executes parameterized SQL.
6. Response flows back as a DTO — never a domain entity.

The frontend mirrors this in reverse:

Page → bankingApi facade → fetch client → JSON → React state

---

## 3. Clean Architecture: Layers and the Dependency Rule

Clean Architecture organizes code in concentric rings. **Dependencies point inward.** The Domain knows nothing about databases or HTTP. The Application knows about Domain but not about ADO.NET. Only Infrastructure and the API composition root wire concrete implementations.

```
┌─────────────────────────────────────────────────────────────┐
│  Presentation                                               │
│  Banking.Api          — controllers, middleware, filters  │
│  banking-web          — pages, components, API client         │
├─────────────────────────────────────────────────────────────┤
│  Application (use cases)                                    │
│  Commands / Queries / Handlers / DTOs / Result<T>           │
│  Abstractions (IAccountRepository, IJwtTokenService, …)     │
├─────────────────────────────────────────────────────────────┤
│  Domain                                                     │
│  Entities, Enums, Domain exceptions                         │
│  Zero NuGet dependencies                                    │
├─────────────────────────────────────────────────────────────┤
│  Infrastructure                                             │
│  ADO.NET repositories, JWT, BCrypt, mock services           │
└─────────────────────────────────────────────────────────────┘
```

### 3.1 How the layers relate

| Layer | Project | Depends on | Must not depend on |
| --- | --- | --- | --- |
| Domain | Banking.Domain | Nothing | Application, Infrastructure, Api |
| Application | Banking.Application | Domain | Infrastructure, Api |
| Infrastructure | Banking.Infrastructure | Application (interfaces), Domain | Api |
| API | Banking.Api | Application, Infrastructure | — (composition root) |
| Web | banking-web | Backend via HTTP only | C# projects |

The **composition root** is Program.cs. It is the only place that says “when someone asks for IAccountRepository, give them AccountRepository.” Application handlers request IAccountRepository in their constructor; they never new AccountRepository().

```csharp
// Banking.Api/Program.cs — wires the graph
builder.Services.AddApplication();           // registers all handlers
builder.Services.AddInfrastructure(config);  // registers all repositories + services
```

```csharp
// Banking.Infrastructure/DependencyInjection.cs — binds interface → implementation
services.AddScoped<IAccountRepository, AccountRepository>();
services.AddScoped<ICustomerRepository, CustomerRepository>();
```

This is **Dependency Inversion**: high-level policy (handlers) defines abstractions; low-level detail (SQL) implements them.

### 3.2 What lives where (concrete examples)

**Domain** — pure business vocabulary:

- Customer, Account, Transaction, Transfer, AuditLogEntry
- Enums: AccountType, TransactionType, CustomerRole
- ConcurrencyException — thrown when ROWVERSION check fails

**Application** — one use case per handler:

- DepositCommand + DepositCommandHandler
- TransferCommand + TransferCommandHandler
- LoginQuery + LoginQueryHandler
- IAccountRepository (interface only)

**Infrastructure** — technology choices:

- AccountRepository — SqlCommand, SqlDataReader, manual mapping
- JwtTokenService, BcryptPasswordHasher, RefreshTokenService
- MockEmailService, MockPaymentGatewayService

**API** — HTTP adapter:

- AccountsController — maps routes to handlers
- ExceptionHandlingMiddleware — maps ConcurrencyException → HTTP 409
- IdempotencyFilter — caches POST responses by Idempotency-Key

---

## 4. Low-Level Design (LLD)

### 4.1 CQRS without MediatR

The application uses **Command Query Responsibility Segregation (CQRS)** in a lightweight form:

- **Commands** change state (deposit, withdraw, transfer, pay bill).
- **Queries** read state (list accounts, spending insights, admin search).

Each command/query is a small immutable record. A dedicated **handler** class exposes HandleAsync(...). Controllers inject handlers directly — there is no MediatR bus. This keeps the indirection low while still giving each use case a single, testable class.

```
AccountsController
    └── DepositCommandHandler.HandleAsync(DepositCommand)
            ├── IAccountRepository.GetByIdAsync
            ├── IConnectionFactory → SqlTransaction
            ├── IAccountRepository.UpdateBalanceAsync
            ├── ITransactionRepository.InsertAsync
            └── IAuditLogRepository.AppendAsync
```

Handlers return Result<T> instead of throwing for expected failures (insufficient funds, account not found). Controllers translate ErrorCode to HTTP status codes.

### 4.2 Repository pattern (ADO.NET)

Repositories hide SQL behind interfaces defined in Application:

```csharp
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct);
    Task UpdateBalanceAsync(
        Guid accountId, decimal newBalance, byte[] rowVersion,
        DbTransaction? dbTransaction = null, CancellationToken ct = default);
}
```

Each method:

1. Opens a connection via IConnectionFactory (or reuses one from an active SqlTransaction).
2. Runs parameterized SQL — no string concatenation.
3. Maps SqlDataReader rows to Domain entities by hand.

For money movement, handlers **own the transaction boundary**. They call connection.BeginTransactionAsync(), pass the DbTransaction into repository methods, and Commit or Rollback. Multiple tables (balance + transaction + transfer + audit) stay consistent.

### 4.3 DTOs at the edges

Domain entities never cross the HTTP boundary. Handlers map to **DTOs** (Data Transfer Objects):

- DepositResponseDto — transaction ID, new balance, timestamp
- AuthResponseDto — JWT, refresh token, CustomerSummaryDto
- AdminCustomerDto — staff search results

This prevents leaking internal fields (e.g. RowVersion, password hashes) and lets the API evolve independently of the database schema.

---

## 5. Data Flows (End to End)

### 5.1 Login and session

**Sequence:**

1. Next.js /login → POST /api/auth/login { email, password }
2. AuthController → LoginQueryHandler
3. CustomerRepository.GetByEmailAsync → SQL SELECT Customer + Role
4. CustomerRepository.GetPasswordHashAsync → SQL SELECT CustomerAuth
5. BCrypt password verify
6. JwtTokenService.GenerateToken(customer)
7. RefreshTokenService.IssueAsync(customer) → INSERT RefreshToken
8. API returns { token, refreshToken, customer }
9. UI stores in localStorage + AuthContext

**Frontend path:** AuthContext.login() → authApi.login() → apiRequest() stores token and customer. Subsequent calls attach Authorization: Bearer <token>. Staff users get role: "Staff" in the JWT and customer object; the admin route checks this before rendering.

**Refresh flow:** POST /api/auth/refresh validates the refresh token hash, revokes the old token (rotation), issues a new access + refresh pair.

### 5.2 Deposit (write path with transaction)

**Sequence:**

1. Account detail page → POST /deposits + Idempotency-Key header
2. IdempotencyFilter checks cache
   - **Cache hit** → return cached JSON (no double credit)
   - **Cache miss** → continue below
3. DepositCommandHandler receives DepositCommand
4. AccountRepository.GetByIdAsync (read balance + RowVersion)
5. BEGIN SQL TRANSACTION
6. AccountRepository.UpdateBalanceAsync (WHERE RowVersion = @rv)
7. TransactionRepository.InsertAsync
8. AuditLogRepository.AppendAsync
9. COMMIT
10. Store idempotency record → return 200 OK

**Key design points:**

- **Optimistic concurrency:** UpdateBalanceAsync includes WHERE RowVersion = @RowVersion. If another request updated the account first, zero rows match → ConcurrencyException → HTTP 409 Conflict.
- **Idempotency:** Retrying the same POST with the same Idempotency-Key returns the stored response without re-running the handler.
- **Audit:** Every successful deposit writes an AuditLog row inside the same SQL transaction.

### 5.3 Transfer (multi-leg atomic write)

A transfer touches two accounts, two transaction rows, one transfer record, and an audit entry — all in **one SQL transaction**:

1. Validate both accounts belong to the customer and are active.
2. Check sufficient balance on the source account.
3. UpdateBalanceAsync on source (debit) and destination (credit).
4. Insert debit + credit Transaction rows linked by ReferenceId = Transfer.Id.
5. Insert Transfer row with human-readable reference (TRF-YYYYMMDD-…).
6. Append audit log; commit.
7. After commit, send transfer receipt email via IEmailService (fire-and-forget side effect).

If any step fails, the entire transaction rolls back — no partial transfers.

### 5.4 Admin customer lookup (RBAC read path)

**Customer search:**

GET /api/admin/customers?search=
→ Is JWT role = Staff?
   → No → 403 Forbidden
   → Yes → SearchCustomersQueryHandler
           → CustomerRepository.SearchAsync
           → Customer table

**Customer accounts:**

GET /api/admin/customers/{id}/accounts
→ GetCustomerAccountsAdminQueryHandler
→ AccountRepository.GetByCustomerIdAsync

[Authorize(Roles = "Staff")] on AdminController enforces RBAC at the API layer. The JWT carries ClaimTypes.Role set during token generation in JwtTokenService.

### 5.5 Frontend data flow

```
Page (React)
  → useAuth() for token
  → bankingApi.payments.submitTransfer(token, request)
    → apiRequest("/api/transfers", { method: "POST", idempotencyKey, token })
      → fetch(API_BASE + path, { headers: { Authorization, Idempotency-Key } })
  → JSON response → toast + re-fetch account data
```

The **bankingApi facade** (shared/api/index.ts) groups domain modules (accounts, payments, insights, …). Pages never import fetch directly — they go through the facade, which keeps swap points in one place.

**Workspace shell:** WorkspaceShell → useRequireAuth() redirects unauthenticated users to /login. Sidebar reads isStaff from AuthContext to show the admin link.

---

## 6. Design Patterns in Practice

| Pattern | Where | How it helps |
| --- | --- | --- |
| Clean Architecture / Layered | Whole backend | Domain stays testable; SQL is replaceable |
| Dependency Inversion (DIP) | I*Repository in Application, impl in Infrastructure | Handlers don't know about ADO.NET |
| CQRS | *Command / *Query + *Handler | Clear separation of reads vs writes |
| Repository | AccountRepository, TransactionRepository, … | Encapsulates SQL; single place per aggregate |
| Unit of Work (manual) | Handlers open SqlTransaction, pass to repos | Atomic multi-table writes |
| DTO | *ResponseDto, *Dto records | Stable API contract |
| Result object | Result<T> with ErrorCode | Expected failures without exceptions |
| Strategy (implicit) | IEmailService, IPaymentGatewayService | Mock vs real provider swap |
| Middleware | ExceptionHandlingMiddleware | Cross-cutting error → HTTP mapping |
| Action Filter | IdempotencyFilter + [Idempotent] | Cross-cutting idempotency on POST endpoints |
| Facade | bankingApi on frontend | Single entry point for UI modules |
| Repository + Factory | IConnectionFactory → SqlConnectionFactory | Centralized connection string / open logic |
| Optimistic concurrency | Account.RowVersion | Safe parallel balance updates |
| Token rotation | RefreshTokenService | Refresh tokens revoked on use |

### 6.1 Result pattern vs exceptions

**Use Result<T> for business outcomes the UI should handle:**

```csharp
return Result<WithdrawResponseDto>.Failure(
    "Insufficient funds for this withdrawal.",
    "INSUFFICIENT_FUNDS");
```

**Use exceptions for infrastructure / unexpected failures:**

- ConcurrencyException → caught by middleware → 409
- Unhandled exceptions → middleware → 500

Controllers pattern-match on ErrorCode:

```csharp
return result.ErrorCode switch
{
    "ACCOUNT_NOT_FOUND" => NotFound(...),
    "INSUFFICIENT_FUNDS" => BadRequest(...),
    _ => BadRequest(...)
};
```

### 6.2 Idempotency pattern

Money movement endpoints (deposits, withdrawals, transfers) are decorated with [Idempotent]. The filter:

1. Reads Idempotency-Key header (optional — if absent, normal flow).
2. Looks up (CustomerId, Key, RequestPath) in IdempotencyRecord.
3. On hit → returns cached body + status without executing the handler.
4. On miss → runs handler, stores successful response for future retries.

This implements **at-least-once delivery** safety for clients that retry on network failure.

---

## 7. Database Design

SQL Server holds all state in BankingDb. Schema is defined in scripts/init-db.sql.

**Core tables:**

| Table | Purpose |
| --- | --- |
| Customer | Identity, name, email, Role (Customer/Staff) |
| CustomerAuth | BCrypt password hash (separated from profile) |
| Account | Balance, type, RowVersion for concurrency |
| Transaction | Immutable ledger entries per account |
| Transfer | Transfer metadata linking two legs |

**Sprint 3+ tables:** CreditCard, InvestmentHolding, BillPayment, ScheduledPayment, CustomerSettings

**Sprint 4 tables:** IdempotencyRecord, RefreshToken, AuditLog

Relationships follow standard banking modeling: a Customer owns many Accounts; each Account has many Transactions; a Transfer references two Accounts and generates two Transaction rows.

---

## 8. Cross-Cutting Concerns

| Concern | Implementation |
| --- | --- |
| Authentication | JWT bearer; sub claim = customer ID |
| Authorization | [Authorize] on controllers; [Authorize(Roles = "Staff")] for admin |
| CORS | Policy allowing http://localhost:3000 |
| Errors | { error, code } JSON; global middleware |
| Concurrency | ROWVERSION on Account; 409 on conflict |
| Idempotency | Header + DB cache |
| Audit | Append-only AuditLog on money movement |
| Testing | Banking.Api.IntegrationTests — WebApplicationFactory + real SQL |

---

## 9. Project Layout (Quick Reference)

```
banking/
├── src/
│   ├── Banking.Domain/           # Entities, enums, exceptions
│   ├── Banking.Application/      # Handlers, DTOs, abstractions
│   ├── Banking.Infrastructure/   # Repositories, JWT, mocks
│   ├── Banking.Api/              # Controllers, middleware, filters
│   └── banking-web/              # Next.js UI
├── scripts/init-db.sql           # Schema + seed data
├── tests/Banking.Api.IntegrationTests/
└── docs/Architecture.md          # Living design doc
```

---

## 10. Summary

The banking app is structured so that **business rules live in Application handlers**, **business vocabulary lives in Domain**, and **SQL/JWT/BCrypt live in Infrastructure**. The API and Next.js UI are thin adapters on either side.

Data flows in one direction for writes:

**Browser → Controller → Handler → Repository → SQL → DTO → JSON → React**

…and the reverse for reads, without ever exposing domain entities or password hashes over HTTP.

The main patterns — CQRS handlers, repository + manual unit of work, dependency inversion, Result objects, idempotency filters, and optimistic concurrency — are not academic extras. Each one solves a concrete problem in a system that moves money: atomicity, safe retries, conflict detection, auditability, and swappable infrastructure.

For sprint-by-sprint delivery detail, see docs/Architecture.md and docs/sprints/sprint1.md through sprint4.md.
