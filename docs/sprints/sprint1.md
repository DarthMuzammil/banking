# Sprint 1 — Foundation & Core Banking Loop

**Duration:** 1 week (adjust to your pace)  
**Theme:** *Move fast, learn the layers, ship a working vertical slice*  
**Phase:** 0 — "Move Fast" (see `prompt.md`)

---

## Sprint Goal

By the end of Sprint 1, a developer can **register, log in, create an account, deposit money, and view balance and transaction history** through a React UI backed by a C# API using ADO.NET against SQL Server.

> **Success statement:** Demo the full loop in under 3 minutes on a clean machine with only connection string + seed script configured.

---

## Out of Scope (Defer to Sprint 2+)

- Transfers between accounts (Sprint 2 — designed in Architecture, not built yet)
- MediatR / full CQRS pipeline (manual handlers OK)
- Refresh tokens, email verification, KYC
- Unit/integration test project (optional stretch)
- CI/CD, Docker, production deployment
- Admin dashboard, reporting, exports
- FluentValidation, AutoMapper, EF Core

---

## User Stories

| ID | As a… | I want to… | So that… | Priority |
|---|---|---|---|---|
| US-1 | Developer | Have a runnable solution skeleton | I can add features incrementally | P0 |
| US-2 | Customer | Register with email and password | I can access the bank | P0 |
| US-3 | Customer | Log in and receive a token | I can call secured APIs | P0 |
| US-4 | Customer | Open a checking or savings account | I have somewhere to store money | P0 |
| US-5 | Customer | Deposit funds into my account | I can increase my balance | P0 |
| US-6 | Customer | See all my accounts and balances | I know my financial position | P0 |
| US-7 | Customer | View transaction history for an account | I can trace activity | P1 |
| US-8 | Developer | Have SQL init script + seed data | I can reset dev DB easily | P0 |

---

## Tasks Breakdown

### Day 1 — Project Bootstrap & Database

| # | Task | Layer | Est. | Done when |
|---|---|---|---|---|
| T1.1 | Create git repo structure (`src/`, `docs/`, `scripts/`) | — | 30m | Folders exist, README stub |
| T1.2 | Create `Banking.Domain` class library | Domain | 30m | Project builds |
| T1.3 | Create `Banking.Application` class library, reference Domain | Application | 30m | Project builds |
| T1.4 | Create `Banking.Infrastructure` class library, reference Application | Infrastructure | 30m | Project builds |
| T1.5 | Create `Banking.Api` Web API, wire DI for connection string | Api | 1h | `dotnet run` returns 200 on `/` or `/api/health` stub |
| T1.6 | Write `scripts/init-db.sql` (Customer, CustomerAuth, Account, Transaction tables) | DB | 1.5h | Script runs cleanly on fresh SQL Server |
| T1.7 | Add dev seed data (one customer, optional pre-created account) | DB | 30m | Query confirms seed rows |

**Learning focus:** Clean Architecture dependency direction; why Domain has no NuGet packages.

---

### Day 2 — Domain & Auth

| # | Task | Layer | Est. | Done when |
|---|---|---|---|---|
| T2.1 | Implement `Customer`, `Account`, `Transaction` entities + enums | Domain | 1h | Types reflect schema |
| T2.2 | Add `InsufficientFundsException` (used in Sprint 2 withdraw/transfer) | Domain | 15m | Exception type exists |
| T2.3 | Define `ICustomerRepository`, `RegisterCustomerCommand`, `LoginQuery` + DTOs | Application | 1h | Interfaces compile |
| T2.4 | Implement `CustomerRepository` with ADO.NET (GetByEmail, Create) | Infrastructure | 2h | Manual test via temporary endpoint or unit snippet |
| T2.5 | Implement password hashing service | Infrastructure | 45m | Hash/verify works |
| T2.6 | Implement `RegisterCustomerCommandHandler` + `LoginQueryHandler` | Application | 1.5h | Handlers return Result/DTO |
| T2.7 | Add `POST /api/auth/register` and `POST /api/auth/login` | Api | 1h | Postman/curl returns token on login |

**Learning focus:** CQRS separation (register = command, login = query); parameterized SQL.

---

### Day 3 — Accounts

| # | Task | Layer | Est. | Done when |
|---|---|---|---|---|
| T3.1 | Define `IAccountRepository` + account DTOs | Application | 45m | Interfaces compile |
| T3.2 | Implement `AccountRepository` (GetByCustomerId, GetById, Create) | Infrastructure | 2h | ADO.NET methods tested |
| T3.3 | Implement `CreateAccountCommandHandler` (generate account number) | Application | 1h | New account persisted |
| T3.4 | Implement `GetCustomerAccountsQueryHandler` | Application | 45m | Returns list for customer |
| T3.5 | Add JWT bearer authentication middleware | Api | 1.5h | `[Authorize]` blocks anonymous |
| T3.6 | Add `GET /api/accounts`, `POST /api/accounts`, `GET /api/accounts/{id}` | Api | 1h | Owner-scoped; 404 for others' accounts |

**Learning focus:** SOLID — repository interface in Application; JWT claims (`sub` = customerId).

---

### Day 4 — Deposits & Transactions

| # | Task | Layer | Est. | Done when |
|---|---|---|---|---|
| T4.1 | Define `ITransactionRepository` | Application | 30m | Interface compiles |
| T4.2 | Implement `TransactionRepository` (Insert, GetByAccountId paginated) | Infrastructure | 2h | SQL with ORDER BY CreatedAt DESC |
| T4.3 | Extend `AccountRepository.UpdateBalanceAsync` | Infrastructure | 1h | Balance updates atomically |
| T4.4 | Implement `DepositCommandHandler` (update balance + insert transaction in SQL transaction) | Application | 2h | Deposit reflected in DB |
| T4.5 | Implement `GetAccountTransactionsQueryHandler` | Application | 45m | Returns paginated DTOs |
| T4.6 | Add `POST /api/accounts/{id}/deposits`, `GET /api/accounts/{id}/transactions` | Api | 1h | Endpoints work with auth |

**Learning focus:** SQL transactions (`BEGIN TRAN` / `COMMIT`); immutable transaction log pattern.

---

### Day 5 — React Frontend

| # | Task | Layer | Est. | Done when |
|---|---|---|---|---|
| T5.1 | Scaffold `banking-web` with Vite + React + TypeScript | Web | 45m | `npm run dev` works |
| T5.2 | Add API client with auth header interceptor | Web | 1h | Token attached after login |
| T5.3 | Build Login + Register pages | Web | 2h | Can register and login |
| T5.4 | Build Dashboard — list accounts | Web | 1.5h | Shows balances from API |
| T5.5 | Build Create Account form (type selector) | Web | 1h | POST creates account |
| T5.6 | Build Account Detail — deposit form + transaction list | Web | 2h | Deposit updates UI after success |
| T5.7 | Basic layout, nav, logout | Web | 1h | Usable demo flow |

**Learning focus:** Feature folders; keep API logic out of presentational components.

---

### Day 6 — Integration & Hardening (Buffer)

| # | Task | Layer | Est. | Done when |
|---|---|---|---|---|
| T6.1 | Global exception middleware → consistent JSON errors | Api | 1h | Validation errors return 400 |
| T6.2 | Input validation on all endpoints (amount > 0, required fields) | Api | 1h | Bad input rejected |
| T6.3 | End-to-end manual test script in README | Docs | 30m | Steps documented |
| T6.4 | Fix bugs from demo rehearsal | All | 2h | Clean 3-minute demo |
| T6.5 | **Stretch:** Withdraw endpoint (same pattern as deposit) | All | 2h | Withdraw with balance check |

---

### Day 7 — Retrospective & Sprint 2 Prep

| # | Task | Est. | Done when |
|---|---|---|---|
| T7.1 | Demo to self/peer; note friction points | 30m | Retro notes |
| T7.2 | Document technical debt (list in README or `docs/tech-debt.md`) | 30m | At least 5 items logged |
| T7.3 | Draft Sprint 2 backlog: transfers, withdraw, MediatR?, mocks | 1h | Next sprint ready |

---

## Technical Debt to Expect (Intentional)

Log these during retro — do not fix in Sprint 1 unless blocking:

1. Handlers injected directly into controllers (no MediatR)
2. DTOs may duplicate entity fields
3. No integration tests
4. Last-write-wins on balance (no ROWVERSION yet)
5. Minimal error codes for client
6. No refresh token flow
7. Account number generation may be naive

---

## Acceptance Criteria (Sprint Done)

- [ ] All P0 user stories demonstrable through UI
- [ ] No raw SQL concatenation; all queries parameterized
- [ ] Passwords never stored plain text; not logged
- [ ] Unauthorized access to another customer's account returns 404/403
- [ ] Deposit increases balance and creates exactly one Transaction row
- [ ] `init-db.sql` reproduces schema from scratch
- [ ] `prompt.md` phase rules were followed (no EF, no over-abstraction)

---

## Demo Script (3 Minutes)

1. Run SQL init script; start API and React dev server  
2. Register new user `alice@example.com`  
3. Log in → land on dashboard (empty accounts)  
4. Create Checking account  
5. Deposit $250  
6. Show updated balance and transaction history entry  
7. Log out  

---

## Risks & Mitigations

| Risk | Mitigation |
|---|---|
| ADO.NET boilerplate slows progress | One repository fully done on Day 2; copy pattern |
| JWT setup confusion | Use Microsoft's official JWT bearer docs; minimal claims |
| Scope creep (transfers) | Strictly out of scope; note in PR/commit messages |
| SQL Server not installed | Docker `mssql` image or LocalDB; document in README |

---

## Definition of Ready (For Each Task)

- Previous dependency task marked done  
- Schema/API shape agreed in `Architecture.md`  
- AI sessions start with prompt template from `prompt.md`

---

## Sprint 2 Preview (Not Started)

- Internal transfers (atomic, SQL transaction)
- Withdraw with insufficient funds handling
- Mock email on successful deposit/transfer
- MediatR or custom dispatcher refactor
- `Banking.Application.Tests` for transfer and deposit rules
