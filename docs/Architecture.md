# Architecture — Banking Application

Minimal high-level and low-level design for the MVP → production evolution.  
**Stack:** C# (.NET 8 Web API) · React (Vite + TypeScript) · SQL Server · ADO.NET

---

## 1. High-Level Design (HLD)

### 1.1 System Context

```
┌─────────────┐         HTTPS/JSON          ┌──────────────────┐
│   Browser   │ ◄──────────────────────────►│  Banking.Api     │
│  (React)    │                             │  (ASP.NET Core)  │
└─────────────┘                             └────────┬─────────┘
                                                     │
                    ┌────────────────────────────────┼────────────────────────┐
                    │                                │                        │
                    ▼                                ▼                        ▼
            ┌───────────────┐              ┌─────────────────┐      ┌─────────────────┐
            │  SQL Server   │              │  Mock Email     │      │  Mock KYC       │
            │  (ADO.NET)    │              │  (console/log)  │      │  (always pass)  │
            └───────────────┘              └─────────────────┘      └─────────────────┘
```

### 1.2 Layered Architecture (Clean Architecture)

```
┌─────────────────────────────────────────────────────────────┐
│  Presentation                                               │
│  • Banking.Api (controllers, middleware, auth)              │
│  • banking-web (React UI)                                   │
├─────────────────────────────────────────────────────────────┤
│  Application (use cases)                                    │
│  • Commands / Queries / Handlers                            │
│  • DTOs, validators, application service interfaces         │
├─────────────────────────────────────────────────────────────┤
│  Domain                                                     │
│  • Customer, Account, Transaction entities                  │
│  • Domain rules (e.g., insufficient funds)                  │
│  • Domain exceptions                                        │
├─────────────────────────────────────────────────────────────┤
│  Infrastructure                                             │
│  • ADO.NET repositories                                     │
│  • JwtTokenService, PasswordHasher                          │
│  • MockEmailService, MockKycService                         │
└─────────────────────────────────────────────────────────────┘

Dependency rule: outer layers depend on inner; Domain has zero dependencies.
```

### 1.3 MVP Scope (Sprint 1–3 rollup)

| Capability | Sprint 1 | Sprint 2 | Sprint 3 |
|---|---|---|---|
| Register / Login | ✓ | harden | RBAC |
| View accounts & balance | ✓ | — | — |
| Deposit / Withdraw | ✓ | audit | idempotency |
| Internal transfer | ✓ | notifications | concurrency |
| Transaction history | ✓ | pagination | export |
| Admin / staff views | — | mock | ✓ |
| External payment | — | mock gateway | stub API |

### 1.4 Key Flows

**Login**
1. React POST `/api/auth/login` with credentials  
2. Api → LoginQuery handler → CustomerRepository (ADO.NET)  
3. Verify password hash → issue JWT  
4. React stores token; attaches `Authorization` header  

**Transfer (critical path)**
1. React POST `/api/transfers` with `{ fromAccountId, toAccountId, amount }`  
2. TransferCommandHandler opens SQL transaction  
3. Lock/read both accounts; validate ownership and balance  
4. Debit source, credit destination; insert two Transaction rows + Transfer record  
5. Commit; return transfer receipt DTO  

---

## 2. Low-Level Design (LLD)

### 2.1 Backend Projects

| Project | Responsibility |
|---|---|
| `Banking.Domain` | Entities, enums (`AccountType`), `InsufficientFundsException` |
| `Banking.Application` | CQRS handlers, `ICustomerRepository`, `IAccountRepository`, `ITransactionRepository`, DTOs |
| `Banking.Infrastructure` | ADO.NET repos, `SqlConnectionFactory`, JWT, mocks |
| `Banking.Api` | REST endpoints, DI composition root, exception middleware |

### 2.2 CQRS Mapping (Sprint 1 — manual dispatch)

Sprint 1 may use explicit handler injection in controllers. MediatR optional in Sprint 2.

**Commands (writes)**
| Command | Handler responsibility |
|---|---|
| `RegisterCustomerCommand` | Hash password, insert Customer |
| `CreateAccountCommand` | Create account with zero balance for customer |
| `DepositCommand` | Credit account, append Transaction |
| `WithdrawCommand` | Validate balance, debit, append Transaction |
| `TransferCommand` | Atomic dual-leg update + Transfer record |

**Queries (reads)**
| Query | Returns |
|---|---|
| `LoginQuery` | Auth token + customer summary |
| `GetCustomerAccountsQuery` | List of account DTOs |
| `GetAccountByIdQuery` | Account detail (owner-scoped) |
| `GetAccountTransactionsQuery` | Paginated transaction list |

### 2.3 API Endpoints (MVP)

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | No | Register customer |
| POST | `/api/auth/login` | No | Login, return JWT |
| GET | `/api/accounts` | Yes | List current user's accounts |
| POST | `/api/accounts` | Yes | Open new account |
| GET | `/api/accounts/{id}` | Yes | Account detail |
| GET | `/api/accounts/{id}/transactions` | Yes | History |
| POST | `/api/accounts/{id}/deposits` | Yes | Deposit |
| POST | `/api/accounts/{id}/withdrawals` | Yes | Withdraw |
| POST | `/api/transfers` | Yes | Transfer between accounts |
| GET | `/api/health` | No | Health check (Sprint 3) |

### 2.4 Application — Interface Sketch

```csharp
// Banking.Application — interfaces only (implementations in Infrastructure)
public interface ICustomerRepository
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Guid> CreateAsync(Customer customer, string passwordHash, CancellationToken ct);
}

public interface IAccountRepository
{
    Task<IReadOnlyList<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct);
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct);
    Task<Guid> CreateAsync(Account account, CancellationToken ct);
    Task UpdateBalanceAsync(Guid accountId, decimal newBalance, byte[] rowVersion, CancellationToken ct);
}

public interface ITransactionRepository
{
    Task InsertAsync(Transaction transaction, CancellationToken ct);
    Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, int skip, int take, CancellationToken ct);
}
```

### 2.5 Infrastructure — ADO.NET Pattern

```csharp
// Pseudocode — each repository method:
// 1. Open connection via IConnectionFactory
// 2. SqlCommand with CommandType.Text and @Parameters
// 3. Map SqlDataReader → entity manually
// 4. For writes, participate in optional SqlTransaction passed from handler
```

**Connection factory:** reads connection string from `IConfiguration`; single database `BankingDb` for MVP.

### 2.6 Cross-Cutting Concerns

| Concern | Sprint 1 | Later |
|---|---|---|
| Authentication | JWT bearer | Refresh tokens |
| Validation | Data annotations / manual | FluentValidation |
| Errors | Middleware → `{ error, code }` JSON | ProblemDetails RFC 7807 |
| Logging | `ILogger` | Serilog + sinks |
| Concurrency | Last-write-wins (document risk) | `rowversion` column |

### 2.7 Frontend Structure

```
banking-web/src/
├── app/                 # Router, providers (auth)
├── features/
│   ├── auth/            # Login, Register pages
│   ├── accounts/        # List, detail, create
│   └── transfers/       # Transfer form, history
├── shared/
│   ├── api/             # axios/fetch client, auth interceptor
│   ├── components/      # Button, Input, Layout
│   └── types/           # TS interfaces mirroring API DTOs
└── main.tsx
```

---

## 3. Database Schema

**RDBMS:** SQL Server (LocalDB or Docker for dev)  
**Naming:** PascalCase tables, singular entity names, `Id` as `UNIQUEIDENTIFIER` PK  

### 3.1 ER Diagram (Logical)

```
┌──────────────┐       ┌──────────────┐       ┌────────────────┐
│   Customer   │       │   Account    │       │  Transaction   │
├──────────────┤       ├──────────────┤       ├────────────────┤
│ Id (PK)      │──┐    │ Id (PK)      │──┐    │ Id (PK)        │
│ Email (UQ)   │  └───►│ CustomerId   │  └───►│ AccountId      │
│ FirstName    │       │ AccountNumber│       │ Type           │
│ LastName     │       │ AccountType  │       │ Amount         │
│ CreatedAt    │       │ Balance      │       │ BalanceAfter   │
└──────────────┘       │ Currency     │       │ Description    │
                       │ Status       │       │ CreatedAt      │
                       │ CreatedAt    │       └────────────────┘
                       │ RowVersion   │
                       └──────┬───────┘
                              │
                       ┌──────▼───────┐
                       │   Transfer   │
                       ├──────────────┤
                       │ Id (PK)      │
                       │ FromAccountId│
                       │ ToAccountId  │
                       │ Amount       │
                       │ Status       │
                       │ CreatedAt    │
                       │ Reference    │
                       └──────────────┘

┌──────────────┐
│ CustomerAuth │
├──────────────┤
│ CustomerId   │ (PK, FK → Customer)
│ PasswordHash │
│ UpdatedAt    │
└──────────────┘
```

### 3.2 Table Definitions

#### Customer
| Column | Type | Constraints |
|---|---|---|
| Id | UNIQUEIDENTIFIER | PK, DEFAULT NEWID() |
| Email | NVARCHAR(256) | NOT NULL, UNIQUE |
| FirstName | NVARCHAR(100) | NOT NULL |
| LastName | NVARCHAR(100) | NOT NULL |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT SYSUTCDATETIME() |

#### CustomerAuth
| Column | Type | Constraints |
|---|---|---|
| CustomerId | UNIQUEIDENTIFIER | PK, FK → Customer.Id |
| PasswordHash | NVARCHAR(500) | NOT NULL |
| UpdatedAt | DATETIME2 | NOT NULL |

#### Account
| Column | Type | Constraints |
|---|---|---|
| Id | UNIQUEIDENTIFIER | PK |
| CustomerId | UNIQUEIDENTIFIER | FK → Customer.Id |
| AccountNumber | NVARCHAR(20) | NOT NULL, UNIQUE |
| AccountType | TINYINT | NOT NULL (1=Checking, 2=Savings) |
| Balance | DECIMAL(18,2) | NOT NULL, DEFAULT 0, CHECK (Balance >= 0) |
| Currency | CHAR(3) | NOT NULL, DEFAULT 'USD' |
| Status | TINYINT | NOT NULL, DEFAULT 1 (1=Active, 2=Closed) |
| CreatedAt | DATETIME2 | NOT NULL |
| RowVersion | ROWVERSION | Optimistic concurrency (use in Sprint 3) |

#### Transaction
| Column | Type | Constraints |
|---|---|---|
| Id | UNIQUEIDENTIFIER | PK |
| AccountId | UNIQUEIDENTIFIER | FK → Account.Id |
| Type | TINYINT | NOT NULL (1=Credit, 2=Debit) |
| Amount | DECIMAL(18,2) | NOT NULL, CHECK (Amount > 0) |
| BalanceAfter | DECIMAL(18,2) | NOT NULL |
| Description | NVARCHAR(500) | NULL |
| ReferenceId | UNIQUEIDENTIFIER | NULL (Transfer Id if applicable) |
| CreatedAt | DATETIME2 | NOT NULL |

**Index:** `IX_Transaction_AccountId_CreatedAt` ON (AccountId, CreatedAt DESC)

#### Transfer
| Column | Type | Constraints |
|---|---|---|
| Id | UNIQUEIDENTIFIER | PK |
| FromAccountId | UNIQUEIDENTIFIER | FK → Account.Id |
| ToAccountId | UNIQUEIDENTIFIER | FK → Account.Id |
| Amount | DECIMAL(18,2) | NOT NULL, CHECK (Amount > 0) |
| Status | TINYINT | NOT NULL (1=Completed, 2=Failed, 3=Pending) |
| Reference | NVARCHAR(50) | NOT NULL, UNIQUE (human-readable ref) |
| CreatedAt | DATETIME2 | NOT NULL |

**Constraint:** `FromAccountId <> ToAccountId`

### 3.3 Seed Data (Dev Only)

- One customer: `demo@bank.local` / password documented in README (not committed)
- Two accounts: Checking ($1,000), Savings ($500)

### 3.4 SQL Init Script Location

`scripts/init-db.sql` — creates database, tables, indexes, seed data.

---

## 4. Non-Functional Requirements (Progressive)

| NFR | MVP | Target |
|---|---|---|
| API response (read) | < 500ms local | < 200ms |
| Transfer integrity | SQL transaction | + idempotency key |
| Availability | Single instance | Health checks |
| Audit | Transaction log | + Audit table Sprint 3 |

---

## 5. Technology Choices & Rationale

| Choice | Why |
|---|---|
| ADO.NET | Learning goal; explicit SQL control |
| SQL Server | Familiar tooling; ROWVERSION support |
| JWT | Stateless API auth for SPA |
| React + Vite | Fast dev feedback, TS safety |
| No EF Core | Avoid hiding SQL; intentional trade-off |

---

## 6. Open Decisions (Resolve During Sprints)

- [ ] MediatR vs manual handler registry (Sprint 2)
- [ ] Minimal APIs vs controllers (pick one in Sprint 1, stick to it)
- [ ] axios vs fetch for React HTTP client
- [ ] Account number generation strategy (sequential vs UUID-based display)

Document decisions in sprint retrospectives.
