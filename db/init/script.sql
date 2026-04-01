CREATE DATABASE AgendamientoDB;
GO

USE AgendamientoDB;
GO

-- Crear login
CREATE LOGIN desarroladorivan WITH PASSWORD = 'Ivan123**';
GO

-- Crear usuario en la DB
CREATE USER desarroladorivan FOR LOGIN desarroladorivan;
GO

-- Dar permisos
ALTER ROLE db_owner ADD MEMBER desarroladorivan;
GO

-- Crear tabla de prueba
CREATE TABLE TestTable (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100)
);
GO

INSERT INTO TestTable (Name) VALUES ('Prueba inicial');
GO