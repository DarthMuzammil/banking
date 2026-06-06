/*
  BankingDb initialization script
  Run against a fresh SQL Server instance (LocalDB, Docker, or full SQL Server).

  Example (Docker):
    docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
      -p 1433:1433 --name banking-sql -d mcr.microsoft.com/mssql/server:2022-latest

    sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -i scripts/init-db.sql
*/

IF DB_ID(N'BankingDb') IS NULL
BEGIN
    CREATE DATABASE BankingDb;
END
GO

USE BankingDb;
GO

-- Drop in dependency order for idempotent re-runs during development
IF OBJECT_ID(N'dbo.Transfer', N'U') IS NOT NULL DROP TABLE dbo.Transfer;
IF OBJECT_ID(N'dbo.[Transaction]', N'U') IS NOT NULL DROP TABLE dbo.[Transaction];
IF OBJECT_ID(N'dbo.Account', N'U') IS NOT NULL DROP TABLE dbo.Account;
IF OBJECT_ID(N'dbo.CustomerAuth', N'U') IS NOT NULL DROP TABLE dbo.CustomerAuth;
IF OBJECT_ID(N'dbo.Customer', N'U') IS NOT NULL DROP TABLE dbo.Customer;
GO

CREATE TABLE dbo.Customer
(
    Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Customer_Id DEFAULT NEWID(),
    Email     NVARCHAR(256)    NOT NULL,
    FirstName NVARCHAR(100)    NOT NULL,
    LastName  NVARCHAR(100)    NOT NULL,
    CreatedAt DATETIME2        NOT NULL CONSTRAINT DF_Customer_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Customer PRIMARY KEY (Id),
    CONSTRAINT UQ_Customer_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.CustomerAuth
(
    CustomerId   UNIQUEIDENTIFIER NOT NULL,
    PasswordHash NVARCHAR(500)    NOT NULL,
    UpdatedAt    DATETIME2        NOT NULL CONSTRAINT DF_CustomerAuth_UpdatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_CustomerAuth PRIMARY KEY (CustomerId),
    CONSTRAINT FK_CustomerAuth_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id)
);
GO

CREATE TABLE dbo.Account
(
    Id            UNIQUEIDENTIFIER NOT NULL,
    CustomerId    UNIQUEIDENTIFIER NOT NULL,
    AccountNumber NVARCHAR(20)     NOT NULL,
    AccountType   TINYINT          NOT NULL,
    Balance       DECIMAL(18, 2)   NOT NULL CONSTRAINT DF_Account_Balance DEFAULT 0,
    Currency      CHAR(3)          NOT NULL CONSTRAINT DF_Account_Currency DEFAULT 'USD',
    Status        TINYINT          NOT NULL CONSTRAINT DF_Account_Status DEFAULT 1,
    CreatedAt     DATETIME2        NOT NULL CONSTRAINT DF_Account_CreatedAt DEFAULT SYSUTCDATETIME(),
    RowVersion    ROWVERSION       NOT NULL,
    CONSTRAINT PK_Account PRIMARY KEY (Id),
    CONSTRAINT UQ_Account_AccountNumber UNIQUE (AccountNumber),
    CONSTRAINT FK_Account_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id),
    CONSTRAINT CK_Account_Balance CHECK (Balance >= 0),
    CONSTRAINT CK_Account_Type CHECK (AccountType IN (1, 2)),
    CONSTRAINT CK_Account_Status CHECK (Status IN (1, 2))
);
GO

CREATE TABLE dbo.[Transaction]
(
    Id           UNIQUEIDENTIFIER NOT NULL,
    AccountId    UNIQUEIDENTIFIER NOT NULL,
    Type         TINYINT          NOT NULL,
    Amount       DECIMAL(18, 2)   NOT NULL,
    BalanceAfter DECIMAL(18, 2)   NOT NULL,
    Description  NVARCHAR(500)    NULL,
    ReferenceId  UNIQUEIDENTIFIER NULL,
    CreatedAt    DATETIME2        NOT NULL CONSTRAINT DF_Transaction_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Transaction PRIMARY KEY (Id),
    CONSTRAINT FK_Transaction_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account (Id),
    CONSTRAINT CK_Transaction_Amount CHECK (Amount > 0),
    CONSTRAINT CK_Transaction_Type CHECK (Type IN (1, 2))
);
GO

CREATE INDEX IX_Transaction_AccountId_CreatedAt
    ON dbo.[Transaction] (AccountId, CreatedAt DESC);
GO

CREATE TABLE dbo.Transfer
(
    Id            UNIQUEIDENTIFIER NOT NULL,
    FromAccountId UNIQUEIDENTIFIER NOT NULL,
    ToAccountId   UNIQUEIDENTIFIER NOT NULL,
    Amount        DECIMAL(18, 2)   NOT NULL,
    Status        TINYINT          NOT NULL,
    Reference     NVARCHAR(50)     NOT NULL,
    CreatedAt     DATETIME2        NOT NULL CONSTRAINT DF_Transfer_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Transfer PRIMARY KEY (Id),
    CONSTRAINT UQ_Transfer_Reference UNIQUE (Reference),
    CONSTRAINT FK_Transfer_FromAccount FOREIGN KEY (FromAccountId) REFERENCES dbo.Account (Id),
    CONSTRAINT FK_Transfer_ToAccount FOREIGN KEY (ToAccountId) REFERENCES dbo.Account (Id),
    CONSTRAINT CK_Transfer_Amount CHECK (Amount > 0),
    CONSTRAINT CK_Transfer_Status CHECK (Status IN (1, 2, 3)),
    CONSTRAINT CK_Transfer_DistinctAccounts CHECK (FromAccountId <> ToAccountId)
);
GO

-- ---------------------------------------------------------------------------
-- Dev seed data (fixed IDs for reproducible local testing)
-- Demo login: demo@bank.local / Demo123!  (password hash only; plain text not stored)
-- ---------------------------------------------------------------------------
DECLARE @DemoCustomerId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @CheckingAccountId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @SavingsAccountId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';

INSERT INTO dbo.Customer (Id, Email, FirstName, LastName, CreatedAt)
VALUES (@DemoCustomerId, N'demo@bank.local', N'Demo', N'Customer', SYSUTCDATETIME());

INSERT INTO dbo.CustomerAuth (CustomerId, PasswordHash, UpdatedAt)
VALUES (
    @DemoCustomerId,
    N'$2a$11$GfvMyTDvn/aTgEQCi3vSNuK8M0ofNpdM8uU3I74SdjkMC906F/2Zy',
    SYSUTCDATETIME()
);

INSERT INTO dbo.Account (Id, CustomerId, AccountNumber, AccountType, Balance, Currency, Status, CreatedAt)
VALUES
    (@CheckingAccountId, @DemoCustomerId, N'CHK-00000001', 1, 1000.00, 'USD', 1, SYSUTCDATETIME()),
    (@SavingsAccountId, @DemoCustomerId, N'SAV-00000001', 2, 500.00, 'USD', 1, SYSUTCDATETIME());
GO

PRINT 'BankingDb schema and seed data applied successfully.';
GO
