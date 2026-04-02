IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AgendamientoDB')
BEGIN
    CREATE DATABASE AgendamientoDB;
END
GO

USE AgendamientoDB;
GO

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'desarroladorivan')
BEGIN
    CREATE LOGIN desarroladorivan WITH PASSWORD = 'Ivan123**';
END
GO

IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'desarroladorivan')
BEGIN
    CREATE USER desarroladorivan FOR LOGIN desarroladorivan;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    JOIN sys.database_principals u ON drm.member_principal_id = u.principal_id
    WHERE r.name = 'db_owner' AND u.name = 'desarroladorivan'
)
BEGIN
    ALTER ROLE db_owner ADD MEMBER desarroladorivan;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users' AND type = 'U')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Username NVARCHAR(256) NOT NULL UNIQUE,
        Password NVARCHAR(MAX) NOT NULL,
        Email NVARCHAR(256) NULL,
        Birthdate DATE NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = 'Email')
BEGIN
    ALTER TABLE Users ADD Email NVARCHAR(256) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = 'Birthdate')
BEGIN
    ALTER TABLE Users ADD Birthdate DATE NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Password, Email, Birthdate) VALUES ('admin', '$2a$11$p.h1zVIa2h6I21LdJ2e.Een4yloWJg2i2bs2nS3sC2jMImoeFAxSO', 'admin@example.com', '1985-01-01');
END
GO

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'prueba')
BEGIN
    INSERT INTO Users (Username, Password, Email, Birthdate) VALUES ('prueba', '$2b$12$BqxHpVhQwUKsiBShkwMTnuUwHvvtEeOByGeARAZjc4T8Zvwc/nNs6', 'prueba@example.com', '1990-09-01');
END
GO