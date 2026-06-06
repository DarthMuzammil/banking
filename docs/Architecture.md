# Architecture — Banking Application

Minimal high-level and low-level design for the MVP → production evolution.  
**Stack:** C# (.NET 9 Web API) · Next.js 16 (App Router) + TypeScript + Tailwind · SQL Server · ADO.NET

**Sprint docs:** [Sprint 1](sprints/sprint1.md) · [Sprint 2](sprints/sprint2.md) · [Sprint 3](sprints/sprint3.md) · [Sprint 4](sprints/sprint4.md)

---

## 1. High-Level Design (HLD)

### 1.1 System Context

```
┌─────────────┐         HTTPS/JSON          ┌──────────────────┐
│   Browser   │ ◄──────────────────────────►│  Banking.Api     │
│  (Next.js)  │         CORS :3000          │  (ASP.NET Core)  │
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
│  • banking-web (Next.js App Router UI)                      │
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

### 1.3 Capability Roadmap (Sprint 1–4)

| Capability | Sprint 1 | Sprint 2 | Sprint 3 | Sprint 4 |
|---|---|---|---|---|
| Register / Login | ✅ | harden | settings API | refresh tokens |
| View accounts & balance | ✅ | — | — | — |
| Deposit | ✅ | — | — | idempotency |
| Withdraw | — | ✅ | — | audit |
| Internal transfer | UI mock | ✅ | — | idempotency |
| Per-account transaction history | ✅ | — | categories (optional) | export API |
| Unified transaction search | UI client-side | ✅ `GET /api/transactions` | — | — |
| Spending insights | UI mock | — | ✅ | — |
| Credit cards | UI mock | — | ✅ | — |
| Investments | UI mock | — | ✅ | — |
| Bill / scheduled payments | UI mock | — | ✅ | mock gateway |
| User settings | UI mock | — | ✅ | — |
| Admin / staff views | — | — | — | ✅ |
| Exception middleware | — | ✅ | — | ProblemDetails |
| Integration tests | — | ✅ | expand | CI |

### 1.4 Frontend ↔ Backend Integration Map

| UI route | Module | API source | Backend endpoint | Status |
|---|---|---|---|---|
| `/login`, `/register` | Auth | Real | `POST /api/auth/*` | ✅ Implemented |
| `/dashboard` | Overview | Mixed | `GET /api/accounts`, per-account transactions | ✅ Accounts; insights mock |
| `/accounts` | Accounts | Real | `GET/POST /api/accounts` | ✅ Implemented |
| `/accounts/{id}` | Account detail | Real | `GET /api/accounts/{id}`, deposits, transactions | ✅ Implemented |
| `/transactions` | Transactions | Client aggregate | `GET /api/accounts/{id}/transactions` (N calls) | 🟡 Sprint 2 unified API |
| `/payments` | Transfer | Mock | `POST /api/transfers` | ⏳ Sprint 2 |
| `/payments` | Bills / scheduled | Mock | `GET/POST /api/payments/*` | ⏳ Sprint 3 |
| `/credit-cards` | Credit cards | Mock | `GET /api/credit-cards` | ⏳ Sprint 3 |
| `/investments` | Portfolio | Mock | `GET /api/investments/portfolio` | ⏳ Sprint 3 |
| `/settings` | Preferences | Mock | `GET/PATCH /api/settings` | ⏳ Sprint 3 |

**Swap layer:** `src/banking-web/src/shared/api/index.ts` exports `bankingApi` — each domain service in `shared/api/services/` can switch from mock to real without page changes.

### 1.5 Key Flows

**Login**
1. Next.js POST `/api/auth/login` with credentials  
2. Api → Login handler → CustomerRepository (ADO.NET)  
3. Verify password hash → issue JWT  
4. Web stores token; `bankingApi` attaches `Authorization` header  

**Deposit (implemented)**
1. POST `/api/accounts/{id}/deposits` with `{ amount, description? }`  
2. DepositCommandHandler opens SQL transaction  
3. Credit account; insert Transaction row  
4. Commit; return updated balance  

**Transfer (Sprint 2)**
1. POST `/api/transfers` with `{ fromAccountId, toAccountId, amount }`  
2. TransferCommandHandler opens SQL transaction  
3. Lock/read both accounts; validate ownership and balance  
4. Debit source, credit destination; insert two Transaction rows + Transfer record  
5. Commit; return transfer receipt DTO  
6. Payments UI calls real API (replaces mock `submitTransfer`)

---

## 2. Low-Level Design (LLD)

### 2.1 Backend Projects

| Project | Responsibility |
|---|---|
| `Banking.Domain` | Entities, enums (`AccountType`), `InsufficientFundsException` |
| `Banking.Application` | CQRS handlers, repository interfaces, DTOs |
| `Banking.Infrastructure` | ADO.NET repos, `SqlConnectionFactory`, JWT, BCrypt, mocks |
| `Banking.Api` | REST endpoints, DI composition root, CORS, exception middleware (Sprint 2) |

### 2.2 CQRS Mapping

Sprint 1 uses explicit handler injection in controllers. MediatR optional in Sprint 2.

**Commands (writes)**
| Command | Handler responsibility | Status |
|---|---|---|
| `RegisterCustomerCommand` | Hash password, insert Customer | ✅ |
| `CreateAccountCommand` | Create account with zero balance | ✅ |
| `DepositCommand` | Credit account, append Transaction | ✅ |
| `WithdrawCommand` | Validate balance, debit, append Transaction | Sprint 2 |
| `TransferCommand` | Atomic dual-leg update + Transfer record | Sprint 2 |
| `PayBillCommand` | Debit account, mock external pay | Sprint 3 |

**Queries (reads)**
| Query | Returns | Status |
|---|---|---|
| `LoginQuery` | Auth token + customer summary | ✅ |
| `GetCustomerAccountsQuery` | List of account DTOs | ✅ |
| `GetAccountByIdQuery` | Account detail (owner-scoped) | ✅ |
| `GetAccountTransactionsQuery` | Paginated transaction list | ✅ |
| `GetCustomerTransactionsQuery` | Cross-account history + filters | Sprint 2 |
| `GetSpendingInsightsQuery` | Category aggregates | Sprint 3 |
| `GetCreditCardsQuery` | Card list for customer | Sprint 3 |
| `GetInvestmentPortfolioQuery` | Holdings summary | Sprint 3 |

### 2.3 API Endpoints

| Method | Route | Auth | Description | Status |
|---|---|---|---|---|
| POST | `/api/auth/register` | No | Register customer | ✅ |
| POST | `/api/auth/login` | No | Login, return JWT | ✅ |
| GET | `/api/accounts` | Yes | List current user's accounts | ✅ |
| POST | `/api/accounts` | Yes | Open new account | ✅ |
| GET | `/api/accounts/{id}` | Yes | Account detail | ✅ |
| GET | `/api/accounts/{id}/transactions` | Yes | Per-account history | ✅ |
| POST | `/api/accounts/{id}/deposits` | Yes | Deposit | ✅ |
| POST | `/api/accounts/{id}/withdrawals` | Yes | Withdraw | Sprint 2 |
| POST | `/api/transfers` | Yes | Transfer between accounts | Sprint 2 |
| GET | `/api/transactions` | Yes | Unified history (`?search&accountId&skip&take`) | Sprint 2 |
| GET | `/api/insights/spending` | Yes | Dashboard spending breakdown | Sprint 3 |
| GET | `/api/credit-cards` | Yes | Credit card list | Sprint 3 |
| GET | `/api/investments/portfolio` | Yes | Investment holdings | Sprint 3 |
| GET | `/api/payments/bills` | Yes | Due bills | Sprint 3 |
| POST | `/api/payments/bills/{id}/pay` | Yes | Pay bill | Sprint 3 |
| GET/POST | `/api/payments/scheduled` | Yes | Scheduled payments | Sprint 3 |
| GET/PATCH | `/api/settings` | Yes | User preferences | Sprint 3 |
| GET | `/api/health` | No | Health check | ✅ |

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
    // Sprint 2:
  // Task<IReadOnlyList<Transaction>> GetByCustomerIdAsync(Guid customerId, TransactionFilter filter, CancellationToken ct);
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

| Concern | Sprint 1 | Sprint 2+ |
|---|---|---|
| Authentication | JWT bearer | Refresh tokens (Sprint 4) |
| CORS | `localhost:3000` | Env-based origins |
| Validation | Data annotations / manual | FluentValidation |
| Errors | Controller-level | Global middleware → `{ error, code }` |
| Logging | `ILogger` | Serilog + sinks (Sprint 4) |
| Concurrency | Last-write-wins (document risk) | `ROWVERSION` enforcement (Sprint 4) |

### 2.7 Frontend Structure (actual)

```
banking-web/src/
├── app/
│   ├── (auth)/              # login, register
│   ├── (workspace)/         # dashboard, accounts, transactions, payments, …
│   ├── globals.css          # dark design tokens
│   └── layout.tsx
├── features/
│   ├── auth/
│   ├── accounts/
│   ├── dashboard/
│   ├── transactions/
│   ├── payments/
│   ├── credit-cards/
│   ├── investments/
│   └── settings/
└── shared/
    ├── api/
    │   ├── index.ts         # bankingApi facade (mock/real per domain)
    │   ├── client.ts        # fetch + JWT
    │   ├── accounts.ts      # real
    │   ├── auth.ts          # real
    │   ├── types/           # DTO mirrors + extended.ts for mock-only shapes
    │   ├── services/        # insights, credit-cards, investments, payments, settings, transactions
    │   └── mocks/           # data.ts — remove in Sprint 3
    ├── components/
    │   ├── ui/              # Button, Card, Input, Badge, …
    │   └── layout/          # Sidebar, WorkspaceShell
    └── hooks/               # useAsync, auth context
```

**Design:** Notion-inspired dark-only UI; typography-led hierarchy; workspace sidebar with grouped nav (Overview, Banking, Products, Account).

---

## 3. Database Schema

**RDBMS:** SQL Server (LocalDB or Docker for dev)  
**Naming:** PascalCase tables, singular entity names, `Id` as `UNIQUEIDENTIFIER` PK  

### 3.1 ER Diagram (Logical — Sprint 1 core)

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
                       │ CreatedAt    │       │ Category (S3)  │
                       │ RowVersion   │       └────────────────┘
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

### 3.2 Table Definitions (Sprint 1 — implemented)

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
| RowVersion | ROWVERSION | Optimistic concurrency (enforce Sprint 4) |

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
| Category | TINYINT | NULL — Sprint 3 optional |

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
**Status:** Table exists in schema; API not wired until Sprint 2.

### 3.3 Planned Tables (Sprint 3)

| Table | Purpose |
|---|---|
| `CreditCard` | Synthetic card products per customer |
| `InvestmentHolding` | Symbol, shares, cost basis |
| `BillPayment` | Due bills linked to payee |
| `ScheduledPayment` | Recurring / future-dated payments |
| `CustomerSettings` | Notification toggles, display prefs |

Full DDL to be added in `scripts/migrations/` when Sprint 3 starts.

### 3.4 Seed Data (Dev Only)

- One customer: `demo@bank.local` / `Demo123!` (see README)
- Two accounts: Checking ($1,000), Savings ($500)
- Sprint 3: seed credit card, holdings, sample bills for demo user

### 3.5 SQL Init Script Location

`scripts/init-db.sql` — creates database, tables, indexes, seed data.

---

## 4. Non-Functional Requirements (Progressive)

| NFR | Sprint 1 | Sprint 2 | Sprint 4 target |
|---|---|---|---|
| API response (read) | < 500ms local | unified transactions | < 200ms |
| Transfer integrity | — | SQL transaction | + idempotency key |
| Availability | Single instance | health check ✅ | Docker Compose |
| Audit | Transaction log | — | Audit table |
| Frontend mock debt | 5 modules on mocks | transfer wired | zero mocks |

---

## 5. Technology Choices & Rationale

| Choice | Why |
|---|---|
| ADO.NET | Learning goal; explicit SQL control |
| SQL Server | Familiar tooling; ROWVERSION support |
| JWT | Stateless API auth for SPA |
| Next.js + Tailwind | App Router, SSR-ready, fast iteration; replaced original Vite plan |
| Mock API facade | UI can ship ahead of backend; swap per domain |
| No EF Core | Avoid hiding SQL; intentional trade-off |

---

## 6. Open Decisions (Resolve During Sprints)

- [ ] MediatR vs manual handler registry (Sprint 2)
- [x] Minimal APIs vs controllers → **Controllers** (Sprint 1)
- [x] HTTP client → **fetch** in `shared/api/client.ts`
- [ ] Account number generation strategy (sequential vs UUID-based display)
- [ ] Transaction categories: DB column vs server-side inference from description
- [ ] Credit/investment data: synthetic seed vs mock external provider interface

Document decisions in sprint retrospectives (`docs/tech-debt.md`).
