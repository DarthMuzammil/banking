# AI Steering Prompt — Banking Application

Use this document as the **single source of truth** when generating, reviewing, or refactoring code for this project. Read it fully before proposing architecture, writing code, or suggesting libraries.

---

## Project Identity

| Field | Value |
|---|---|
| **Name** | Banking Application |
| **Stack** | C# (.NET 8+) backend, React (Vite + Javascript) frontend |
| **Data access** | ADO.NET (SqlConnection, SqlCommand, SqlDataReader) — **not** Entity Framework unless explicitly approved |
| **Timeline** | ~2–3 weeks to production-like MVP with mock/dummy external services where needed |
| **Author intent** | Learn by building. Mimic real software evolution: start simple → ship → refactor when pain appears |

---

## Core Principles (Non-Negotiable)

1. **OOP** — Encapsulate behavior in classes; favor composition over inheritance; keep entities focused.
2. **SOLID** — Apply when it reduces change cost. Do not introduce abstractions with only one implementation unless a sprint task calls for it.
3. **Clean Architecture** — Respect dependency direction: Domain ← Application ← Infrastructure ← Presentation. Dependencies point inward.
4. **Clean Code** — Meaningful names, small functions, no magic strings, consistent formatting, comments only for non-obvious business rules.
5. **CQRS** — Separate **Commands** (writes) from **Queries** (reads). Handlers are thin orchestrators; business rules live in domain/application layer.
6. **ADO.NET** — Explicit SQL, parameterized commands, repository-style data access behind interfaces defined in Application layer.

---

## Evolution Philosophy — How This App Should Grow

### Phase 0 — "Move Fast" (Sprint 1)
- Monorepo or two-repo layout with clear folders
- Minimal layers: `Domain`, `Application`, `Infrastructure`, `Api`, `Web`
- In-memory or single SQL Server database
- Hand-written SQL in repositories
- Simple JWT auth (or session-based if simpler)
- React pages: Login, Dashboard, Accounts, Transfer
- **Acceptable shortcuts:** duplicated DTOs, no MediatR yet (manual command/query dispatch), basic error handling, console logging
- **Not acceptable:** EF Core, auto-scaffolding entire solution, microservices, event sourcing, Kubernetes

### Phase 1 — "First Refactor" (Sprint 2)
- Introduce MediatR (or lightweight custom dispatcher) for CQRS
- Extract shared validation, Result<T> pattern
- Add integration tests for critical flows
- Replace shortcuts identified in Sprint 1 retrospective
- Mock external services (email, KYC, payment gateway) behind interfaces

### Phase 2 — "Production Shape" (Sprint 3)
- Structured logging (Serilog), health checks, config per environment
- Idempotency for transfers, optimistic concurrency on balances
- Audit trail table, role-based authorization
- API versioning, OpenAPI docs, basic rate limiting
- Frontend: form validation, error boundaries, loading states
- CI pipeline (build + test)

**Rule for AI:** Always match the **current phase**. Do not jump ahead unless the user explicitly asks.

---

## What to Generate vs What to Avoid

### Do
- Propose **small, reviewable diffs** (one feature or one refactor at a time)
- Explain **why** before **what** when suggesting structural changes
- Use **interfaces in Application**, implementations in **Infrastructure**
- Write **parameterized SQL**; map readers to domain objects manually
- Keep React components dumb where possible; hooks/services for API calls
- Suggest **manual steps** the developer should type — treat this as a learning project
- Flag when a pattern (e.g., full CQRS bus) is premature for the current sprint

### Do Not
- Run `dotnet new` templates that scaffold entire Clean Architecture solutions without user confirmation
- Add libraries without stating trade-offs (e.g., MediatR, FluentValidation, AutoMapper)
- Generate hundreds of lines in one response
- Introduce microservices, message buses, or DDD aggregates everywhere on day 1
- Use code generators (T4, NSwag client gen, EF migrations) unless user requests
- Replace ADO.NET with ORM "for convenience"
- Over-abstract (e.g., `IRepository<T>` for every entity on day 1)

---

## Solution Structure (Target)

```
banking/
├── src/
│   ├── Banking.Domain/           # Entities, value objects, domain exceptions
│   ├── Banking.Application/      # Commands, queries, handlers, interfaces, DTOs
│   ├── Banking.Infrastructure/   # ADO.NET repos, JWT, external service mocks
│   ├── Banking.Api/              # ASP.NET Core Web API, controllers/minimal APIs
│   └── banking-web/              # React + Vite + Javascript
├── tests/
│   └── Banking.Application.Tests/
├── docs/
│   ├── Architecture.md
│   └── sprints/
│       └── sprint1.md
├── scripts/                      # SQL init scripts
└── prompt.md                     # This file
```

Adjust only with user approval. Prefer creating folders **as features need them**, not all upfront.

---

## Coding Standards

### C# Backend
- **Naming:** `PascalCase` types/methods; `_camelCase` private fields; `I` prefix for interfaces
- **Commands:** `CreateAccountCommand`, handler `CreateAccountCommandHandler`
- **Queries:** `GetAccountByIdQuery`, handler returns DTOs — never domain entities to API
- **Results:** Prefer `Result` or `Result<T>` over throwing for expected failures (invalid amount, insufficient funds)
- **SQL:** One repository class per aggregate/table group; no SQL in controllers
- **Transactions:** Use `SqlTransaction` for transfer/debit-credit operations

### React Frontend
- Feature folders: `features/accounts`, `features/auth`, `shared/components`
- API client module per resource; no fetch scattered in components
- Environment variable for API base URL

### Git / Workflow
- Small commits with clear messages
- No committing secrets or connection strings
- User builds and runs locally — AI provides instructions, not only finished code

---

## Domain Vocabulary (Use Consistently)

| Term | Meaning |
|---|---|
| **Customer** | Registered user of the bank |
| **Account** | Ledger account (Checking/Savings) owned by a customer |
| **Transaction** | Immutable record of debit/credit |
| **Transfer** | Movement between two accounts (atomic) |
| **Balance** | Current available funds (stored, not computed on every read in MVP) |

---

## Security Baseline (Even in MVP)

- Passwords hashed (BCrypt or ASP.NET Identity PasswordHasher — pick one, document choice)
- JWT with short expiry + refresh token (Sprint 2 if Sprint 1 is tight)
- HTTPS in local dev via dev cert
- Never log passwords, tokens, or full PAN/account secrets
- Parameterized queries only — no string concatenation for SQL

---

## External Services (Mock Until Sprint 2+)

| Service | MVP Behavior |
|---|---|
| Email notifications | Log to console / write to `Outbox` table |
| KYC verification | Always return `Approved` stub |
| Payment gateway | Internal transfer only; no real ACH/card |
| SMS OTP | Fixed code `123456` in dev |

---

## Prompt Template for Each AI Session

Copy and paste at the start of a coding session:

```
Context: Banking app — C# + React. Read /prompt.md and /docs/Architecture.md.
Current sprint: sprint1.md
Current phase: [0 Move Fast | 1 Refactor | 2 Production]

Task: [specific task]

Constraints:
- ADO.NET only for data access
- Match current evolution phase — no over-engineering
- Small diff; explain design choices briefly
- I will type the code — guide me step by step OR show one file at a time

Do NOT: scaffold entire solution, add EF Core, or introduce patterns from future sprints.
```

---

## Definition of Done (Per Feature)

- [ ] Domain rule documented in code or sprint notes
- [ ] Command/query + handler (or documented reason for deferral)
- [ ] ADO.NET repository method with parameterized SQL
- [ ] API endpoint with validation and appropriate HTTP status codes
- [ ] React screen wired to API with basic error display
- [ ] Manual test steps listed in PR/commit description
- [ ] No secrets in source control

---

## When in Doubt

Ask: *"Would a two-person startup ship this in the current sprint?"*

If no → simplify.  
If yes but messy → ship, add TODO with sprint reference for refactor.

The goal is **working software that teaches**, not a textbook implementation on day one.
