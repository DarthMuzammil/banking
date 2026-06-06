# Sprint 3 — Product Modules (Mock → Real)

**Duration:** 1–2 weeks  
**Theme:** *Back the expanded UI with real (or mock-service) APIs*  
**Phase:** 1 — "First Refactor" (see `prompt.md`)  
**Prerequisite:** Sprint 2 transfer + withdraw complete

---

## Sprint Goal

Every **sidebar module** in `banking-web` calls a **real backend endpoint**. External products (credit, investments, bill pay) may use **mock external providers** behind application interfaces — but not hardcoded frontend mocks.

> **Success statement:** Remove `shared/api/mocks/data.ts` from production paths; `bankingApi` index points to real services only.

---

## Modules to Implement

| Module | UI today | Sprint 3 deliverable |
|---|---|---|
| Spending insights | Mock on dashboard | `GET /api/insights/spending` — aggregate from transactions |
| Credit cards | Mock list + utilization | `CreditCard` table + CRUD read APIs (synthetic data or seed) |
| Investments | Mock portfolio | `InvestmentHolding` table + `GET /api/investments/portfolio` |
| Bill payments | Mock due list | `BillPayment` table + pay bill command (mock ACH) |
| Scheduled payments | Mock list | `ScheduledPayment` table + CRUD |
| Settings | Mock toggles | `GET/PATCH /api/settings` — persist notification prefs |
| Transaction categories | Client inference | Optional: `Category` column on `Transaction` |

---

## User Stories

| ID | As a… | I want to… | So that… | Priority |
|---|---|---|---|---|
| US-17 | Customer | See real spending insights | Dashboard reflects my actual transactions | P1 |
| US-18 | Customer | View my credit cards | I know balances and due dates | P2 |
| US-19 | Customer | View my investment portfolio | I track holdings in one place | P2 |
| US-20 | Customer | Pay bills and schedule payments | I manage outflows | P2 |
| US-21 | Customer | Save notification preferences | Settings persist across sessions | P1 |

---

## Schema Additions (high level)

New tables — details in `Architecture.md` §3.5 when implemented:

- `CreditCard` (CustomerId, lastFour, limit, balance, dueDate, …)
- `InvestmentHolding` (CustomerId, symbol, shares, costBasis, …)
- `BillPayment` / `ScheduledPayment`
- `CustomerSettings` (CustomerId, notification flags, …)
- Optional: `Transaction.Category` TINYINT

Migration: extend `scripts/init-db.sql` or add `scripts/migrations/003-product-modules.sql`.

---

## Tasks Breakdown (suggested order)

### Week 1 — Settings + Insights + Categories

| # | Task | Est. |
|---|---|---|
| T3.1 | `CustomerSettings` schema + repository | 2h |
| T3.2 | `GET/PATCH /api/settings` | 2h |
| T3.3 | Wire Settings page to real API | 1h |
| T3.4 | `GetSpendingInsightsQuery` from transaction aggregates | 3h |
| T3.5 | `GET /api/insights/spending` + dashboard wire-up | 2h |
| T3.6 | Add `Category` to Transaction (optional) + seed mapping | 3h |

### Week 2 — Credit, Investments, Payments

| # | Task | Est. |
|---|---|---|
| T3.7 | Credit card schema + `GET /api/credit-cards` | 4h |
| T3.8 | Investment schema + `GET /api/investments/portfolio` | 4h |
| T3.9 | Bill + scheduled payment schema + endpoints | 6h |
| T3.10 | `PayBillCommand` with mock payment gateway | 3h |
| T3.11 | Remove frontend mock services; update `bankingApi` index | 2h |
| T3.12 | Seed data for demo customer (card, holdings, bills) | 2h |

---

## Acceptance Criteria

- [ ] No module in sidebar relies on `mocks/data.ts`
- [ ] Settings persist to SQL Server
- [ ] Insights computed from real transaction data
- [ ] Credit cards and investments show seeded data for demo user
- [ ] Bill pay debits account + creates transaction row

---

## Out of Scope (Sprint 4)

- Real ACH/card networks
- RBAC / admin portal
- Idempotency keys
- CI/CD pipeline
