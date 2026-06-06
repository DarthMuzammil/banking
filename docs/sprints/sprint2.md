# Sprint 2 — Core Banking Completion

**Duration:** 1 week  
**Theme:** *Close the gap between UI promises and backend reality for money movement*  
**Phase:** 0 → 1 transition (see `prompt.md`)  
**Prerequisite:** Sprint 1 core loop complete

---

## Sprint Goal

A customer can **withdraw**, **transfer between own accounts**, and use the **Payments** and **Transactions** UI against **real APIs** — with consistent error handling and a path to integration tests.

> **Success statement:** Replace mock `submitTransfer` with `POST /api/transfers`; add withdraw from account detail; unified `GET /api/transactions` replaces client-side N+1 aggregation.

---

## User Stories

| ID | As a… | I want to… | So that… | Priority |
|---|---|---|---|---|
| US-12 | Customer | Withdraw from my account | I can access cash | P0 |
| US-13 | Customer | Transfer between my accounts | I can move money without leaving the app | P0 |
| US-14 | Customer | See all transactions in one API call | The transactions page is fast and filterable server-side | P1 |
| US-15 | Developer | Get consistent API errors | The UI can show meaningful messages | P0 |
| US-16 | Developer | Have integration tests for deposit/withdraw/transfer | Regressions are caught | P1 |

---

## Frontend wiring (swap mocks → real)

| File | Change |
|---|---|
| `shared/api/services/payments.ts` | `submitTransfer` → `POST /api/transfers` |
| `shared/api/services/transactions.ts` | `getAllTransactions` → `GET /api/transactions` (optional: keep client export) |
| `app/(workspace)/accounts/[id]/page.tsx` | Add withdraw form |
| `shared/api/accounts.ts` | Add `withdraw(token, accountId, amount, description?)` |

---

## Tasks Breakdown

### Day 1 — Withdraw

| # | Task | Layer | Est. | Done when |
|---|---|---|---|---|
| T2.1 | `WithdrawCommand` + handler (balance check, SQL txn) | Application | 2h | Uses `InsufficientFundsException` |
| T2.2 | `POST /api/accounts/{id}/withdrawals` | Api | 1h | 400 insufficient funds; 404 not owned |
| T2.3 | Withdraw form on account detail page | Web | 1.5h | Balance updates after success |

---

### Day 2–3 — Internal Transfer

| # | Task | Layer | Est. | Done when |
|---|---|---|---|---|
| T2.4 | `ITransferRepository` + `TransferCommandHandler` | Application | 2h | Atomic dual-leg + `Transfer` row |
| T2.5 | `TransferRepository` (ADO.NET) | Infrastructure | 2h | SQL transaction wraps both accounts |
| T2.6 | `POST /api/transfers` | Api | 1h | Returns reference + receipt DTO |
| T2.7 | Wire Payments page to real transfer API | Web | 1h | Mock removed from `payments.ts` |
| T2.8 | Mock email on deposit/transfer (console) | Infrastructure | 1h | `IEmailService` logs receipt |

---

### Day 4 — Unified Transactions API

| # | Task | Layer | Est. | Done when |
|---|---|---|---|---|
| T2.9 | `GetCustomerTransactionsQuery` (cross-account, paginated) | Application | 2h | `?search=&accountId=&skip=&take=` |
| T2.10 | `GET /api/transactions` | Api | 1h | Owner-scoped; replaces N+1 client calls |
| T2.11 | Update Transactions page to use unified endpoint | Web | 1h | Filters passed as query params |

---

### Day 5 — Hardening (Sprint 1 Day 6 carryover)

| # | Task | Layer | Est. | Done when |
|---|---|---|---|---|
| T2.12 | Global exception middleware | Api | 1h | `{ error, code }` JSON shape |
| T2.13 | Input validation all endpoints | Api | 1h | amount > 0, required fields |
| T2.14 | `Banking.Application.Tests` — deposit, withdraw, transfer | Tests | 3h | Critical paths covered |

---

### Day 6 — Refactor buffer (optional)

| # | Task | Layer | Est. | Done when |
|---|---|---|---|---|
| T2.15 | MediatR or lightweight dispatcher | Application | 2h | Controllers thin |
| T2.16 | `Result<T>` consistency audit | Application | 1h | No unhandled domain throws for expected failures |

---

## API Additions (Sprint 2)

| Method | Route | Auth | Status |
|---|---|---|---|
| POST | `/api/accounts/{id}/withdrawals` | Yes | New |
| POST | `/api/transfers` | Yes | New |
| GET | `/api/transactions` | Yes | New |

---

## Acceptance Criteria

- [ ] Withdraw rejects amount > balance with clear error code
- [ ] Transfer is atomic (both legs or neither)
- [ ] Payments transfer form uses real API
- [ ] `GET /api/transactions` supports pagination + account filter
- [ ] Exception middleware on all controllers
- [ ] At least 3 integration tests for money movement

---

## Out of Scope (Sprint 3)

- Credit cards, investments, bill pay backends
- Transaction categories in database
- Refresh tokens, RBAC
