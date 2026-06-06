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
IF OBJECT_ID(N'dbo.IdempotencyRecord', N'U') IS NOT NULL DROP TABLE dbo.IdempotencyRecord;
IF OBJECT_ID(N'dbo.RefreshToken', N'U') IS NOT NULL DROP TABLE dbo.RefreshToken;
IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NOT NULL DROP TABLE dbo.AuditLog;
IF OBJECT_ID(N'dbo.ScheduledPayment', N'U') IS NOT NULL DROP TABLE dbo.ScheduledPayment;
IF OBJECT_ID(N'dbo.BillPayment', N'U') IS NOT NULL DROP TABLE dbo.BillPayment;
IF OBJECT_ID(N'dbo.CreditCardSpending', N'U') IS NOT NULL DROP TABLE dbo.CreditCardSpending;
IF OBJECT_ID(N'dbo.CreditCard', N'U') IS NOT NULL DROP TABLE dbo.CreditCard;
IF OBJECT_ID(N'dbo.InvestmentHolding', N'U') IS NOT NULL DROP TABLE dbo.InvestmentHolding;
IF OBJECT_ID(N'dbo.CustomerSettings', N'U') IS NOT NULL DROP TABLE dbo.CustomerSettings;
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
    Role      TINYINT          NOT NULL CONSTRAINT DF_Customer_Role DEFAULT 1,
    CreatedAt DATETIME2        NOT NULL CONSTRAINT DF_Customer_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Customer PRIMARY KEY (Id),
    CONSTRAINT UQ_Customer_Email UNIQUE (Email),
    CONSTRAINT CK_Customer_Role CHECK (Role IN (1, 2))
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
    Category     TINYINT          NULL,
    CreatedAt    DATETIME2        NOT NULL CONSTRAINT DF_Transaction_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Transaction PRIMARY KEY (Id),
    CONSTRAINT FK_Transaction_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account (Id),
    CONSTRAINT CK_Transaction_Amount CHECK (Amount > 0),
    CONSTRAINT CK_Transaction_Type CHECK (Type IN (1, 2)),
    CONSTRAINT CK_Transaction_Category CHECK (Category IS NULL OR Category BETWEEN 1 AND 7)
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

CREATE TABLE dbo.CustomerSettings
(
    CustomerId          UNIQUEIDENTIFIER NOT NULL,
    TwoFactorEnabled    BIT              NOT NULL CONSTRAINT DF_CustomerSettings_2FA DEFAULT 0,
    NotifyTransactions  BIT              NOT NULL CONSTRAINT DF_CustomerSettings_NotifyTx DEFAULT 1,
    NotifySecurity      BIT              NOT NULL CONSTRAINT DF_CustomerSettings_NotifySec DEFAULT 1,
    NotifyMarketing     BIT              NOT NULL CONSTRAINT DF_CustomerSettings_NotifyMkt DEFAULT 0,
    UpdatedAt           DATETIME2        NOT NULL CONSTRAINT DF_CustomerSettings_UpdatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_CustomerSettings PRIMARY KEY (CustomerId),
    CONSTRAINT FK_CustomerSettings_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id)
);
GO

CREATE TABLE dbo.CreditCard
(
    Id          UNIQUEIDENTIFIER NOT NULL,
    CustomerId  UNIQUEIDENTIFIER NOT NULL,
    Name        NVARCHAR(100)    NOT NULL,
    LastFour    CHAR(4)          NOT NULL,
    Balance     DECIMAL(18, 2)   NOT NULL,
    CreditLimit DECIMAL(18, 2)   NOT NULL,
    DueDate     DATE             NOT NULL,
    MinPayment  DECIMAL(18, 2)   NOT NULL,
    Apr         DECIMAL(5, 2)    NOT NULL,
    CONSTRAINT PK_CreditCard PRIMARY KEY (Id),
    CONSTRAINT FK_CreditCard_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id),
    CONSTRAINT CK_CreditCard_Balance CHECK (Balance >= 0),
    CONSTRAINT CK_CreditCard_Limit CHECK (CreditLimit > 0)
);
GO

CREATE TABLE dbo.CreditCardSpending
(
    Id       UNIQUEIDENTIFIER NOT NULL,
    CardId   UNIQUEIDENTIFIER NOT NULL,
    Category NVARCHAR(50)     NOT NULL,
    Amount   DECIMAL(18, 2)   NOT NULL,
    CONSTRAINT PK_CreditCardSpending PRIMARY KEY (Id),
    CONSTRAINT FK_CreditCardSpending_Card FOREIGN KEY (CardId) REFERENCES dbo.CreditCard (Id),
    CONSTRAINT CK_CreditCardSpending_Amount CHECK (Amount >= 0)
);
GO

CREATE TABLE dbo.InvestmentHolding
(
    Id                UNIQUEIDENTIFIER NOT NULL,
    CustomerId        UNIQUEIDENTIFIER NOT NULL,
    Symbol            NVARCHAR(10)     NOT NULL,
    Name              NVARCHAR(100)    NOT NULL,
    Shares            DECIMAL(18, 4)   NOT NULL,
    CurrentValue      DECIMAL(18, 2)   NOT NULL,
    DayChangePercent  DECIMAL(8, 4)   NOT NULL,
    CONSTRAINT PK_InvestmentHolding PRIMARY KEY (Id),
    CONSTRAINT FK_InvestmentHolding_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id),
    CONSTRAINT CK_InvestmentHolding_Shares CHECK (Shares >= 0)
);
GO

CREATE TABLE dbo.BillPayment
(
    Id         UNIQUEIDENTIFIER NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    AccountId  UNIQUEIDENTIFIER NULL,
    Payee      NVARCHAR(100)    NOT NULL,
    Amount     DECIMAL(18, 2)   NOT NULL,
    DueDate    DATE             NOT NULL,
    Status     TINYINT          NOT NULL,
    PaidAt     DATETIME2        NULL,
    CONSTRAINT PK_BillPayment PRIMARY KEY (Id),
    CONSTRAINT FK_BillPayment_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id),
    CONSTRAINT FK_BillPayment_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account (Id),
    CONSTRAINT CK_BillPayment_Amount CHECK (Amount > 0),
    CONSTRAINT CK_BillPayment_Status CHECK (Status IN (1, 2, 3))
);
GO

CREATE TABLE dbo.ScheduledPayment
(
    Id         UNIQUEIDENTIFIER NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    AccountId  UNIQUEIDENTIFIER NOT NULL,
    Payee      NVARCHAR(100)    NOT NULL,
    Amount     DECIMAL(18, 2)   NOT NULL,
    Frequency  TINYINT          NOT NULL,
    NextDate   DATE             NOT NULL,
    CONSTRAINT PK_ScheduledPayment PRIMARY KEY (Id),
    CONSTRAINT FK_ScheduledPayment_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id),
    CONSTRAINT FK_ScheduledPayment_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account (Id),
    CONSTRAINT CK_ScheduledPayment_Amount CHECK (Amount > 0),
    CONSTRAINT CK_ScheduledPayment_Frequency CHECK (Frequency IN (1, 2, 3))
);
GO

CREATE TABLE dbo.IdempotencyRecord
(
    Id             UNIQUEIDENTIFIER NOT NULL,
    CustomerId     UNIQUEIDENTIFIER NOT NULL,
    IdempotencyKey NVARCHAR(100)    NOT NULL,
    RequestPath    NVARCHAR(200)    NOT NULL,
    ResponseStatus INT              NOT NULL,
    ResponseBody   NVARCHAR(MAX)    NOT NULL,
    CreatedAt      DATETIME2        NOT NULL CONSTRAINT DF_IdempotencyRecord_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_IdempotencyRecord PRIMARY KEY (Id),
    CONSTRAINT UQ_IdempotencyRecord_Key UNIQUE (CustomerId, IdempotencyKey, RequestPath),
    CONSTRAINT FK_IdempotencyRecord_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id)
);
GO

CREATE TABLE dbo.RefreshToken
(
    Id         UNIQUEIDENTIFIER NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    TokenHash  NVARCHAR(500)    NOT NULL,
    ExpiresAt  DATETIME2        NOT NULL,
    CreatedAt  DATETIME2        NOT NULL CONSTRAINT DF_RefreshToken_CreatedAt DEFAULT SYSUTCDATETIME(),
    RevokedAt  DATETIME2        NULL,
    CONSTRAINT PK_RefreshToken PRIMARY KEY (Id),
    CONSTRAINT FK_RefreshToken_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id)
);
GO

CREATE INDEX IX_RefreshToken_TokenHash ON dbo.RefreshToken (TokenHash);
GO

CREATE TABLE dbo.AuditLog
(
    Id         UNIQUEIDENTIFIER NOT NULL,
    CustomerId UNIQUEIDENTIFIER NULL,
    Action     NVARCHAR(100)    NOT NULL,
    EntityType NVARCHAR(50)     NOT NULL,
    EntityId   UNIQUEIDENTIFIER NULL,
    Details    NVARCHAR(MAX)    NULL,
    CreatedAt  DATETIME2        NOT NULL CONSTRAINT DF_AuditLog_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_AuditLog PRIMARY KEY (Id),
    CONSTRAINT FK_AuditLog_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id)
);
GO

CREATE INDEX IX_AuditLog_CustomerId_CreatedAt ON dbo.AuditLog (CustomerId, CreatedAt DESC);
GO

-- ---------------------------------------------------------------------------
-- Dev seed data (fixed IDs for reproducible local testing)
-- Demo login: demo@bank.local / Demo123!
-- Staff login: staff@bank.local / Staff123!
-- ---------------------------------------------------------------------------
DECLARE @DemoCustomerId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @StaffCustomerId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
DECLARE @CheckingAccountId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @SavingsAccountId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @CreditCard1Id UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444441';
DECLARE @CreditCard2Id UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444442';

INSERT INTO dbo.Customer (Id, Email, FirstName, LastName, Role, CreatedAt)
VALUES
    (@DemoCustomerId, N'demo@bank.local', N'Demo', N'Customer', 1, SYSUTCDATETIME()),
    (@StaffCustomerId, N'staff@bank.local', N'Staff', N'User', 2, SYSUTCDATETIME());

INSERT INTO dbo.CustomerAuth (CustomerId, PasswordHash, UpdatedAt)
VALUES
    (@DemoCustomerId, N'$2a$11$GfvMyTDvn/aTgEQCi3vSNuK8M0ofNpdM8uU3I74SdjkMC906F/2Zy', SYSUTCDATETIME()),
    (@StaffCustomerId, N'$2a$11$a7o1zzopvaeBnHAShqZ6rOccqmNGdvYV6npa.Z8qWtbY5WZnXoB1O', SYSUTCDATETIME());

INSERT INTO dbo.CustomerSettings (CustomerId, TwoFactorEnabled, NotifyTransactions, NotifySecurity, NotifyMarketing, UpdatedAt)
VALUES (@StaffCustomerId, 0, 1, 1, 0, SYSUTCDATETIME());

INSERT INTO dbo.CustomerSettings (CustomerId, TwoFactorEnabled, NotifyTransactions, NotifySecurity, NotifyMarketing, UpdatedAt)
VALUES (@DemoCustomerId, 0, 1, 1, 0, SYSUTCDATETIME());

INSERT INTO dbo.Account (Id, CustomerId, AccountNumber, AccountType, Balance, Currency, Status, CreatedAt)
VALUES
    (@CheckingAccountId, @DemoCustomerId, N'CHK-00000001', 1, 1000.00, 'USD', 1, SYSUTCDATETIME()),
    (@SavingsAccountId, @DemoCustomerId, N'SAV-00000001', 2, 500.00, 'USD', 1, SYSUTCDATETIME());

INSERT INTO dbo.CreditCard (Id, CustomerId, Name, LastFour, Balance, CreditLimit, DueDate, MinPayment, Apr)
VALUES
    (@CreditCard1Id, @DemoCustomerId, N'Platinum Rewards', '4821', 1842.50, 10000.00, '2026-06-15', 55.00, 18.90),
    (@CreditCard2Id, @DemoCustomerId, N'Business Card', '9034', 420.00, 5000.00, '2026-06-22', 25.00, 16.40);

INSERT INTO dbo.CreditCardSpending (Id, CardId, Category, Amount)
VALUES
    ('55555555-5555-5555-5555-555555555551', @CreditCard1Id, N'Travel', 680.00),
    ('55555555-5555-5555-5555-555555555552', @CreditCard1Id, N'Dining', 420.00),
    ('55555555-5555-5555-5555-555555555553', @CreditCard1Id, N'Software', 380.00),
    ('55555555-5555-5555-5555-555555555554', @CreditCard1Id, N'Office', 290.00),
    ('55555555-5555-5555-5555-555555555555', @CreditCard1Id, N'Other', 472.00);

INSERT INTO dbo.InvestmentHolding (Id, CustomerId, Symbol, Name, Shares, CurrentValue, DayChangePercent)
VALUES
    ('66666666-6666-6666-6666-666666666661', @DemoCustomerId, N'VTI', N'Total Stock Market', 120.0000, 28500.00, 0.8000),
    ('66666666-6666-6666-6666-666666666662', @DemoCustomerId, N'BND', N'Total Bond Market', 200.0000, 15200.00, 0.1000),
    ('66666666-6666-6666-6666-666666666663', @DemoCustomerId, N'VXUS', N'International Stocks', 85.0000, 4550.00, -0.3000);

INSERT INTO dbo.BillPayment (Id, CustomerId, AccountId, Payee, Amount, DueDate, Status, PaidAt)
VALUES
    ('77777777-7777-7777-7777-777777777771', @DemoCustomerId, @CheckingAccountId, N'Electric Company', 142.30, '2026-06-12', 1, NULL),
    ('77777777-7777-7777-7777-777777777772', @DemoCustomerId, @CheckingAccountId, N'Internet Provider', 79.99, '2026-06-18', 3, NULL),
    ('77777777-7777-7777-7777-777777777773', @DemoCustomerId, @CheckingAccountId, N'Insurance', 210.00, '2026-06-01', 2, SYSUTCDATETIME());

INSERT INTO dbo.ScheduledPayment (Id, CustomerId, AccountId, Payee, Amount, Frequency, NextDate)
VALUES
    ('88888888-8888-8888-8888-888888888881', @DemoCustomerId, @CheckingAccountId, N'Rent', 1800.00, 2, '2026-07-01'),
    ('88888888-8888-8888-8888-888888888882', @DemoCustomerId, @CheckingAccountId, N'Savings transfer', 500.00, 2, '2026-06-15');
GO

PRINT 'BankingDb schema and seed data applied successfully.';
GO
