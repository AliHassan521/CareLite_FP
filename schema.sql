-- Drop existing table if exists
IF OBJECT_ID('Users', 'U') IS NOT NULL
    DROP TABLE Users;
GO

CREATE TABLE Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(500) NOT NULL,
    FullName VARCHAR(200) NOT NULL,
    Email VARCHAR(200) NULL,
    Phone VARCHAR(50) NULL,
    RoleId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NULL
);
GO

-- Drop existing table if exists
IF OBJECT_ID('Roles', 'U') IS NOT NULL
    DROP TABLE Roles;
GO

CREATE TABLE Roles
(
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

-- Insert default roles
INSERT INTO Roles (RoleName) VALUES ('Admin'), ('Staff'), ('Clinician');
GO

-- Drop existing table if exists
IF OBJECT_ID('AuditLogs', 'U') IS NOT NULL
    DROP TABLE AuditLogs;
GO

CREATE TABLE AuditLogs
(
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    UserId INT NULL,
    Action VARCHAR(200) NOT NULL,
    Description VARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- Drop table if it exists
IF OBJECT_ID('Patients', 'U') IS NOT NULL
    DROP TABLE Patients;
GO

CREATE TABLE Patients (
    PatientId INT IDENTITY(1,1) PRIMARY KEY,
    FullName VARCHAR(150) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    Phone VARCHAR(50) NOT NULL,
    CreatedBy INT NULL,          
	CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Patient_Email UNIQUE(Email),
    CONSTRAINT UQ_Patient_Phone UNIQUE(Phone)
);

ALTER TABLE Patients
ADD DateOfBirth DATE NULL,
    Gender VARCHAR(10) NULL,
    Address VARCHAR(200) NULL;


select * from AuditLogs