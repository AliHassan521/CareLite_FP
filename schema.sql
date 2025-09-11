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

-- Drop if exists
IF OBJECT_ID('Appointments', 'U') IS NOT NULL
    DROP TABLE Appointments;
GO

CREATE TABLE Appointments (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    ProviderId INT NOT NULL, -- UserId of provider (Clinician)
    StartTime DATETIME NOT NULL,
    DurationMinutes INT NOT NULL, -- 15, 30, or 60
    Status VARCHAR(20) NOT NULL DEFAULT 'Scheduled', -- Scheduled, Completed, Cancelled, No-Show
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Appointment_Patient FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    CONSTRAINT FK_Appointment_Provider FOREIGN KEY (ProviderId) REFERENCES Users(UserId)
);
GO

-- Dummy Patients
INSERT INTO Patients (FullName, Email, Phone, DateOfBirth, Gender, Address)
VALUES
('Jane Smith', 'jane.smith@example.com', '2345678901', '1990-05-15', 'Female', '456 Oak Ave'),
('Alice Johnson', 'alice.johnson@example.com', '3456789012', '1978-09-23', 'Female', '789 Pine Rd'),
('Bob Brown', 'bob.brown@example.com', '4567890123', '1982-12-11', 'Male', '321 Maple Dr');

-- Dummy Users (with roles)
-- First, get RoleIds
-- SELECT * FROM Roles;
-- Assume: 1=Admin, 2=Staff, 3=Clinician
INSERT INTO Users (Username, PasswordHash, FullName, Email, Phone, RoleId, IsActive, CreatedAt)
VALUES
('admin1', '$2a$11$Qe6Qw6Qw6Qw6Qw6Qw6Qw6uQw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6', 'Admin User', 'admin1@clinic.com', '5551112222', 1, 1, GETUTCDATE()),
('staff1', '$2a$11$Qe6Qw6Qw6Qw6Qw6Qw6Qw6uQw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6', 'Staff User', 'staff1@clinic.com', '5552223333', 2, 1, GETUTCDATE()),
('clinician1', '$2a$11$Qe6Qw6Qw6Qw6Qw6Qw6Qw6uQw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6', 'Dr. Alice Clinician', 'clinician1@clinic.com', '5553334444', 3, 1, GETUTCDATE()),
('clinician2', '$2a$11$Qe6Qw6Qw6Qw6Qw6Qw6Qw6uQw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6Qw6', 'Dr. Bob Clinician', 'clinician2@clinic.com', '5554445555', 3, 1, GETUTCDATE());

select * from Patients