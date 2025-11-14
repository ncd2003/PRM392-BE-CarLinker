-- =============================================
-- Fix Migration History and Create Missing Tables
-- =============================================
USE CarLinker;
GO

SET QUOTED_IDENTIFIER ON;
GO

PRINT '========================================';
PRINT 'Fixing Migration History';
PRINT '========================================';
PRINT '';

-- Add TenMigration to history if not exists
IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20251112191535_TenMigration')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20251112191535_TenMigration', '8.0.20');
    PRINT '  -> Added TenMigration to history';
END
ELSE
BEGIN
    PRINT '  -> TenMigration already in history';
END

-- Add AddGarageTable to history if not exists
IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20251113161333_AddGarageTable')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20251113161333_AddGarageTable', '8.0.20');
    PRINT '  -> Added AddGarageTable to history';
END
ELSE
BEGIN
    PRINT '  -> AddGarageTable already in history';
END

PRINT '';
PRINT '========================================';
PRINT 'Creating Missing Tables';
PRINT '========================================';
PRINT '';

-- Create ServiceCategory if missing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceCategory]') AND type in (N'U'))
BEGIN
    PRINT '  -> Creating ServiceCategory table...';
    CREATE TABLE [dbo].[ServiceCategory] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(MAX) NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIMEOFFSET NULL DEFAULT SYSDATETIMEOFFSET(),
        [UpdatedAt] DATETIMEOFFSET NULL,
        CONSTRAINT [PK_ServiceCategory] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT '  -> ServiceCategory created';
END
ELSE
BEGIN
    PRINT '  -> ServiceCategory already exists';
END

-- Create ServiceRecord if missing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceRecord]') AND type in (N'U'))
BEGIN
    PRINT '  -> Creating ServiceRecord table...';
    CREATE TABLE [dbo].[ServiceRecord] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ServiceRecordStatus] INT NOT NULL,
        [TotalCost] DECIMAL(18,2) NULL,
        [StartTime] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [EndTime] DATETIME2 NULL,
        [UserId] INT NOT NULL,
        [StaffId] INT NULL,
        [GarageId] INT NOT NULL,
        [VehicleId] INT NOT NULL,
        [CreatedAt] DATETIMEOFFSET NULL DEFAULT SYSDATETIMEOFFSET(),
        [UpdatedAt] DATETIMEOFFSET NULL,
        CONSTRAINT [PK_ServiceRecord] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ServiceRecord_Garage_GarageId] FOREIGN KEY([GarageId])
            REFERENCES [dbo].[Garage] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ServiceRecord_User_StaffId] FOREIGN KEY([StaffId])
            REFERENCES [dbo].[User] ([Id]),
        CONSTRAINT [FK_ServiceRecord_User_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[User] ([Id]),
        CONSTRAINT [FK_ServiceRecord_Vehicle_VehicleId] FOREIGN KEY([VehicleId])
            REFERENCES [dbo].[Vehicle] ([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ServiceRecord_GarageId] ON [dbo].[ServiceRecord]([GarageId] ASC);
    CREATE NONCLUSTERED INDEX [IX_ServiceRecord_StaffId] ON [dbo].[ServiceRecord]([StaffId] ASC);
    CREATE NONCLUSTERED INDEX [IX_ServiceRecord_UserId] ON [dbo].[ServiceRecord]([UserId] ASC);
    CREATE NONCLUSTERED INDEX [IX_ServiceRecord_VehicleId] ON [dbo].[ServiceRecord]([VehicleId] ASC);
    
    PRINT '  -> ServiceRecord created';
END
ELSE
BEGIN
    PRINT '  -> ServiceRecord already exists';
END

-- Create ServiceItem if missing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceItem]') AND type in (N'U'))
BEGIN
    PRINT '  -> Creating ServiceItem table...';
    CREATE TABLE [dbo].[ServiceItem] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(MAX) NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [ServiceCategoryId] INT NULL,
        [ServiceRecordId] INT NULL,
        [CreatedAt] DATETIMEOFFSET NULL DEFAULT SYSDATETIMEOFFSET(),
        [UpdatedAt] DATETIMEOFFSET NULL,
        CONSTRAINT [PK_ServiceItem] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ServiceItem_ServiceCategory_ServiceCategoryId] FOREIGN KEY([ServiceCategoryId])
            REFERENCES [dbo].[ServiceCategory] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_ServiceItem_ServiceRecord_ServiceRecordId] FOREIGN KEY([ServiceRecordId])
            REFERENCES [dbo].[ServiceRecord] ([Id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_ServiceItem_ServiceCategoryId] ON [dbo].[ServiceItem]([ServiceCategoryId] ASC);
    CREATE NONCLUSTERED INDEX [IX_ServiceItem_ServiceRecordId] ON [dbo].[ServiceItem]([ServiceRecordId] ASC);
    
    PRINT '  -> ServiceItem created';
END
ELSE
BEGIN
    PRINT '  -> ServiceItem already exists';
END

-- Create GarageServiceItem if missing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GarageServiceItem]') AND type in (N'U'))
BEGIN
    PRINT '  -> Creating GarageServiceItem table...';
    CREATE TABLE [dbo].[GarageServiceItem] (
        [GarageId] INT NOT NULL,
        [ServiceItemId] INT NOT NULL,
        [Price] DECIMAL(18,2) NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_GarageServiceItem] PRIMARY KEY CLUSTERED ([GarageId], [ServiceItemId]),
        CONSTRAINT [FK_GarageServiceItem_Garage_GarageId] FOREIGN KEY([GarageId])
            REFERENCES [dbo].[Garage] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GarageServiceItem_ServiceItem_ServiceItemId] FOREIGN KEY([ServiceItemId])
            REFERENCES [dbo].[ServiceItem] ([Id]) ON DELETE CASCADE
    );
    
    CREATE NONCLUSTERED INDEX [IX_GarageServiceItem_ServiceItemId] ON [dbo].[GarageServiceItem]([ServiceItemId] ASC);
    
    PRINT '  -> GarageServiceItem created';
END
ELSE
BEGIN
    PRINT '  -> GarageServiceItem already exists';
END

PRINT '';
PRINT '========================================';
PRINT 'Migration History Updated:';
PRINT '========================================';

SELECT MigrationId, ProductVersion 
FROM __EFMigrationsHistory 
ORDER BY MigrationId;

PRINT '';
PRINT '========================================';
PRINT 'Service Tables Status:';
PRINT '========================================';

SELECT 
    'ServiceCategory' AS TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceCategory]')) THEN 'EXISTS' ELSE 'MISSING' END AS Status
UNION ALL
SELECT 
    'ServiceRecord',
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceRecord]')) THEN 'EXISTS' ELSE 'MISSING' END
UNION ALL
SELECT 
    'ServiceItem',
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceItem]')) THEN 'EXISTS' ELSE 'MISSING' END
UNION ALL
SELECT 
    'GarageServiceItem',
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GarageServiceItem]')) THEN 'EXISTS' ELSE 'MISSING' END;

PRINT '';
PRINT '========================================';
PRINT 'Fix Complete!';
PRINT '========================================';

GO
