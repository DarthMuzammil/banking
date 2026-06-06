# Sprint 1 — Foundation & Core Banking Loop

**Duration:** 1 week (adjust to your pace)  
**Theme:** *Move fast, learn the layers, ship a working vertical slice*  
**Phase:** 0 — "Move Fast" (see `prompt.md`)  
**Status:** ✅ **Core complete** — backend loop + full UI workspace shipped; Day 6 hardening deferred to Sprint 2

---

## Sprint Goal

By the end of Sprint 1, a developer can **register, log in, create an account, deposit money, and view balance and transaction history** through a **Next.js** UI backed by a C# API using ADO.NET against SQL Server.

The UI also exposes a broader **banking workspace** (payments, credit cards, investments, settings). Modules beyond the core loop use **mock APIs** until Sprint 2–3 back them with real endpoints (see [Frontend ↔ Backend Gap](#frontend--backend-gap)).

> **Success statement:** Demo the core loop in under 3 minutes on a clean machine with only connection string + seed script configured.

---

## What Shipped

| Area | Status |
|---|---|
| Solution skeleton + SQL schema | ✅ Done |
| Auth (register / login / JWT) | ✅ Done |
| Accounts (list, create, detail) | ✅ Done |
| Deposits + per-account transaction history | ✅ Done |
| Next.js frontend (dark mode, Notion-inspired) | ✅ Done |
| Dashboard, Accounts, Transactions pages (real API) | ✅ Done |
| Payments, Credit cards, Investments, Settings (mock API) | ✅ UI only |
| Withdraw, Transfer (backend) | ❌ Sprint 2 |
| Global exception middleware | ❌ Sprint 2 |
| Integration tests | ❌ Sprint 2+ |

---

## Frontend ↔ Backend Gap

| UI module | Route | Frontend | Backend | Target sprint |
|---|---|---|---|---|
| Auth | `/login`, `/register` | Real API | `POST /api/auth/*` | Sprint 1 ✅ |
| Overview | `/dashboard` | Real + mock insights | Accounts + transactions | Insights → Sprint 3 |
| Accounts | `/accounts`, `/accounts/{id}` | Real API | `GET/POST /api/accounts`, deposits | Sprint 1 ✅ |
| Transactions | `/transactions` | Client aggregation + filter | Per-account history only | Unified API → Sprint 2 |
| Payments — transfer | `/payments` | Mock | `POST /api/transfers` designed | Sprint 2 |
| Payments — bills / scheduled | `/payments` | Mock | Not designed | Sprint 3 |
| Credit cards | `/credit-cards` | Mock | Not designed | Sprint 3 |
| Investments | `/investments` | Mock | Not designed | Sprint 3 |
| Settings | `/settings` | Mock prefs | Profile from JWT only | Sprint 3 |
| Withdraw | — (no UI yet) | — | Planned in Architecture | Sprint 2 |

Swap path for each mock: `src/banking-web/src/shared/api/services/*.ts` → real implementation when endpoint exists.

---

## User Stories

| ID | As a… | I want to… | So that… | Priority | Status |
|---|---|---|---|---|---|
| US-1 | Developer | Have a runnable solution skeleton | I can add features incrementally | P0 | ✅ |
| US-2 | Customer | Register with email and password | I can access the bank | P0 | ✅ |
| US-3 | Customer | Log in and receive a token | I can call secured APIs | P0 | ✅ |
| US-4 | Customer | Open a checking or savings account | I have somewhere to store money | P0 | ✅ |
| US-5 | Customer | Deposit funds into my account | I can increase my balance | P0 | ✅ |
| US-6 | Customer | See all my accounts and balances | I know my financial position | P0 | ✅ |
| US-7 | Customer | View transaction history for an account | I can trace activity | P1 | ✅ |
| US-8 | Developer | Have SQL init script + seed data | I can reset dev DB easily | P0 | ✅ |
| US-9 | Customer | Browse a unified transactions view | I can search activity across accounts | P1 | 🟡 UI only (client-side) |
| US-10 | Customer | See spending insights on dashboard | I understand spending trends | P2 | 🟡 Mock |
| US-11 | Customer | Use a full banking workspace (nav, settings) | The app feels like a real product | P1 | ✅ UI / mock data |

---

## Tasks Breakdown

### Day 1 — Project Bootstrap & Database ✅

| # | Task | Layer | Est. | Done when | Status |
|---|---|---|---|---|---|
| T1.1 | Create repo structure (`src/`, `docs/`, `scripts/`) | — | 30m | Folders exist, README stub | ✅ |
| T1.2 | Create `Banking.Domain` class library | Domain | 30m | Project builds | ✅ |
| T1.3 | Create `Banking.Application`, reference Domain | Application | 30m | Project builds | ✅ |
| T1.4 | Create `Banking.Infrastructure`, reference Application | Infrastructure | 30m | Project builds | ✅ |
| T1.5 | Create `Banking.Api`, wire DI + health endpoint | Api | 1h | `GET /api/health` → 200 | ✅ |
| T1.6 | Write `scripts/init-db.sql` | DB | 1.5h | Script runs on fresh SQL Server | ✅ |
| T1.7 | Add dev seed data | DB | 30m | Seed rows queryable | ✅ |

---

### Day 2 — Domain & Auth ✅

| # | Task | Layer | Est. | Done when | Status |
|---|---|---|---|---|---|
| T2.1 | `Customer`, `Account`, `Transaction` entities + enums | Domain | 1h | Types reflect schema | ✅ |
| T2.2 | `InsufficientFundsException` | Domain | 15m | Exception type exists | ✅ |
| T2.3 | `ICustomerRepository`, auth commands/queries + DTOs | Application | 1h | Interfaces compile | ✅ |
| T2.4 | `CustomerRepository` (ADO.NET) | Infrastructure | 2h | Parameterized SQL | ✅ |
| T2.5 | Password hashing (BCrypt) | Infrastructure | 45m | Hash/verify works | ✅ |
| T2.6 | Register + Login handlers | Application | 1.5h | `Result`/DTO returned | ✅ |
| T2.7 | `POST /api/auth/register`, `POST /api/auth/login` | Api | 1h | Login returns JWT | ✅ |

---

### Day 3 — Accounts ✅

| # | Task | Layer | Est. | Done when | Status |
|---|---|---|---|---|---|
| T3.1 | `IAccountRepository` + account DTOs | Application | 45m | Interfaces compile | ✅ |
| T3.2 | `AccountRepository` (GetByCustomerId, GetById, Create) | Infrastructure | 2h | ADO.NET tested | ✅ |
| T3.3 | `CreateAccountCommandHandler` | Application | 1h | Account persisted | ✅ |
| T3.4 | `GetCustomerAccountsQueryHandler` | Application | 45m | Returns list | ✅ |
| T3.5 | JWT bearer middleware | Api | 1.5h | `[Authorize]` blocks anonymous | ✅ |
| T3.6 | `GET/POST /api/accounts`, `GET /api/accounts/{id}` | Api | 1h | Owner-scoped; 404 cross-customer | ✅ |

---

### Day 4 — Deposits & Transactions ✅

| # | Task | Layer | Est. | Done when | Status |
|---|---|---|---|---|---|
| T4.1 | `ITransactionRepository` | Application | 30m | Interface compiles | ✅ |
| T4.2 | `TransactionRepository` (Insert, GetByAccountId paginated) | Infrastructure | 2h | `ORDER BY CreatedAt DESC` | ✅ |
| T4.3 | `AccountRepository.UpdateBalanceAsync` | Infrastructure | 1h | Balance updates in SQL txn | ✅ |
| T4.4 | `DepositCommandHandler` | Application | 2h | Deposit atomic in DB | ✅ |
| T4.5 | `GetAccountTransactionsQueryHandler` | Application | 45m | Paginated DTOs | ✅ |
| T4.6 | `POST .../deposits`, `GET .../transactions` | Api | 1h | Endpoints work with auth | ✅ |

---

### Day 5 — Next.js Frontend ✅

> **Note:** Original plan specified Vite + React. Implemented as **Next.js 16 + Tailwind** (App Router). Same learning goals apply.

| # | Task | Layer | Est. | Done when | Status |
|---|---|---|---|---|---|
| T5.1 | Scaffold `banking-web` (Next.js + TypeScript + Tailwind) | Web | 45m | `npm run dev` works | ✅ |
| T5.2 | API client + JWT in `Authorization` header | Web | 1h | Token attached after login | ✅ |
| T5.3 | Login + Register pages | Web | 2h | Can register and login | ✅ |
| T5.4 | Dashboard — balances, accounts, recent activity | Web | 1.5h | Real API data | ✅ |
| T5.5 | Create Account form | Web | 1h | POST creates account | ✅ |
| T5.6 | Account Detail — deposit + transaction list | Web | 2h | Deposit updates UI | ✅ |
| T5.7 | Workspace layout, sidebar nav, logout | Web | 1h | Usable demo flow | ✅ |
| T5.8 | Mock API facade (`bankingApi` per domain) | Web | 1h | Swappable mock/real services | ✅ |
| T5.9 | Transactions page (search, filter, CSV export) | Web | 2h | Client-side aggregation | ✅ |
| T5.10 | Payments, Credit cards, Investments, Settings (mock) | Web | 3h | Pages render mock data | ✅ |
| T5.11 | Dark mode UI pass (Notion-inspired design system) | Web | 3h | Consistent components + a11y basics | ✅ |
| T5.12 | CORS for `localhost:3000` | Api | 15m | Frontend can call API | ✅ |

---

### Day 6 — Integration & Hardening → **Moved to Sprint 2**

| # | Task | Layer | Est. | Done when | Status |
|---|---|---|---|---|---|
| T6.1 | Global exception middleware → consistent JSON errors | Api | 1h | Validation → 400 | ⏳ Sprint 2 |
| T6.2 | Input validation on all endpoints | Api | 1h | Bad input rejected | ⏳ Sprint 2 |
| T6.3 | End-to-end manual test script in README | Docs | 30m | Steps documented | 🟡 Partial |
| T6.4 | Fix bugs from demo rehearsal | All | 2h | Clean demo | 🟡 Ongoing |
| T6.5 | **Stretch:** Withdraw endpoint | All | 2h | Withdraw with balance check | ⏳ Sprint 2 |

---

### Day 7 — Retrospective & Sprint 2 Prep

| # | Task | Est. | Done when | Status |
|---|---|---|---|---|
| T7.1 | Demo; note friction points | 30m | Retro notes | ⬜ |
| T7.2 | Document tech debt (`docs/tech-debt.md`) | 30m | ≥ 5 items logged | ⬜ |
| T7.3 | Review `sprint2.md` backlog | 1h | Next sprint ready | ✅ Drafted |

---

## Out of Scope (Sprint 1 — remain in later sprints)

- Backend: withdraw, transfer, unified transactions API → **Sprint 2**
- Backend: credit cards, investments, bills, insights, settings → **Sprint 3**
- MediatR / full CQRS pipeline → Sprint 2–3
- Refresh tokens, email verification, KYC → Sprint 3+
- Unit/integration tests → Sprint 2+
- CI/CD, Docker compose for full stack → Sprint 4
- Admin dashboard → Sprint 4

---

## Technical Debt (Intentional)

1. Handlers injected directly into controllers (no MediatR)
2. DTOs duplicate entity fields
3. No integration tests
4. Last-write-wins on balance (no `ROWVERSION` enforcement)
5. Minimal error codes for client
6. No refresh token flow
7. Naive account number generation
8. **Frontend ahead of backend** — 5 modules on mock APIs
9. Transactions page N+1 calls (one request per account)
10. Transaction categories inferred client-side, not stored

---

## Acceptance Criteria

- [x] All P0 user stories demonstrable through UI (core loop)
- [x] No raw SQL concatenation; parameterized queries only
- [x] Passwords hashed; never logged
- [x] Cross-customer account access returns 404
- [x] Deposit creates exactly one `Transaction` row
- [x] `init-db.sql` reproduces schema from scratch
- [x] `prompt.md` phase rules followed (no EF, no over-abstraction)
- [ ] Global exception middleware (deferred Sprint 2)
- [ ] Withdraw + transfer backend (deferred Sprint 2)

---

## Demo Script (3 Minutes) — Core Loop

1. `docker start banking-sql` (if needed); run init script once  
2. Start API (`src/Banking.Api`) and web (`src/banking-web`)  
3. Log in as `demo@bank.local` / `Demo123!`  
4. Overview → see seed accounts and balances  
5. Open account → deposit $250  
6. Transactions → see new entry; export CSV (client-side)  
7. Optional: show Payments / Credit cards (mock data — call out as preview)  
8. Log out  

---

## Risks & Mitigations

| Risk | Mitigation |
|---|---|
| Frontend mock drift from future APIs | Shared TS types in `extended.ts`; document contracts in Architecture |
| ADO.NET boilerplate | Copy repository pattern from Day 2–4 |
| SQL Server not installed | Docker `banking-sql` container; document in README |
| Scope creep | Mock modules clearly labeled; sprint boundaries in gap table |

---

## Sprint Roadmap (Updated)

| Sprint | Theme | Focus |
|---|---|---|
| **Sprint 1** ✅ | Foundation & core loop | Auth, accounts, deposits, Next.js workspace |
| **[Sprint 2](sprint2.md)** | Core banking completion | Withdraw, transfer, hardening, unified transactions API |
| **[Sprint 3](sprint3.md)** | Product modules | Credit cards, investments, payments, insights, settings |
| **[Sprint 4](sprint4.md)** | Production shape | RBAC, idempotency, CI/CD, observability |

See `docs/Architecture.md` §1.3 and §2.3 for endpoint and capability tracking.
