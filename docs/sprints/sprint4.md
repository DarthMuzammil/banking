# Sprint 4 — Production Shape

**Duration:** 1–2 weeks  
**Theme:** *Operate safely at scale*  
**Phase:** 2 — "Production Shape" (see `prompt.md`)  
**Prerequisite:** Sprint 3 all modules on real APIs

---

## Sprint Goal

The application is **deployable**, **observable**, and **safe under concurrency** — with admin capabilities and automated quality gates.

---

## Focus Areas

| Area | Deliverables |
|---|---|
| Concurrency | `ROWVERSION` on balance updates; optimistic conflict → 409 |
| Idempotency | `Idempotency-Key` header on transfer/deposit |
| Security | Refresh tokens; RBAC (customer vs staff); audit log table |
| Operations | Structured logging (Serilog); health checks; OpenAPI |
| Quality | CI pipeline (build + test); Docker Compose for local stack |
| Admin | Staff read-only customer search (mock or real) |

---

## User Stories (preview)

| ID | As a… | I want to… | Priority |
|---|---|---|---|
| US-22 | Customer | Retry a transfer safely without double-charge | P0 |
| US-23 | Staff | Look up a customer account | P1 |
| US-24 | Operator | See health and logs in deployment | P0 |

---

## Tasks (outline)

- T4.1 Enforce `ROWVERSION` in `UpdateBalanceAsync`
- T4.2 Idempotency table + middleware
- T4.3 Refresh token flow
- T4.4 `AuditLog` table + write on money movement
- T4.5 Serilog + request logging
- T4.6 OpenAPI / Swagger UI
- T4.7 GitHub Actions CI
- T4.8 `docker-compose.yml` (API + SQL + web)
- T4.9 Admin API + UI (stretch)

---

## Acceptance Criteria

- [ ] Duplicate transfer with same idempotency key is no-op
- [ ] Concurrent balance updates return 409, not silent overwrite
- [ ] CI runs on every PR
- [ ] `docker compose up` starts full stack

See `docs/Architecture.md` §4 NFRs for targets.
