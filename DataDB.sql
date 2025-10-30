-- ============================================================
-- SCRIPT THÊM USER (ĐÃ XÓA TRƯỜNG CreatedBy)
-- ============================================================

-- Chú ý: Bảng giả định là 'Users'. ID sẽ tự động tăng.
-- BaseModel fields used: CreatedAt
-- Enum values: 
-- Role: 0=ADMIN, 1=CUSTOMER, 2=GARAGE
-- UserStatus: 0=ACTIVE, 1=UNACTIVE, 2=BLOCK
-- IsActive: 1=True, 0=False

INSERT INTO [dbo].[User] 
    (FullName, Email, PhoneNumber, PasswordHash, UserRole, UserStatus, CreatedAt, IsActive)
VALUES
-- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- 15 TÀI KHOẢN HỆ THỐNG VÀ ĐỐI TÁC (ADMIN/GARAGE)
-- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- 3 ADMIN (Role=0)
(N'Admin Tổng Quản', N'admin.master@system.com', N'0901111111', N'hashed_admin_1_password', 0, 0, '2024-01-15 09:00:00 +07:00', 1), -- Active
(N'Admin Phân Tích', N'admin.analyst@system.com', N'0902222222', N'hashed_admin_2_password', 0, 0, '2024-01-20 10:30:00 +07:00', 1), -- Active
(N'Admin Đã Xóa', N'admin.deleted@system.com', N'0903333333', N'hashed_admin_3_password', 0, 0, '2023-10-01 11:45:00 +07:00', 0), -- IsActive=0
-- 12 GARAGE (Role=2)
(N'Gara Thăng Long', N'gara.thanglong@partner.com', N'0911234567', N'hashed_gara_1_password', 2, 0, '2024-03-01 14:15:00 +07:00', 1),
(N'Dịch Vụ Lốp Xe Siêu Tốc', N'tires.fast@partner.com', N'0912345678', N'hashed_gara_2_password', 2, 0, '2024-03-10 15:30:00 +07:00', 1),
(N'Phụ Tùng Chính Hãng A', N'spareparts.a@partner.com', N'0913456789', N'hashed_gara_3_password', 2, 0, '2024-04-05 16:45:00 +07:00', 1),
(N'Gara Hoàn Mỹ', N'gara.hoanmy@partner.com', N'0914567890', N'hashed_gara_4_password', 2, 0, '2024-04-20 17:00:00 +07:00', 1),
(N'Đại Lý Xe Cũ Minh', N'usedcar.minh@partner.com', N'0915678901', N'hashed_gara_5_password', 2, 0, '2024-05-01 18:15:00 +07:00', 1),
(N'Trung Tâm Bảo Trì B', N'maintenance.b@partner.com', N'0916789012', N'hashed_gara_6_password', 2, 0, '2024-05-15 19:30:00 +07:00', 1),
(N'Gara Bị Khóa 1', N'gara.locked.1@partner.com', N'0917890123', N'hashed_gara_7_password', 2, 2, '2024-06-01 10:00:00 +07:00', 1), -- BLOCK
(N'Gara Chưa Kích Hoạt 1', N'gara.unactive.1@partner.com', N'0918901234', N'hashed_gara_8_password', 2, 1, '2024-06-05 11:00:00 +07:00', 1), -- UNACTIVE
(N'Gara Bị Khóa 2', N'gara.locked.2@partner.com', N'0919012345', N'hashed_gara_9_password', 2, 2, '2024-07-01 12:00:00 +07:00', 1), -- BLOCK
(N'Gara Thử Nghiệm 1', N'gara.test.1@partner.com', N'0920123456', N'hashed_gara_10_password', 2, 0, '2024-07-15 13:00:00 +07:00', 1),
(N'Gara Đã Xóa', N'gara.deleted.2@partner.com', N'0921234567', N'hashed_gara_11_password', 2, 1, '2023-12-01 14:00:00 +07:00', 0), -- UNACTIVE & IsActive=0
(N'Gara Hoạt Động Cũ', N'gara.old.active@partner.com', N'0922345678', N'hashed_gara_12_password', 2, 0, '2024-02-01 15:00:00 +07:00', 1),
-- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- 35 TÀI KHOẢN CUSTOMER (Role=1)
-- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- 28 ACTIVE Customers
(N'Nguyễn Văn An', N'nguyen.van.an@example.com', N'0931111111', N'hashed_customer_1_password', 1, 0, '2024-06-15 16:00:00 +07:00', 1),
(N'Trần Thị Bích', N'tran.thi.bich@example.com', N'0932222222', N'hashed_customer_2_password', 1, 0, '2024-06-20 17:00:00 +07:00', 1),
(N'Lê Đình Chung', N'le.dinh.chung@example.com', N'0933333333', N'hashed_customer_3_password', 1, 0, '2024-06-25 18:00:00 +07:00', 1),
(N'Phạm Thu Dung', N'pham.thu.dung@example.com', N'0934444444', N'hashed_customer_4_password', 1, 0, '2024-07-01 19:00:00 +07:00', 1),
(N'Hoàng Văn Giang', N'hoang.van.giang@example.com', N'0935555555', N'hashed_customer_5_password', 1, 0, '2024-07-05 09:00:00 +07:00', 1),
(N'Vũ Thị Hiền', N'vu.thi.hien@example.com', N'0936666666', N'hashed_customer_6_password', 1, 0, '2024-07-10 10:00:00 +07:00', 1),
(N'Bùi Minh Khôi', N'bui.minh.khoi@example.com', N'0937777777', N'hashed_customer_7_password', 1, 0, '2024-07-15 11:00:00 +07:00', 1),
(N'Đỗ Ngọc Lan', N'do.ngoc.lan@example.com', N'0938888888', N'hashed_customer_8_password', 1, 0, '2024-07-20 12:00:00 +07:00', 1),
(N'Tô Văn Mạnh', N'to.van.manh@example.com', N'0939999999', N'hashed_customer_9_password', 1, 0, '2024-07-25 13:00:00 +07:00', 1),
(N'Đặng Thị Nga', N'dang.thi.nga@example.com', N'0810000000', N'hashed_customer_10_password', 1, 0, '2024-08-01 14:00:00 +07:00', 1),
(N'Lý Hữu Phúc', N'ly.huu.phuc@example.com', N'0811111111', N'hashed_customer_11_password', 1, 0, '2024-08-05 15:00:00 +07:00', 1),
(N'Mai Thị Quỳnh', N'mai.thi.quynh@example.com', N'0812222222', N'hashed_customer_12_password', 1, 0, '2024-08-10 16:00:00 +07:00', 1),
(N'Trần Quang Sơn', N'tran.quang.son@example.com', N'0813333333', N'hashed_customer_13_password', 1, 0, '2024-08-15 17:00:00 +07:00', 1),
(N'Ngô Thu Thủy', N'ngo.thu.thuy@example.com', N'0814444444', N'hashed_customer_14_password', 1, 0, '2024-08-20 18:00:00 +07:00', 1),
(N'Võ Văn Út', N'vo.van.ut@example.com', N'0815555555', N'hashed_customer_15_password', 1, 0, '2024-08-25 19:00:00 +07:00', 1),
(N'Phan Thị Vân', N'phan.thi.van@example.com', N'0816666666', N'hashed_customer_16_password', 1, 0, '2024-09-01 09:00:00 +07:00', 1),
(N'Tạ Chí Vinh', N'ta.chi.vinh@example.com', N'0817777777', N'hashed_customer_17_password', 1, 0, '2024-09-05 10:00:00 +07:00', 1),
(N'Hồ Thị Xuân', N'ho.thi.xuan@example.com', N'0818888888', N'hashed_customer_18_password', 1, 0, '2024-09-10 11:00:00 +07:00', 1),
(N'Cao Văn Yên', N'cao.van.yen@example.com', N'0819999999', N'hashed_customer_19_password', 1, 0, '2024-09-15 12:00:00 +07:00', 1),
(N'Kiều Thị Ánh', N'kieu.thi.anh@example.com', N'0321111111', N'hashed_customer_20_password', 1, 0, '2024-09-20 13:00:00 +07:00', 1),
(N'Dương Văn Bảo', N'duong.van.bao@example.com', N'0322222222', N'hashed_customer_21_password', 1, 0, '2024-09-25 14:00:00 +07:00', 1),
(N'Lưu Thị Cẩm', N'luu.thi.cam@example.com', N'0323333333', N'hashed_customer_22_password', 1, 0, '2024-10-01 15:00:00 +07:00', 1),
(N'Mạc Văn Dũng', N'mac.van.dung@example.com', N'0324444444', N'hashed_customer_23_password', 1, 0, '2024-10-05 16:00:00 +07:00', 1),
(N'Nguyễn Phương Hà', N'nguyen.phuong.ha@example.com', N'0325555555', N'hashed_customer_24_password', 1, 0, '2024-10-10 17:00:00 +07:00', 1),
(N'Tống Thị Hương', N'tong.thi.huong@example.com', N'0326666666', N'hashed_customer_25_password', 1, 0, '2024-10-16 09:00:00 +07:00', 1),
(N'Trịnh Văn Khoa', N'trinh.van.khoa@example.com', N'0327777777', N'hashed_customer_26_password', 1, 0, '2024-10-17 10:00:00 +07:00', 1),
(N'Út Thị Liên', N'ut.thi.lien@example.com', N'0328888888', N'hashed_customer_27_password', 1, 0, '2024-10-17 11:00:00 +07:00', 1),
(N'Chung Văn Minh', N'chung.van.minh@example.com', N'0329999999', N'hashed_customer_28_password', 1, 0, '2024-10-18 12:00:00 +07:00', 1),
-- 5 UNACTIVE Customers (Role=1, UserStatus=1)
(N'User Chưa Kích Hoạt 1', N'unactive.user.1@example.com', N'0701111111', N'hashed_unactive_1_password', 1, 1, '2024-05-20 13:00:00 +07:00', 1),
(N'User Chưa Kích Hoạt 2', N'unactive.user.2@example.com', N'0702222222', N'hashed_unactive_2_password', 1, 1, '2024-06-01 14:00:00 +07:00', 1),
(N'User Chưa Kích Hoạt 3', N'unactive.user.3@example.com', N'0703333333', N'hashed_unactive_3_password', 1, 1, '2024-07-01 15:00:00 +07:00', 1),
(N'User Chưa Kích Hoạt 4', N'unactive.user.4@example.com', N'0704444444', N'hashed_unactive_4_password', 1, 1, '2024-08-01 16:00:00 +07:00', 1),
(N'User Đã Xóa 1', N'deleted.user.1@example.com', N'0705555555', N'hashed_deleted_1_password', 1, 0, '2023-01-01 17:00:00 +07:00', 0), -- IsActive=0
-- 2 BLOCK Customers (Role=1, UserStatus=2)
(N'User Bị Khóa Lâu', N'block.user.1@example.com', N'0706666666', N'hashed_block_1_password', 1, 2, '2024-01-01 18:00:00 +07:00', 1),
(N'User Bị Khóa Mới', N'block.user.2@example.com', N'0707777777', N'hashed_block_2_password', 1, 2, '2024-10-10 19:00:00 +07:00', 1);
-- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

GO

-- ============================================================
-- SCRIPT THÊM VEHICLE (ĐÃ XÓA TRƯỜNG CreatedBy)
-- ============================================================

-- Chú ý: Bảng giả định là 'Vehicles'. ID sẽ tự động tăng.
-- BaseModel fields used: CreatedAt
-- Enum values: 
-- FuelType: 0=GASOLINE, 1=DIESEL, 2=ELECTRIC, 3=HYBRID
-- TransmissionType: 0=AUTOMATIC, 1=MANUAL, 2=CTV, 3=DCT

-- Đảm bảo đã chạy script INSERT 50 User trước!

-- SCRIPT VEHICLES ĐÃ SỬA: UserId được điều chỉnh để bắt đầu từ 31

INSERT INTO [dbo].[Vehicle] 
    (LicensePlate, FuelType, TransmissionType, Brand, Model, Year, IsActive, UserId, CreatedAt)
VALUES
-- ------------------------------------------------------------------------------------------------------------------
-- Gốc UserId 1-4 (Đã sửa thành 31-34)
-- ------------------------------------------------------------------------------------------------------------------
(N'51A-00101', 0, 0, N'Toyota', N'Camry', 2022, 1, 31, '2024-01-15 09:10:00 +07:00'), -- Gốc: 1
(N'60C-67890', 3, 0, N'Lexus', N'NX350h', 2023, 1, 31, '2024-01-15 09:15:00 +07:00'), -- Gốc: 1
(N'29Z-00001', 0, 1, N'Honda', N'Wave Alpha', 2018, 1, 32, '2024-01-20 10:40:00 +07:00'), -- Gốc: 2
(N'29B-99999', 1, 0, N'Ford', N'Transit', 2021, 1, 34, '2024-03-05 14:00:00 +07:00'), -- Gốc: 4
(N'30K-12345', 2, 0, N'Vinfast', N'VF e34', 2023, 1, 34, '2024-03-05 14:01:00 +07:00'), -- Gốc: 4

-- ------------------------------------------------------------------------------------------------------------------
-- Gốc UserId 16-40 (Đã sửa thành 46-50) & Bắt đầu từ 31 trở đi
-- ------------------------------------------------------------------------------------------------------------------
-- Khách hàng (Sử dụng 50 User cuối trong bảng)
(N'30E-11122', 0, 0, N'Mazda', N'3', 2020, 1, 46, '2024-06-15 16:30:00 +07:00'), -- Gốc: 16 -> 46 (User Chưa Kích Hoạt 2)
(N'34F-33445', 0, 0, N'Hyundai', N'Accent', 2021, 1, 47, '2024-06-20 17:30:00 +07:00'),
(N'51G-55667', 0, 0, N'Kia', N'K3', 2022, 1, 48, '2024-06-25 18:30:00 +07:00'),
(N'70H-77889', 0, 0, N'Vinfast', N'Fadil', 2020, 1, 49, '2024-07-01 19:30:00 +07:00'),
(N'43I-99001', 0, 0, N'Mitsubishi', N'Xpander', 2023, 1, 50, '2024-07-05 09:30:00 +07:00'),

-- Tiếp tục điều chỉnh các ID còn lại về phạm vi 31-50 hoặc ID có sẵn.
-- Lưu ý: Từ đây, tôi sẽ dùng ID 31-50 vì chỉ có chúng tồn tại.
(N'29L-12312', 1, 0, N'Toyota', N'Fortuner', 2019, 1, 32, '2024-07-10 10:30:00 +07:00'),
(N'34M-34534', 1, 0, N'Ford', N'Everest', 2020, 1, 33, '2024-07-15 11:30:00 +07:00'),
(N'51N-56756', 1, 0, N'Hyundai', N'SantaFe', 2021, 1, 34, '2024-07-20 12:30:00 +07:00'),
(N'70P-78978', 0, 1, N'Kia', N'Morning', 2018, 1, 35, '2024-07-25 13:30:00 +07:00'),
(N'43Q-90190', 0, 1, N'Hyundai', N'i10', 2017, 1, 36, '2024-08-01 14:30:00 +07:00'),
(N'51R-11234', 0, 1, N'Chevrolet', N'Spark', 2016, 1, 37, '2024-08-05 15:30:00 +07:00'),
(N'29S-56789', 0, 1, N'Mazda', N'2', 2019, 1, 38, '2024-08-10 16:30:00 +07:00'),
(N'30T-00005', 2, 3, N'Vinfast', N'VF8', 2024, 1, 39, '2024-08-15 17:30:00 +07:00'), 
(N'34U-10010', 2, 3, N'Porsche', N'Taycan', 2023, 1, 40, '2024-08-20 18:30:00 +07:00'), 
(N'51V-20020', 3, 2, N'Toyota', N'Corolla Cross', 2022, 1, 41, '2024-08-25 19:30:00 +07:00'), 
(N'70W-30030', 3, 2, N'Honda', N'CR-V Hybrid', 2024, 1, 42, '2024-09-01 09:30:00 +07:00'), 
(N'43X-40040', 1, 0, N'Hyundai', N'Tucson', 2016, 1, 43, '2024-09-05 10:30:00 +07:00'),
(N'51Y-50050', 0, 2, N'Toyota', N'Vios', 2017, 1, 44, '2024-09-10 11:30:00 +07:00'), 
(N'29Z-60060', 0, 2, N'Honda', N'Civic', 2018, 1, 45, '2024-09-15 12:30:00 +07:00'), 
(N'30A-70070', 0, 0, N'Mercedes', N'C200', 2019, 0, 46, '2024-09-20 13:30:00 +07:00'), 
(N'34B-80080', 1, 0, N'BMW', N'X5', 2020, 0, 47, '2024-09-25 14:30:00 +07:00'), 
(N'51C-90090', 0, 3, N'Audi', N'A4', 2020, 1, 48, '2024-10-01 15:30:00 +07:00'), 
(N'70D-10101', 0, 0, N'Volvo', N'XC60', 2023, 1, 49, '2024-10-05 16:30:00 +07:00'), 
(N'43E-20202', 1, 0, N'Jeep', N'Wrangler', 2022, 1, 50, '2024-10-10 17:30:00 +07:00'),
(N'51F-30303', 0, 0, N'Subaru', N'Forester', 2021, 1, 31, '2024-10-16 09:30:00 +07:00'); -- Dùng lại ID 31

GO