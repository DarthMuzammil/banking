# Banking Application

Learning project: C# Web API + Next.js + SQL Server with ADO.NET and Clean Architecture.

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
│   ├── Banking.Api/              # REST API
│   └── banking-web/              # Next.js + Tailwind frontend
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

## Frontend

Next.js + Tailwind workspace at `src/banking-web`. Design language: Notion-inspired — calm neutrals, typography-led hierarchy, generous whitespace.

**Modules:** Overview, Accounts, Transactions, Payments, Credit cards, Investments, Settings.

**Mock APIs** (easily swappable): credit cards, investments, payments, insights, settings. Real API: auth, accounts, deposits, transactions. See `src/banking-web/src/shared/api/index.ts`.

```bash
# Terminal 1 — API (must be running first)
cd src/Banking.Api && dotnet run

# Terminal 2 — Web
cd src/banking-web
cp .env.example .env.local   # if .env.local doesn't exist
npm run dev
```

Open [http://localhost:3000](http://localhost:3000).

### Demo flow

1. Register or log in (`demo@bank.local` / `Demo123!`)
2. Dashboard lists accounts with balances
3. Create a Checking or Savings account
4. Open an account → deposit funds → see updated balance and transaction history
5. Log out

## Sprint Progress

- [x] Day 1 — Solution skeleton, DB schema, health endpoint
- [x] Day 2 — Domain entities & auth (register/login)
- [x] Day 3 — Accounts
- [x] Day 4 — Deposits & transactions
- [x] Day 5 — Next.js frontend
