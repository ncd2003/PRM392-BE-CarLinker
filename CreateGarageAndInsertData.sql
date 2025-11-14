-- ============================================================
-- CREATE GARAGE TABLE AND INSERT 2 SAMPLE GARAGES
-- Database: CarLinker
-- ============================================================

USE [CarLinker];
GO

PRINT N'🔍 Checking if Garage table exists...';
GO

-- STEP 1: Create Garage table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Garage')
BEGIN
    PRINT N'📋 Creating Garage table...';
    
    CREATE TABLE [dbo].[Garage] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [OperatingTime] nvarchar(100) NOT NULL,
        [PhoneNumber] nvarchar(15) NOT NULL,
        [Image] nvarchar(255) NULL,
        [Latitude] nvarchar(50) NOT NULL,
        [Longitude] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetimeoffset NULL DEFAULT SYSDATETIMEOFFSET(),
        [UpdatedAt] datetimeoffset NULL DEFAULT SYSDATETIMEOFFSET(),
        CONSTRAINT [PK_Garage] PRIMARY KEY ([Id])
    );
    
    PRINT N'✅ Garage table created successfully!';
END
ELSE
BEGIN
    PRINT N'✅ Garage table already exists.';
END
GO

-- STEP 2: Add Foreign Key if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE name = 'FK_Garage_User_UserId' 
    AND parent_object_id = OBJECT_ID('Garage')
)
BEGIN
    PRINT N'🔗 Adding Foreign Key constraint...';
    
    ALTER TABLE [dbo].[Garage]
    ADD CONSTRAINT [FK_Garage_User_UserId] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]) ON DELETE NO ACTION;
    
    PRINT N'✅ Foreign Key added successfully!';
END
ELSE
BEGIN
    PRINT N'✅ Foreign Key already exists.';
END
GO

-- STEP 3: Create Index if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Garage_UserId' AND object_id = OBJECT_ID('Garage'))
BEGIN
    PRINT N'📊 Creating index on UserId...';
    
    CREATE INDEX [IX_Garage_UserId] ON [dbo].[Garage] ([UserId]);
    
    PRINT N'✅ Index created successfully!';
END
ELSE
BEGIN
    PRINT N'✅ Index already exists.';
END
GO

-- STEP 4: Insert 2 Garage records
PRINT N'';
PRINT N'📝 Inserting garage records...';
GO

BEGIN TRANSACTION;

-- Check if we have the required users
DECLARE @User1Id int = (SELECT Id FROM [dbo].[User] WHERE Email = 'owner.thanglong@partner.com');
DECLARE @User2Id int = (SELECT Id FROM [dbo].[User] WHERE Email = 'owner.sieutoc@partner.com');

IF @User1Id IS NULL OR @User2Id IS NULL
BEGIN
    PRINT N'❌ ERROR: Required users not found!';
    PRINT N'   Please ensure these users exist:';
    PRINT N'   - owner.thanglong@partner.com';
    PRINT N'   - owner.sieutoc@partner.com';
    ROLLBACK TRANSACTION;
END
ELSE
BEGIN
    -- Garage 1: Gara Thăng Long
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Garage] WHERE UserId = @User1Id)
    BEGIN
        INSERT INTO [dbo].[Garage]
            ([UserId], [Name], [Email], [Description], [OperatingTime], [PhoneNumber], 
             [Image], [Latitude], [Longitude], [IsActive], [CreatedAt], [UpdatedAt])
        VALUES
        (
            @User1Id,
            N'Gara Thăng Long',
            N'contact.thanglong@garage.com',
            N'Chuyên sửa chữa và bảo dưỡng ô tô chuyên nghiệp tại Hà Nội. Đội ngũ thợ lành nghề, trang thiết bị hiện đại.',
            N'Thứ 2 - Thứ 7: 08:00 - 18:00, Chủ nhật: 08:00 - 12:00',
            N'0911234567',
            NULL,
            N'21.028511',
            N'105.804817',
            1,
            SYSDATETIMEOFFSET(),
            SYSDATETIMEOFFSET()
        );
        
        PRINT N'✅ Garage 1 inserted: Gara Thăng Long';
    END
    ELSE
    BEGIN
        PRINT N'⚠️  Garage for user owner.thanglong@partner.com already exists, skipping...';
    END

    -- Garage 2: Gara Siêu Tốc
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Garage] WHERE UserId = @User2Id)
    BEGIN
        INSERT INTO [dbo].[Garage]
            ([UserId], [Name], [Email], [Description], [OperatingTime], [PhoneNumber], 
             [Image], [Latitude], [Longitude], [IsActive], [CreatedAt], [UpdatedAt])
        VALUES
        (
            @User2Id,
            N'Gara Siêu Tốc',
            N'contact.sieutoc@garage.com',
            N'Dịch vụ sửa chữa nhanh chóng, uy tín tại TP.HCM. Chuyên các dòng xe sedan và SUV. Phụ tùng chính hãng.',
            N'Thứ 2 - Chủ nhật: 07:30 - 19:00',
            N'0912345678',
            NULL,
            N'10.762622',
            N'106.660172',
            1,
            SYSDATETIMEOFFSET(),
            SYSDATETIMEOFFSET()
        );
        
        PRINT N'✅ Garage 2 inserted: Gara Siêu Tốc';
    END
    ELSE
    BEGIN
        PRINT N'⚠️  Garage for user owner.sieutoc@partner.com already exists, skipping...';
    END

    COMMIT TRANSACTION;
    
    PRINT N'';
    PRINT N'========================================';
    PRINT N'✅ COMPLETED SUCCESSFULLY!';
    PRINT N'========================================';
END
GO

-- STEP 5: Display inserted garages
PRINT N'';
PRINT N'📋 Current Garages in database:';
GO

SELECT 
    g.Id,
    g.Name,
    g.Email,
    u.FullName AS [Owner],
    u.Email AS [OwnerEmail],
    g.PhoneNumber,
    g.OperatingTime,
    g.IsActive,
    g.CreatedAt
FROM [dbo].[Garage] g
INNER JOIN [dbo].[User] u ON g.UserId = u.Id
ORDER BY g.Id;
GO

PRINT N'';
PRINT N'✅ Script execution completed!';
GO
