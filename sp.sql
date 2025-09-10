-- Drop existing SP if exists
IF OBJECT_ID('sp_GetUserByUsername', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetUserByUsername;
GO

CREATE PROCEDURE sp_GetUserByUsername
    @Username VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId, u.Username, u.PasswordHash, u.FullName, u.Email, u.Phone,
           u.RoleId, r.RoleName, u.IsActive, u.CreatedAt, u.UpdatedAt
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    WHERE u.Username = @Username;
END
GO

-- Drop existing SP if exists
IF OBJECT_ID('sp_CreateUser', 'P') IS NOT NULL
    DROP PROCEDURE sp_CreateUser;
GO

CREATE PROCEDURE sp_CreateUser
    @Username VARCHAR(100),
    @PasswordHash VARCHAR(500),
    @FullName VARCHAR(200),
    @Email VARCHAR(200) = NULL,
    @Phone VARCHAR(50) = NULL,
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Users (Username, PasswordHash, FullName, Email, Phone, RoleId, IsActive, CreatedAt)
    VALUES (@Username, @PasswordHash, @FullName, @Email, @Phone, @RoleId, 1, GETUTCDATE());

    -- Return the created user
    SELECT u.UserId, u.Username, u.PasswordHash, u.FullName, u.Email, u.Phone,
           u.RoleId, r.RoleName, u.IsActive, u.CreatedAt
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    WHERE u.UserId = SCOPE_IDENTITY();
END
GO

-- Drop existing SP if exists
IF OBJECT_ID('sp_AddAuditLog', 'P') IS NOT NULL
    DROP PROCEDURE sp_AddAuditLog;
GO

CREATE PROCEDURE sp_AddAuditLog
    @CorrelationId UNIQUEIDENTIFIER,
    @UserId INT = NULL,
    @Action VARCHAR(200),
    @Description VARCHAR(MAX),
    @CreatedAt DATETIME
AS
BEGIN
    INSERT INTO AuditLogs (CorrelationId, UserId, Action, Description, CreatedAt)
    VALUES (@CorrelationId, @UserId, @Action, @Description, @CreatedAt);
END
GO

-- Drop existing SP if exists
IF OBJECT_ID('sp_GetAuditLogs', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetAuditLogs;
GO

CREATE PROCEDURE sp_GetAuditLogs
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.AuditId, a.CorrelationId, a.UserId, u.Username, a.Action, a.Description, a.CreatedAt
    FROM AuditLogs a
    LEFT JOIN Users u ON a.UserId = u.UserId
    ORDER BY a.CreatedAt DESC;
END
GO

IF OBJECT_ID('sp_CreatePatient', 'P') IS NOT NULL
    DROP PROCEDURE sp_CreatePatient;
GO

Create PROCEDURE sp_CreatePatient
    @FullName VARCHAR(150),
    @Email VARCHAR(150),
    @Phone VARCHAR(50),
    @DateOfBirth DATE,
    @Gender VARCHAR(10),
    @Address VARCHAR(200),
    @PatientId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check for duplicates
    IF EXISTS(SELECT 1 FROM Patients WHERE Email = @Email OR Phone = @Phone)
    BEGIN
        RAISERROR('Potential duplicate detected', 16, 1);
        RETURN;
    END

    INSERT INTO Patients (FullName, Email, Phone, DateOfBirth, Gender, Address)
    VALUES (@FullName, @Email, @Phone, @DateOfBirth, @Gender, @Address);

    SET @PatientId = SCOPE_IDENTITY();
END
GO


IF OBJECT_ID('sp_FindPatientDuplicates', 'P') IS NOT NULL
    DROP PROCEDURE sp_FindPatientDuplicates;
GO

CREATE PROCEDURE sp_FindPatientDuplicates
    @FullName VARCHAR(150) = NULL,
    @Email VARCHAR(150) = NULL,
    @Phone VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM Patients
    WHERE (@FullName IS NOT NULL AND FullName LIKE '%' + @FullName + '%')
       OR (@Email IS NOT NULL AND Email = @Email)
       OR (@Phone IS NOT NULL AND Phone = @Phone);
END
GO

IF OBJECT_ID('sp_SearchPatients', 'P') IS NOT NULL
    DROP PROCEDURE sp_SearchPatients;
GO
CREATE PROCEDURE sp_SearchPatients
    @Query VARCHAR(150) = NULL,
    @Page INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    -- Calculate paging
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Main query with search and paging
    SELECT 
        PatientId,
        FullName,
        Email,
        Phone,
        DateOfBirth,
        Gender,
        Address,
        CreatedAt
    FROM Patients
    WHERE 
        (@Query IS NULL OR
         FullName LIKE '%' + @Query + '%' OR
         Email LIKE '%' + @Query + '%' OR
         Phone LIKE '%' + @Query + '%')
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- Total count for pagination
    SELECT COUNT(*) AS TotalCount
    FROM Patients
    WHERE 
        (@Query IS NULL OR
         FullName LIKE '%' + @Query + '%' OR
         Email LIKE '%' + @Query + '%' OR
         Phone LIKE '%' + @Query + '%');
END
GO