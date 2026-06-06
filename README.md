# Banking Application

Learning project: C# Web API + React + SQL Server with ADO.NET and Clean Architecture.

See `prompt.md` for AI steering rules and `docs/Architecture.md` for design details.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) (or newer)
- SQL Server (LocalDB, Docker, or full instance)
- Node.js 20+ (for frontend — Sprint 1 Day 5)

## Project Structure

```
banking/
├── src/
│   ├── Banking.Domain/           # Entities, domain rules (no external packages)
│   ├── Banking.Application/      # Use cases, interfaces, DTOs
│   ├── Banking.Infrastructure/   # ADO.NET, JWT, mocks
│   └── Banking.Api/              # REST API
├── docs/
├── scripts/
│   └── init-db.sql
└── prompt.md
```

## Database Setup

Start SQL Server (Docker example):

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name banking-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

Run the init script:

```bash
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -i scripts/init-db.sql
```

### Dev seed credentials

| Field | Value |
|---|---|
| Email | `demo@bank.local` |
| Password | `Demo123!` |

Seed includes one customer with Checking ($1,000) and Savings ($500) accounts.

## API

Configure the connection string in `src/Banking.Api/appsettings.Development.json` (use User Secrets in production-like setups — do not commit real passwords).

```bash
cd src/Banking.Api
dotnet run
```

Health check: `GET http://localhost:5xxx/api/health` → `{ "status": "healthy" }`

## Auth Endpoints (Day 2)

```bash
# Register
curl -X POST http://localhost:5099/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@example.com","password":"Password123!","firstName":"Alice","lastName":"Smith"}'

# Login (returns JWT)
curl -X POST http://localhost:5099/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@bank.local","password":"Demo123!"}'
```

Password hashing uses **BCrypt** (`BCrypt.Net-Next`). JWT `sub` claim carries the customer ID (used for account scoping in Day 3).

## Sprint Progress

- [x] Day 1 — Solution skeleton, DB schema, health endpoint
- [x] Day 2 — Domain entities & auth (register/login)
- [ ] Day 3 — Accounts
- [ ] Day 4 — Deposits & transactions
- [ ] Day 5 — React frontend
