-- ============================================================
-- BƯỚC 1: TẠO USER VÀ VEHICLE TRƯỚC
-- Database: CarLinker
-- ============================================================

USE [CarLinker];
GO

-- Xóa dữ liệu cũ nếu có
DELETE FROM [Vehicle];
DELETE FROM [User];
GO

-- Reset Identity
DBCC CHECKIDENT ('[User]', RESEED, 0);
DBCC CHECKIDENT ('[Vehicle]', RESEED, 0);
GO

PRINT N'🔄 Đang tạo Users...';

-- ============================================================
-- TẠO 30 USERS
-- ============================================================
-- Role: 0=CUSTOMER, 1=OWNER, 2=DEALER, 3=WAREHOUSE, 4=STAFF, 5=MANAGER
-- UserStatus: 0=ACTIVE, 1=UNACTIVE, 2=BLOCK

INSERT INTO [dbo].[User] 
    (FullName, Email, PhoneNumber, PasswordHash, UserRole, UserStatus, CreatedAt, IsActive, Image, RefreshToken, RefreshTokenExpiryTime)
VALUES
-- 3 Admin/Manager/Staff
(N'Admin Hệ Thống', N'admin@carlinker.com', N'0901000001', N'$2a$11$hashed_password_1', 5, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Quản Lý Kho', N'manager@carlinker.com', N'0901000002', N'$2a$11$hashed_password_2', 5, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Nhân Viên Hỗ Trợ', N'staff@carlinker.com', N'0901000003', N'$2a$11$hashed_password_3', 4, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),

-- 5 Garage Owners (Role = 1)
(N'Chủ Gara Thăng Long', N'owner.thanglong@partner.com', N'0911234567', N'$2a$11$hashed_password_4', 1, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Chủ Gara Siêu Tốc', N'owner.sieutoc@partner.com', N'0912345678', N'$2a$11$hashed_password_5', 1, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Chủ Phụ Tùng A', N'owner.phutung@partner.com', N'0913456789', N'$2a$11$hashed_password_6', 1, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Chủ Gara Hoàn Mỹ', N'owner.hoanmy@partner.com', N'0914567890', N'$2a$11$hashed_password_7', 1, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Chủ Đại Lý Xe Minh', N'owner.minh@partner.com', N'0915678901', N'$2a$11$hashed_password_8', 1, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),

-- 2 Dealers (Role = 2)
(N'Đại Lý Phụ Tùng Chính Hãng', N'dealer1@carlinker.com', N'0920000001', N'$2a$11$hashed_password_9', 2, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Đại Lý Lốp Xe Toàn Quốc', N'dealer2@carlinker.com', N'0920000002', N'$2a$11$hashed_password_10', 2, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),

-- 20 Customers (Role = 0)
(N'Nguyễn Văn An', N'nguyen.van.an@example.com', N'0931111111', N'$2a$11$hashed_password_11', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Trần Thị Bích', N'tran.thi.bich@example.com', N'0932222222', N'$2a$11$hashed_password_12', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Lê Đình Chung', N'le.dinh.chung@example.com', N'0933333333', N'$2a$11$hashed_password_13', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Phạm Thu Dung', N'pham.thu.dung@example.com', N'0934444444', N'$2a$11$hashed_password_14', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Hoàng Văn Giang', N'hoang.van.giang@example.com', N'0935555555', N'$2a$11$hashed_password_15', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Vũ Thị Hiền', N'vu.thi.hien@example.com', N'0936666666', N'$2a$11$hashed_password_16', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Bùi Minh Khôi', N'bui.minh.khoi@example.com', N'0937777777', N'$2a$11$hashed_password_17', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Đỗ Ngọc Lan', N'do.ngoc.lan@example.com', N'0938888888', N'$2a$11$hashed_password_18', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Tô Văn Mạnh', N'to.van.manh@example.com', N'0939999999', N'$2a$11$hashed_password_19', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Đặng Thị Nga', N'dang.thi.nga@example.com', N'0810000000', N'$2a$11$hashed_password_20', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Lý Hữu Phúc', N'ly.huu.phuc@example.com', N'0811111111', N'$2a$11$hashed_password_21', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Mai Thị Quỳnh', N'mai.thi.quynh@example.com', N'0812222222', N'$2a$11$hashed_password_22', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Trần Quang Sơn', N'tran.quang.son@example.com', N'0813333333', N'$2a$11$hashed_password_23', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Ngô Thu Thủy', N'ngo.thu.thuy@example.com', N'0814444444', N'$2a$11$hashed_password_24', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Võ Văn Út', N'vo.van.ut@example.com', N'0815555555', N'$2a$11$hashed_password_25', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Phan Thị Vân', N'phan.thi.van@example.com', N'0816666666', N'$2a$11$hashed_password_26', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Tạ Chí Vinh', N'ta.chi.vinh@example.com', N'0817777777', N'$2a$11$hashed_password_27', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Hồ Thị Xuân', N'ho.thi.xuan@example.com', N'0818888888', N'$2a$11$hashed_password_28', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Cao Văn Yên', N'cao.van.yen@example.com', N'0819999999', N'$2a$11$hashed_password_29', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL),
(N'Kiều Thị Ánh', N'kieu.thi.anh@example.com', N'0321111111', N'$2a$11$hashed_password_30', 0, 0, SYSDATETIMEOFFSET(), 1, NULL, NULL, NULL);
GO

DECLARE @UserCount INT = (SELECT COUNT(*) FROM [User]);
PRINT N'✅ Đã tạo ' + CAST(@UserCount AS NVARCHAR) + N' Users';
GO

PRINT N'🔄 Đang tạo Vehicles...';

-- ============================================================
-- TẠO 15 VEHICLES CHO CUSTOMERS (UserId 11-25)
-- ============================================================
-- FuelType: 0=GASOLINE, 1=DIESEL, 2=ELECTRIC, 3=HYBRID
-- TransmissionType: 0=AUTOMATIC, 1=MANUAL, 2=CTV, 3=DCT

INSERT INTO [dbo].[Vehicle] 
    (LicensePlate, FuelType, TransmissionType, Brand, Model, Year, IsActive, Image, UserId, CreatedAt, UpdatedAt)
VALUES
(N'51A-12345', 0, 0, N'Toyota', N'Camry', 2022, 1, '', 11, SYSDATETIMEOFFSET(), NULL),
(N'60C-67890', 3, 0, N'Lexus', N'NX350h', 2023, 1, '', 11, SYSDATETIMEOFFSET(), NULL),
(N'29Z-00001', 0, 1, N'Honda', N'Wave Alpha', 2018, 1, '', 12, SYSDATETIMEOFFSET(), NULL),
(N'29B-99999', 1, 0, N'Ford', N'Transit', 2021, 1, '', 13, SYSDATETIMEOFFSET(), NULL),
(N'30K-12345', 2, 0, N'Vinfast', N'VF e34', 2023, 1, '', 13, SYSDATETIMEOFFSET(), NULL),
(N'30E-11122', 0, 0, N'Mazda', N'3', 2020, 1, '', 14, SYSDATETIMEOFFSET(), NULL),
(N'34F-33445', 0, 0, N'Hyundai', N'Accent', 2021, 1, '', 15, SYSDATETIMEOFFSET(), NULL),
(N'51G-55667', 0, 0, N'Kia', N'K3', 2022, 1, '', 16, SYSDATETIMEOFFSET(), NULL),
(N'70H-77889', 0, 0, N'Vinfast', N'Fadil', 2020, 1, '', 17, SYSDATETIMEOFFSET(), NULL),
(N'43I-99001', 0, 0, N'Mitsubishi', N'Xpander', 2023, 1, '', 18, SYSDATETIMEOFFSET(), NULL),
(N'29L-12312', 1, 0, N'Toyota', N'Fortuner', 2019, 1, '', 19, SYSDATETIMEOFFSET(), NULL),
(N'34M-34534', 1, 0, N'Ford', N'Everest', 2020, 1, '', 20, SYSDATETIMEOFFSET(), NULL),
(N'51N-56756', 1, 0, N'Hyundai', N'SantaFe', 2021, 1, '', 21, SYSDATETIMEOFFSET(), NULL),
(N'70P-78978', 0, 1, N'Kia', N'Morning', 2018, 1, '', 22, SYSDATETIMEOFFSET(), NULL),
(N'43Q-90190', 0, 1, N'Hyundai', N'i10', 2017, 1, '', 23, SYSDATETIMEOFFSET(), NULL);
GO

DECLARE @VehicleCount INT = (SELECT COUNT(*) FROM [Vehicle]);
PRINT N'✅ Đã tạo ' + CAST(@VehicleCount AS NVARCHAR) + N' Vehicles';
GO

PRINT N'';
PRINT N'════════════════════════════════════════════';
PRINT N'✅ HOÀN TẤT TẠO USER VÀ VEHICLE!';
PRINT N'════════════════════════════════════════════';
PRINT N'Bây giờ có thể chạy script tạo dữ liệu còn lại';
GO