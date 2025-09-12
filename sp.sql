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
IF OBJECT_ID('sp_SearchUsers', 'P') IS NOT NULL
    DROP PROCEDURE sp_SearchUsers;
GO

CREATE PROCEDURE sp_SearchUsers
    @Query VARCHAR(150) = NULL,
    @Page INT = 1,
    @PageSize INT = 1000
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT 
        u.UserId,
        u.Username,
        u.FullName,
        u.Email,
        u.Phone,
        u.RoleId,
        r.RoleName,
        u.IsActive,
        u.CreatedAt
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    WHERE (@Query IS NULL OR u.FullName LIKE '%' + @Query + '%' OR u.Username LIKE '%' + @Query + '%' OR u.Email LIKE '%' + @Query + '%')
    ORDER BY u.FullName
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- Total count for pagination
    SELECT COUNT(*) AS TotalCount
    FROM Users u
    WHERE (@Query IS NULL OR u.FullName LIKE '%' + @Query + '%' OR u.Username LIKE '%' + @Query + '%' OR u.Email LIKE '%' + @Query + '%');
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

-- Drop if exists
IF OBJECT_ID('sp_CreateAppointment', 'P') IS NOT NULL
    DROP PROCEDURE sp_CreateAppointment;
GO

CREATE PROCEDURE sp_CreateAppointment
    @PatientId INT,
    @ProviderId INT,
    @StartTime DATETIME,
    @DurationMinutes INT,
    @AppointmentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Enforce allowed durations
    IF @DurationMinutes NOT IN (15, 30, 60)
    BEGIN
        RAISERROR('Invalid duration. Only 15, 30, or 60 minutes allowed.', 16, 1);
        RETURN;
    END

    -- Enforce business hours (example: 08:00 to 18:00)
    IF DATEPART(HOUR, @StartTime) < 8 OR DATEPART(HOUR, DATEADD(MINUTE, @DurationMinutes, @StartTime)) > 18
    BEGIN
        RAISERROR('Appointment must be within business hours (08:00-18:00).', 16, 1);
        RETURN;
    END

    -- Prevent double-booking for provider
    IF EXISTS (
        SELECT 1 FROM Appointments
        WHERE ProviderId = @ProviderId
          AND Status = 'Scheduled'
          AND (
                (@StartTime >= StartTime AND @StartTime < DATEADD(MINUTE, DurationMinutes, StartTime))
             OR (DATEADD(MINUTE, @DurationMinutes, @StartTime) > StartTime AND DATEADD(MINUTE, @DurationMinutes, @StartTime) <= DATEADD(MINUTE, DurationMinutes, StartTime))
             OR (@StartTime <= StartTime AND DATEADD(MINUTE, @DurationMinutes, @StartTime) >= DATEADD(MINUTE, DurationMinutes, StartTime))
          )
    )
    BEGIN
        RAISERROR('Provider is already booked for this time slot.', 16, 1);
        RETURN;
    END

    INSERT INTO Appointments (PatientId, ProviderId, StartTime, DurationMinutes)
    VALUES (@PatientId, @ProviderId, @StartTime, @DurationMinutes);

    SET @AppointmentId = SCOPE_IDENTITY();
END
GO

-- Drop if exists
IF OBJECT_ID('sp_GetProviderAppointments', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetProviderAppointments;
GO

CREATE PROCEDURE sp_GetProviderAppointments
    @ProviderId INT,
    @WeekStart DATETIME, -- Monday 00:00 of the week
    @WeekEnd DATETIME    -- Sunday 23:59 of the week
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.*, p.FullName AS PatientName
    FROM Appointments a
    INNER JOIN Patients p ON a.PatientId = p.PatientId
    WHERE a.ProviderId = @ProviderId
      AND a.StartTime >= @WeekStart
      AND a.StartTime < @WeekEnd
    ORDER BY a.StartTime;
END
GO

-- Drop existing SP if exists
IF OBJECT_ID('sp_UpdateAppointment', 'P') IS NOT NULL
    DROP PROCEDURE sp_UpdateAppointment;
GO

CREATE PROCEDURE sp_UpdateAppointment
    @AppointmentId INT,
    @PatientId INT,
    @ProviderId INT,
    @StartTime DATETIME,
    @DurationMinutes INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Enforce allowed durations
        IF @DurationMinutes NOT IN (15, 30, 60)
        BEGIN
            RAISERROR('Invalid duration. Only 15, 30, or 60 minutes allowed.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Enforce business hours (example: 08:00 to 18:00)
        IF DATEPART(HOUR, @StartTime) < 8 OR DATEPART(HOUR, DATEADD(MINUTE, @DurationMinutes, @StartTime)) > 18
        BEGIN
            RAISERROR('Appointment must be within business hours (08:00-18:00).', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Prevent double-booking for provider (exclude current appointment)
        IF EXISTS (
            SELECT 1 FROM Appointments
            WHERE ProviderId = @ProviderId
              AND Status = 'Scheduled'
              AND AppointmentId <> @AppointmentId
              AND (
                    (@StartTime >= StartTime AND @StartTime < DATEADD(MINUTE, DurationMinutes, StartTime))
                 OR (DATEADD(MINUTE, @DurationMinutes, @StartTime) > StartTime AND DATEADD(MINUTE, @DurationMinutes, @StartTime) <= DATEADD(MINUTE, DurationMinutes, StartTime))
                 OR (@StartTime <= StartTime AND DATEADD(MINUTE, @DurationMinutes, @StartTime) >= DATEADD(MINUTE, DurationMinutes, StartTime))
              )
        )
        BEGIN
            RAISERROR('Provider is already booked for this time slot.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        UPDATE Appointments
        SET PatientId = @PatientId,
            ProviderId = @ProviderId,
            StartTime = @StartTime,
            DurationMinutes = @DurationMinutes,
            UpdatedAt = GETUTCDATE()
        WHERE AppointmentId = @AppointmentId;

        -- Return updated appointment
        SELECT * FROM Appointments WHERE AppointmentId = @AppointmentId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Drop existing SP if exists
IF OBJECT_ID('sp_GetBusinessHours', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetBusinessHours;
GO

CREATE PROCEDURE sp_GetBusinessHours
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SettingKey, SettingValue FROM BusinessSettings WHERE SettingKey IN ('BusinessStart', 'BusinessEnd');
END
GO


-- Add status history on appointment creation
ALTER PROCEDURE sp_CreateAppointment
    @PatientId INT,
    @ProviderId INT,
    @StartTime DATETIME,
    @DurationMinutes INT,
    @CreatedBy INT = NULL, -- UserId
    @AppointmentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Appointments (PatientId, ProviderId, StartTime, DurationMinutes, Status, CreatedAt)
    VALUES (@PatientId, @ProviderId, @StartTime, @DurationMinutes, 'Scheduled', GETUTCDATE());

    SET @AppointmentId = SCOPE_IDENTITY();

    -- Insert status history
    INSERT INTO AppointmentStatusHistory (AppointmentId, OldStatus, NewStatus, ChangedAt, ChangedBy)
    VALUES (@AppointmentId, NULL, 'Scheduled', GETUTCDATE(), @CreatedBy);
END
GO

-- Add status history on appointment update
ALTER PROCEDURE sp_UpdateAppointment
    @AppointmentId INT,
    @PatientId INT,
    @ProviderId INT,
    @StartTime DATETIME,
    @DurationMinutes INT,
    @NewStatus VARCHAR(20),
    @ChangedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldStatus VARCHAR(20);
    SELECT @OldStatus = Status FROM Appointments WHERE AppointmentId = @AppointmentId;

    UPDATE Appointments
    SET PatientId = @PatientId,
        ProviderId = @ProviderId,
        StartTime = @StartTime,
        DurationMinutes = @DurationMinutes,
        Status = @NewStatus,
        UpdatedAt = GETUTCDATE()
    WHERE AppointmentId = @AppointmentId;

    -- Insert status history
    INSERT INTO AppointmentStatusHistory (AppointmentId, OldStatus, NewStatus, ChangedAt, ChangedBy)
    VALUES (@AppointmentId, @OldStatus, @NewStatus, GETUTCDATE(), @ChangedBy);

    -- Return updated row
    SELECT * FROM Appointments WHERE AppointmentId = @AppointmentId;
END
GO
