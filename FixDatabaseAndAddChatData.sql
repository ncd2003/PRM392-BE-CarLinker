-- =============================================
-- Fix Database: Create Garage Table & Clean Garbage Data
-- =============================================
-- This script will:
-- 1. Create Garage table if missing
-- 2. Delete invalid ChatRoom and ChatMessage records
-- 3. Add sample garage owners to User table
-- 4. Add sample garages
-- 5. Add valid chat data for testing
-- =============================================

USE CarLinker;
GO

SET QUOTED_IDENTIFIER ON;
GO

PRINT '========================================';
PRINT 'Starting Database Cleanup and Setup';
PRINT '========================================';
PRINT '';

-- =============================================
-- STEP 1: Create Garage Table if Missing
-- =============================================
PRINT 'STEP 1: Checking Garage table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Garage]') AND type in (N'U'))
BEGIN
    PRINT '  -> Creating Garage table...';
    
    CREATE TABLE [dbo].[Garage] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(MAX) NOT NULL,
        [Email] NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [OperatingTime] NVARCHAR(MAX) NOT NULL,
        [PhoneNumber] NVARCHAR(MAX) NOT NULL,
        [Image] NVARCHAR(MAX) NULL,
        [Latitude] NVARCHAR(MAX) NULL,
        [Longitude] NVARCHAR(MAX) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [UserId] INT NOT NULL,
        [CreatedAt] DATETIMEOFFSET NULL DEFAULT SYSDATETIMEOFFSET(),
        [UpdatedAt] DATETIMEOFFSET NULL,
        CONSTRAINT [PK_Garage] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Garage_User_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[User] ([Id]) ON DELETE NO ACTION
    );
    
    CREATE NONCLUSTERED INDEX [IX_Garage_UserId] ON [dbo].[Garage]([UserId] ASC);
    CREATE NONCLUSTERED INDEX [IX_Garage_IsActive] ON [dbo].[Garage]([IsActive] ASC);
    
    PRINT '  -> Garage table created successfully!';
END
ELSE
BEGIN
    PRINT '  -> Garage table already exists.';
END
PRINT '';

-- =============================================
-- STEP 2: Clean Up Garbage Data
-- =============================================
PRINT 'STEP 2: Cleaning up invalid data...';

-- Count records before cleanup
DECLARE @ChatRoomCount INT, @ChatMessageCount INT;
SELECT @ChatRoomCount = COUNT(*) FROM ChatRoom WHERE GarageId NOT IN (SELECT Id FROM Garage);
SELECT @ChatMessageCount = COUNT(*) FROM ChatMessage WHERE RoomId NOT IN (SELECT Id FROM ChatRoom WHERE GarageId IN (SELECT Id FROM Garage));

PRINT '  -> Found ' + CAST(@ChatRoomCount AS VARCHAR(10)) + ' invalid ChatRoom records';
PRINT '  -> Found ' + CAST(@ChatMessageCount AS VARCHAR(10)) + ' invalid ChatMessage records';

-- Delete ChatMessages with invalid RoomId
DELETE FROM ChatMessage 
WHERE RoomId IN (
    SELECT Id FROM ChatRoom 
    WHERE GarageId NOT IN (SELECT Id FROM Garage)
);

-- Delete ChatRoomMembers with invalid RoomId
DELETE FROM ChatRoomMember 
WHERE RoomId IN (
    SELECT Id FROM ChatRoom 
    WHERE GarageId NOT IN (SELECT Id FROM Garage)
);

-- Delete ChatRooms with invalid GarageId
DELETE FROM ChatRoom 
WHERE GarageId NOT IN (SELECT Id FROM Garage);

-- Delete ChatRooms with invalid CustomerId
DELETE FROM ChatRoom 
WHERE CustomerId NOT IN (SELECT Id FROM [User] WHERE UserRole = 0); -- CUSTOMER = 0

PRINT '  -> Garbage data cleaned successfully!';
PRINT '';

-- =============================================
-- STEP 3: Add Sample Garage Owners to User Table
-- =============================================
PRINT 'STEP 3: Adding garage owners...';

-- Check if garage owner users exist
IF NOT EXISTS (SELECT * FROM [User] WHERE Email = 'owner.thanglong@partner.com')
BEGIN
    INSERT INTO [User] (FullName, Email, PhoneNumber, PasswordHash, UserRole, UserStatus, IsActive, CreatedAt)
    VALUES (
        N'Nguyễn Văn Thành',
        'owner.thanglong@partner.com',
        '0901234567',
        '$2a$11$hashed_password_here', -- In production, use proper password hashing
        1, -- GARAGE owner role
        1, -- ACTIVE status
        1,
        SYSDATETIMEOFFSET()
    );
    PRINT '  -> Added garage owner: Nguyễn Văn Thành';
END
ELSE
BEGIN
    PRINT '  -> Garage owner already exists: owner.thanglong@partner.com';
END

IF NOT EXISTS (SELECT * FROM [User] WHERE Email = 'owner.sieutoc@partner.com')
BEGIN
    INSERT INTO [User] (FullName, Email, PhoneNumber, PasswordHash, UserRole, UserStatus, IsActive, CreatedAt)
    VALUES (
        N'Trần Minh Hải',
        'owner.sieutoc@partner.com',
        '0907654321',
        '$2a$11$hashed_password_here', -- In production, use proper password hashing
        1, -- GARAGE owner role
        1, -- ACTIVE status
        1,
        SYSDATETIMEOFFSET()
    );
    PRINT '  -> Added garage owner: Trần Minh Hải';
END
ELSE
BEGIN
    PRINT '  -> Garage owner already exists: owner.sieutoc@partner.com';
END

-- Add a sample customer for chat testing
IF NOT EXISTS (SELECT * FROM [User] WHERE Email = 'customer.test@example.com')
BEGIN
    INSERT INTO [User] (FullName, Email, PhoneNumber, PasswordHash, UserRole, UserStatus, IsActive, CreatedAt)
    VALUES (
        N'Lê Văn Khách',
        'customer.test@example.com',
        '0912345678',
        '$2a$11$hashed_password_here',
        0, -- CUSTOMER role
        1, -- ACTIVE status
        1,
        SYSDATETIMEOFFSET()
    );
    PRINT '  -> Added test customer: Lê Văn Khách';
END
ELSE
BEGIN
    PRINT '  -> Test customer already exists: customer.test@example.com';
END

PRINT '';

-- =============================================
-- STEP 4: Add Sample Garages
-- =============================================
PRINT 'STEP 4: Adding sample garages...';

DECLARE @Owner1Id INT, @Owner2Id INT;
SELECT @Owner1Id = Id FROM [User] WHERE Email = 'owner.thanglong@partner.com';
SELECT @Owner2Id = Id FROM [User] WHERE Email = 'owner.sieutoc@partner.com';

IF NOT EXISTS (SELECT * FROM Garage WHERE Email = 'contact@garathanglong.vn')
BEGIN
    INSERT INTO Garage (Name, Email, Description, OperatingTime, PhoneNumber, Image, Latitude, Longitude, IsActive, UserId, CreatedAt)
    VALUES (
        N'Gara Thăng Long',
        'contact@garathanglong.vn',
        N'Garage chuyên sửa chữa và bảo dưỡng xe hơi tại Hà Nội. Đội ngũ kỹ thuật viên giàu kinh nghiệm, trang thiết bị hiện đại.',
        N'8:00 AM - 6:00 PM (Mon-Sat)',
        '0901234567',
        'https://example.com/images/garage1.jpg',
        '21.028511',
        '105.804817',
        1,
        @Owner1Id,
        SYSDATETIMEOFFSET()
    );
    PRINT '  -> Added garage: Gara Thăng Long';
END
ELSE
BEGIN
    PRINT '  -> Garage already exists: Gara Thăng Long';
END

IF NOT EXISTS (SELECT * FROM Garage WHERE Email = 'info@garasieutoc.vn')
BEGIN
    INSERT INTO Garage (Name, Email, Description, OperatingTime, PhoneNumber, Image, Latitude, Longitude, IsActive, UserId, CreatedAt)
    VALUES (
        N'Gara Siêu Tốc',
        'info@garasieutoc.vn',
        N'Trung tâm sửa chữa xe ô tô chuyên nghiệp tại TP.HCM. Cam kết sửa nhanh, giá tốt, chất lượng cao.',
        N'7:30 AM - 7:00 PM (Mon-Sun)',
        '0907654321',
        'https://example.com/images/garage2.jpg',
        '10.762622',
        '106.660172',
        1,
        @Owner2Id,
        SYSDATETIMEOFFSET()
    );
    PRINT '  -> Added garage: Gara Siêu Tốc';
END
ELSE
BEGIN
    PRINT '  -> Garage already exists: Gara Siêu Tốc';
END

PRINT '';

-- =============================================
-- STEP 5: Add Garage Staff
-- =============================================
PRINT 'STEP 5: Adding garage staff...';

DECLARE @Garage1Id INT, @Garage2Id INT;
SELECT @Garage1Id = Id FROM Garage WHERE Email = 'contact@garathanglong.vn';
SELECT @Garage2Id = Id FROM Garage WHERE Email = 'info@garasieutoc.vn';

IF NOT EXISTS (SELECT * FROM GarageStaff WHERE Email = 'staff.thanglong@garage.com')
BEGIN
    INSERT INTO GarageStaff (FullName, Email, PhoneNumber, PasswordHash, GarageRole, UserStatus, GarageId, IsActive, CreatedAt)
    VALUES (
        N'Phạm Văn Nam',
        'staff.thanglong@garage.com',
        '0981111111',
        '$2a$11$hashed_password_here', -- In production, use proper password hashing
        2, -- STAFF role
        1, -- ACTIVE status
        @Garage1Id,
        1,
        SYSDATETIMEOFFSET()
    );
    PRINT '  -> Added staff: Phạm Văn Nam (Gara Thăng Long)';
END
ELSE
BEGIN
    PRINT '  -> Staff already exists: staff.thanglong@garage.com';
END

IF NOT EXISTS (SELECT * FROM GarageStaff WHERE Email = 'staff.sieutoc@garage.com')
BEGIN
    INSERT INTO GarageStaff (FullName, Email, PhoneNumber, PasswordHash, GarageRole, UserStatus, GarageId, IsActive, CreatedAt)
    VALUES (
        N'Nguyễn Thị Lan',
        'staff.sieutoc@garage.com',
        '0982222222',
        '$2a$11$hashed_password_here', -- In production, use proper password hashing
        2, -- STAFF role
        1, -- ACTIVE status
        @Garage2Id,
        1,
        SYSDATETIMEOFFSET()
    );
    PRINT '  -> Added staff: Nguyễn Thị Lan (Gara Siêu Tốc)';
END
ELSE
BEGIN
    PRINT '  -> Staff already exists: staff.sieutoc@garage.com';
END

PRINT '';

-- =============================================
-- STEP 6: Add Sample Chat Data
-- =============================================
PRINT 'STEP 6: Adding sample chat data...';

DECLARE @CustomerId INT, @StaffId1 INT, @StaffId2 INT;
SELECT @CustomerId = Id FROM [User] WHERE Email = 'customer.test@example.com';
SELECT @StaffId1 = Id FROM GarageStaff WHERE Email = 'staff.thanglong@garage.com';
SELECT @StaffId2 = Id FROM GarageStaff WHERE Email = 'staff.sieutoc@garage.com';

-- Create ChatRoom between customer and Gara Thăng Long
DECLARE @RoomId1 BIGINT, @RoomId2 BIGINT;

IF NOT EXISTS (SELECT * FROM ChatRoom WHERE GarageId = @Garage1Id AND CustomerId = @CustomerId)
BEGIN
    INSERT INTO ChatRoom (GarageId, CustomerId, LastMessageAt, CreatedAt)
    VALUES (@Garage1Id, @CustomerId, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
    
    SET @RoomId1 = SCOPE_IDENTITY();
    PRINT '  -> Created chat room: Customer ↔ Gara Thăng Long';
    
    -- Add staff member to room
    INSERT INTO ChatRoomMember (RoomId, UserType, UserId, JoinedAt, CreatedAt)
    VALUES (@RoomId1, 1, @StaffId1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()); -- UserType 1 = STAFF
    
    -- Add sample messages
    INSERT INTO ChatMessage (RoomId, SenderType, SenderId, Message, MessageType, Status, IsRead, CreatedAt)
    VALUES 
        (@RoomId1, 0, @CustomerId, N'Xin chào, tôi muốn đặt lịch bảo dưỡng xe.', 0, 0, 1, DATEADD(MINUTE, -30, SYSDATETIMEOFFSET())),
        (@RoomId1, 1, @StaffId1, N'Chào anh, garage chúng tôi có thể hỗ trợ anh. Xe của anh là loại gì ạ?', 0, 0, 1, DATEADD(MINUTE, -25, SYSDATETIMEOFFSET())),
        (@RoomId1, 0, @CustomerId, N'Xe Toyota Vios 2020, cần thay dầu và kiểm tra phanh.', 0, 0, 1, DATEADD(MINUTE, -20, SYSDATETIMEOFFSET())),
        (@RoomId1, 1, @StaffId1, N'Dạ được ạ. Anh có thể đến vào lúc nào thuận tiện ạ?', 0, 0, 0, DATEADD(MINUTE, -15, SYSDATETIMEOFFSET()));
    
    PRINT '  -> Added 4 sample messages to chat room';
END
ELSE
BEGIN
    PRINT '  -> Chat room already exists: Customer ↔ Gara Thăng Long';
END

-- Create ChatRoom between customer and Gara Siêu Tốc
IF NOT EXISTS (SELECT * FROM ChatRoom WHERE GarageId = @Garage2Id AND CustomerId = @CustomerId)
BEGIN
    INSERT INTO ChatRoom (GarageId, CustomerId, LastMessageAt, CreatedAt)
    VALUES (@Garage2Id, @CustomerId, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
    
    SET @RoomId2 = SCOPE_IDENTITY();
    PRINT '  -> Created chat room: Customer ↔ Gara Siêu Tốc';
    
    -- Add staff member to room
    INSERT INTO ChatRoomMember (RoomId, UserType, UserId, JoinedAt, CreatedAt)
    VALUES (@RoomId2, 1, @StaffId2, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()); -- UserType 1 = STAFF
    
    -- Add sample messages
    INSERT INTO ChatMessage (RoomId, SenderType, SenderId, Message, MessageType, Status, IsRead, CreatedAt)
    VALUES 
        (@RoomId2, 0, @CustomerId, N'Cho em hỏi garage có dịch vụ sửa điều hòa không ạ?', 0, 0, 1, DATEADD(HOUR, -2, SYSDATETIMEOFFSET())),
        (@RoomId2, 1, @StaffId2, N'Dạ có ạ. Em có thể hỗ trợ anh chị. Xe đang gặp vấn đề gì ạ?', 0, 0, 0, DATEADD(HOUR, -1, SYSDATETIMEOFFSET()));
    
    PRINT '  -> Added 2 sample messages to chat room';
END
ELSE
BEGIN
    PRINT '  -> Chat room already exists: Customer ↔ Gara Siêu Tốc';
END

PRINT '';

-- =============================================
-- STEP 7: Display Summary
-- =============================================
PRINT '========================================';
PRINT 'Database Setup Complete!';
PRINT '========================================';
PRINT '';
PRINT 'Summary:';

DECLARE @GarageCount VARCHAR(10), @StaffCount VARCHAR(10), @RoomCount VARCHAR(10), @MsgCount VARCHAR(10);
SELECT @GarageCount = CAST(COUNT(*) AS VARCHAR(10)) FROM Garage;
SELECT @StaffCount = CAST(COUNT(*) AS VARCHAR(10)) FROM GarageStaff;
SELECT @RoomCount = CAST(COUNT(*) AS VARCHAR(10)) FROM ChatRoom;
SELECT @MsgCount = CAST(COUNT(*) AS VARCHAR(10)) FROM ChatMessage;

PRINT '  Garages: ' + @GarageCount;
PRINT '  Garage Staff: ' + @StaffCount;
PRINT '  Chat Rooms: ' + @RoomCount;
PRINT '  Chat Messages: ' + @MsgCount;
PRINT '';

-- Display created garages
PRINT 'Garages in database:';
SELECT 
    g.Id,
    g.Name,
    g.Email,
    g.PhoneNumber,
    u.FullName AS OwnerName,
    u.Email AS OwnerEmail
FROM Garage g
INNER JOIN [User] u ON g.UserId = u.Id;

PRINT '';

-- Display chat rooms
PRINT 'Chat rooms in database:';
SELECT 
    cr.Id AS RoomId,
    g.Name AS GarageName,
    u.FullName AS CustomerName,
    cr.LastMessageAt,
    (SELECT COUNT(*) FROM ChatMessage WHERE RoomId = cr.Id) AS MessageCount
FROM ChatRoom cr
INNER JOIN Garage g ON cr.GarageId = g.Id
INNER JOIN [User] u ON cr.CustomerId = u.Id;

PRINT '';
PRINT '========================================';
PRINT 'You can now test the chat API!';
PRINT '========================================';

GO
