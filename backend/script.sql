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

-- Crear usuarios médicos si no existen
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'medico_juan')
BEGIN
    INSERT INTO Users (Username, Password, Email, Birthdate) VALUES ('medico_juan', '$2b$12$BqxHpVhQwUKsiBShkwMTnuUwHvvtEeOByGeARAZjc4T8Zvwc/nNs6', 'juan.perez@clinica.com', '1980-05-15');
END
GO

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'medico_maria')
BEGIN
    INSERT INTO Users (Username, Password, Email, Birthdate) VALUES ('medico_maria', '$2b$12$BqxHpVhQwUKsiBShkwMTnuUwHvvtEeOByGeARAZjc4T8Zvwc/nNs6', 'maria.gomez@clinica.com', '1982-07-20');
END
GO

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'medico_carlos')
BEGIN
    INSERT INTO Users (Username, Password, Email, Birthdate) VALUES ('medico_carlos', '$2b$12$BqxHpVhQwUKsiBShkwMTnuUwHvvtEeOByGeARAZjc4T8Zvwc/nNs6', 'carlos.lopez@clinica.com', '1978-03-10');
END
GO

-- Nueva tabla: Especialidad
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Especialidades' AND type = 'U')
BEGIN
    CREATE TABLE Especialidades (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Nombre NVARCHAR(256) NOT NULL
    );
END
GO

-- Nueva tabla: Medico
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Medicos' AND type = 'U')
BEGIN
    CREATE TABLE Medicos (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UsuarioId UNIQUEIDENTIFIER NOT NULL,
        Nombre NVARCHAR(256) NOT NULL,
        Apellido NVARCHAR(256) NOT NULL,
        EspecialidadId UNIQUEIDENTIFIER NOT NULL,
        Email NVARCHAR(256) NULL,
        Telefono NVARCHAR(256) NULL,
        FOREIGN KEY (UsuarioId) REFERENCES Users(Id),
        FOREIGN KEY (EspecialidadId) REFERENCES Especialidades(Id)
    );
END
GO

-- Agregar columna UsuarioId si no existe (para migración)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Medicos') AND name = 'UsuarioId')
BEGIN
    ALTER TABLE Medicos ADD UsuarioId UNIQUEIDENTIFIER NULL;
END
GO

-- Nueva tabla: DisponibilidadMedica
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DisponibilidadesMedicas' AND type = 'U')
BEGIN
    CREATE TABLE DisponibilidadesMedicas (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        MedicoId UNIQUEIDENTIFIER NOT NULL,
        DiaSemana NVARCHAR(50) NOT NULL,
        HoraInicio TIME NOT NULL,
        HoraFin TIME NOT NULL,
        DuracionCitaMinutos INT NOT NULL,
        FOREIGN KEY (MedicoId) REFERENCES Medicos(Id)
    );
END
GO

-- Nueva tabla: Cita
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Cita' AND type = 'U')
BEGIN
    CREATE TABLE Citas (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        PacienteId UNIQUEIDENTIFIER NOT NULL,
        MedicoId UNIQUEIDENTIFIER NOT NULL,
        FechaCita DATE NOT NULL,
        HoraCita TIME NOT NULL,
        Estado NVARCHAR(50) NOT NULL,
        FOREIGN KEY (PacienteId) REFERENCES Users(Id) ON DELETE CASCADE,
        FOREIGN KEY (MedicoId) REFERENCES Medicos(Id) ON DELETE CASCADE
    );
END
GO

-- Poblar datos iniciales de prueba

-- Especialidades
IF NOT EXISTS (SELECT 1 FROM Especialidades WHERE Nombre = 'Cardiología')
BEGIN
    INSERT INTO Especialidades (Nombre) VALUES ('Cardiología');
END
GO

IF NOT EXISTS (SELECT 1 FROM Especialidades WHERE Nombre = 'Dermatología')
BEGIN
    INSERT INTO Especialidades (Nombre) VALUES ('Dermatología');
END
GO

IF NOT EXISTS (SELECT 1 FROM Especialidades WHERE Nombre = 'Pediatría')
BEGIN
    INSERT INTO Especialidades (Nombre) VALUES ('Pediatría');
END
GO

-- Médicos (usando IDs de especialidades y usuarios)
DECLARE @IdCardiologia UNIQUEIDENTIFIER = (SELECT Id FROM Especialidades WHERE Nombre = 'Cardiología');
DECLARE @IdDermatologia UNIQUEIDENTIFIER = (SELECT Id FROM Especialidades WHERE Nombre = 'Dermatología');
DECLARE @IdPediatria UNIQUEIDENTIFIER = (SELECT Id FROM Especialidades WHERE Nombre = 'Pediatría');
DECLARE @IdUsuarioJuan UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Username = 'medico_juan');
DECLARE @IdUsuarioMaria UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Username = 'medico_maria');
DECLARE @IdUsuarioCarlos UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Username = 'medico_carlos');

IF NOT EXISTS (SELECT 1 FROM Medicos WHERE Nombre = 'Juan' AND Apellido = 'Pérez')
BEGIN
    INSERT INTO Medicos (UsuarioId, Nombre, Apellido, EspecialidadId, Email, Telefono) VALUES (@IdUsuarioJuan, 'Juan', 'Pérez', @IdCardiologia, 'juan.perez@clinica.com', '555-1234');
END
GO

IF NOT EXISTS (SELECT 1 FROM Medicos WHERE Nombre = 'María' AND Apellido = 'Gómez')
BEGIN
    INSERT INTO Medicos (UsuarioId, Nombre, Apellido, EspecialidadId, Email, Telefono) VALUES (@IdUsuarioMaria, 'María', 'Gómez', @IdDermatologia, 'maria.gomez@clinica.com', '555-5678');
END
GO

IF NOT EXISTS (SELECT 1 FROM Medicos WHERE Nombre = 'Carlos' AND Apellido = 'López')
BEGIN
    INSERT INTO Medicos (UsuarioId, Nombre, Apellido, EspecialidadId, Email, Telefono) VALUES (@IdUsuarioCarlos, 'Carlos', 'López', @IdPediatria, 'carlos.lopez@clinica.com', '555-9012');
END
GO

-- Disponibilidad médica
DECLARE @IdJuan UNIQUEIDENTIFIER = (SELECT Id FROM Medicos WHERE Nombre = 'Juan' AND Apellido = 'Pérez');
DECLARE @IdMaria UNIQUEIDENTIFIER = (SELECT Id FROM Medicos WHERE Nombre = 'María' AND Apellido = 'Gómez');
DECLARE @IdCarlos UNIQUEIDENTIFIER = (SELECT Id FROM Medicos WHERE Nombre = 'Carlos' AND Apellido = 'López');

IF NOT EXISTS (SELECT 1 FROM DisponibilidadesMedicas WHERE MedicoId = @IdJuan AND DiaSemana = 'Lunes')
BEGIN
    INSERT INTO DisponibilidadesMedicas (MedicoId, DiaSemana, HoraInicio, HoraFin, DuracionCitaMinutos) VALUES (@IdJuan, 'Lunes', '09:00', '17:00', 30);
END
GO

IF NOT EXISTS (SELECT 1 FROM DisponibilidadesMedicas WHERE MedicoId = @IdMaria AND DiaSemana = 'Martes')
BEGIN
    INSERT INTO DisponibilidadesMedicas (MedicoId, DiaSemana, HoraInicio, HoraFin, DuracionCitaMinutos) VALUES (@IdMaria, 'Martes', '10:00', '16:00', 30);
END
GO

IF NOT EXISTS (SELECT 1 FROM DisponibilidadesMedicas WHERE MedicoId = @IdCarlos AND DiaSemana = 'Miércoles')
BEGIN
    INSERT INTO DisponibilidadesMedicas (MedicoId, DiaSemana, HoraInicio, HoraFin, DuracionCitaMinutos) VALUES (@IdCarlos, 'Miércoles', '08:00', '15:00', 30);
END
GO

-- Citas de prueba (usando IDs de usuarios y médicos)
DECLARE @IdAdmin UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Username = 'admin');

IF NOT EXISTS (SELECT 1 FROM Citas WHERE PacienteId = @IdAdmin AND MedicoId = @IdJuan AND FechaCita = '2026-04-07')
BEGIN
    INSERT INTO Citas (PacienteId, MedicoId, FechaCita, HoraCita, Estado) VALUES (@IdAdmin, @IdJuan, '2026-04-07', '10:00', 'Pendiente');
END
GO

IF NOT EXISTS (SELECT 1 FROM Citas WHERE PacienteId = @IdAdmin AND MedicoId = @IdMaria AND FechaCita = '2026-04-08')
BEGIN
    INSERT INTO Citas (PacienteId, MedicoId, FechaCita, HoraCita, Estado) VALUES (@IdAdmin, @IdMaria, '2026-04-08', '11:00', 'Confirmada');
END
GO