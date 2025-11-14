-- ============================================================
-- FIX MIGRATION HISTORY AND CREATE MISSING GARAGE TABLE
-- Database: CarLinker
-- ============================================================

USE [CarLinker];
GO

-- BƯỚC 1: Cập nhật Migration History để mark migrations đã apply
-- (vì database đã có các bảng từ migrations cũ)
PRINT N'🔄 Đang cập nhật Migration History...';

-- Xóa tất cả migration history cũ để tránh conflict
DELETE FROM [__EFMigrationsHistory];
GO

-- Thêm migration history cho AddChatTables (migration đã apply thực tế)
INSERT INTO [__EFMigrationsHistory] (MigrationId, ProductVersion)
VALUES 
    ('20251112163222_AddChatTables', '8.0.11'),
    ('20251112172445_AddUser_ImageColumn', '8.0.11');
GO

PRINT N'✅ Migration History đã được cập nhật';
GO

-- BƯỚC 2: Tạo bảng GARAGE (thiếu từ migration TenMigration)
PRINT N'🔄 Đang tạo bảng Garage...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Garage')
BEGIN
    CREATE TABLE [dbo].[Garage] (
        [Id] int NOT NULL IDENTITY,
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
        [CreatedAt] datetimeoffset NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_Garage] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Garage_User_UserId] FOREIGN KEY ([UserId]) 
            REFERENCES [User] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_Garage_UserId] ON [Garage] ([UserId]);
    
    PRINT N'✅ Bảng Garage đã được tạo';
END
ELSE
BEGIN
    PRINT N'⚠️  Bảng Garage đã tồn tại';
END
GO

-- BƯỚC 3: Tạo bảng GARAGESTAFF (nếu chưa có)
PRINT N'🔄 Đang tạo bảng GarageStaff...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GarageStaff')
BEGIN
    CREATE TABLE [dbo].[GarageStaff] (
        [Id] int NOT NULL IDENTITY,
        [GarageId] int NULL,
        [FullName] nvarchar(100) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [PhoneNumber] nvarchar(15) NOT NULL,
        [PasswordHash] nvarchar(255) NOT NULL,
        [Position] nvarchar(50) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetimeoffset NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_GarageStaff] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GarageStaff_Garage_GarageId] FOREIGN KEY ([GarageId]) 
            REFERENCES [Garage] ([Id]) ON DELETE SET NULL
    );

    CREATE UNIQUE INDEX [UX_GarageStaff_Email] ON [GarageStaff] ([Email]);
    CREATE INDEX [IX_GarageStaff_PhoneNumber] ON [GarageStaff] ([PhoneNumber]);
    CREATE INDEX [IX_GarageStaff_GarageId_IsActive] ON [GarageStaff] ([GarageId], [IsActive]);
    
    PRINT N'✅ Bảng GarageStaff đã được tạo';
END
ELSE
BEGIN
    PRINT N'⚠️  Bảng GarageStaff đã tồn tại';
END
GO

-- BƯỚC 4: Tạo bảng GARAGESERVICEITEM (nếu chưa có)
PRINT N'🔄 Đang tạo bảng GarageServiceItem...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GarageServiceItem')
BEGIN
    CREATE TABLE [dbo].[GarageServiceItem] (
        [GarageId] int NOT NULL,
        [ServiceItemId] int NOT NULL,
        [Id] int NOT NULL IDENTITY,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetimeoffset NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_GarageServiceItem] PRIMARY KEY ([GarageId], [ServiceItemId]),
        CONSTRAINT [FK_GarageServiceItem_Garage_GarageId] FOREIGN KEY ([GarageId]) 
            REFERENCES [Garage] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GarageServiceItem_ServiceItem_ServiceItemId] FOREIGN KEY ([ServiceItemId]) 
            REFERENCES [ServiceItem] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_GarageServiceItem_ServiceItemId] ON [GarageServiceItem] ([ServiceItemId]);
    
    PRINT N'✅ Bảng GarageServiceItem đã được tạo';
END
ELSE
BEGIN
    PRINT N'⚠️  Bảng GarageServiceItem đã tồn tại';
END
GO

-- BƯỚC 5: Thêm Foreign Key cho ServiceRecord nếu chưa có
PRINT N'🔄 Đang kiểm tra Foreign Key ServiceRecord -> Garage...';

IF NOT EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE name = 'FK_ServiceRecord_Garage_GarageId' 
    AND parent_object_id = OBJECT_ID('ServiceRecord')
)
BEGIN
    -- Thêm cột GarageId nếu chưa có
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ServiceRecord') AND name = 'GarageId')
    BEGIN
        ALTER TABLE [ServiceRecord] ADD [GarageId] int NOT NULL DEFAULT 0;
    END

    -- Tạo Foreign Key
    ALTER TABLE [ServiceRecord]
    ADD CONSTRAINT [FK_ServiceRecord_Garage_GarageId] 
    FOREIGN KEY ([GarageId]) REFERENCES [Garage] ([Id]) ON DELETE NO ACTION;

    CREATE INDEX [IX_ServiceRecord_GarageId] ON [ServiceRecord] ([GarageId]);
    
    PRINT N'✅ Foreign Key ServiceRecord -> Garage đã được tạo';
END
ELSE
BEGIN
    PRINT N'⚠️  Foreign Key ServiceRecord -> Garage đã tồn tại';
END
GO

-- BƯỚC 6: Mark migration TenMigration là đã apply
PRINT N'🔄 Đang mark TenMigration là đã apply...';

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE MigrationId = '20251112191535_TenMigration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] (MigrationId, ProductVersion)
    VALUES ('20251112191535_TenMigration', '8.0.11');
    
    PRINT N'✅ Migration TenMigration đã được mark là applied';
END
GO

PRINT N'';
PRINT N'========================================';
PRINT N'✅ HOÀN THÀNH! Database đã được cập nhật';
PRINT N'========================================';
PRINT N'';
PRINT N'Các bảng đã tạo/cập nhật:';
PRINT N'  ✓ Garage';
PRINT N'  ✓ GarageStaff';
PRINT N'  ✓ GarageServiceItem';
PRINT N'  ✓ ServiceRecord (thêm FK to Garage)';
PRINT N'';
GO
