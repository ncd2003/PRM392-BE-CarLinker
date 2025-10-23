-- Chú ý: Bảng giả định là 'Users'. ID sẽ tự động tăng.
-- BaseModel fields used: CreatedBy, CreatedAt
-- Enum values: 
-- Role: 0=ADMIN, 1=CUSTOMER, 2=GARAGE
-- UserStatus: 0=ACTIVE, 1=UNACTIVE, 2=BLOCK
-- IsActive: 1=True, 0=False

INSERT INTO [dbo].[User] 
    (FullName, Email, PhoneNumber, PasswordHash, UserRole, UserStatus, CreatedBy, CreatedAt, IsActive)
--                                                                                             ^^^^^^^^
VALUES
-- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- 15 TÀI KHOẢN HỆ THỐNG VÀ ĐỐI TÁC (ADMIN/GARAGE)
-- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- 3 ADMIN (Role=0)
('Admin Tổng Quản', 'admin.master@system.com', '0901111111', 'hashed_admin_1_password', 0, 0, 'System', '2024-01-15 09:00:00 +07:00', 1), -- Active
('Admin Phân Tích', 'admin.analyst@system.com', '0902222222', 'hashed_admin_2_password', 0, 0, 'System', '2024-01-20 10:30:00 +07:00', 1), -- Active
('Admin Đã Xóa', 'admin.deleted@system.com', '0903333333', 'hashed_admin_3_password', 0, 0, 'System', '2023-10-01 11:45:00 +07:00', 0), -- IsActive=0
-- 12 GARAGE (Role=2)
('Gara Thăng Long', 'gara.thanglong@partner.com', '0911234567', 'hashed_gara_1_password', 2, 0, 'Admin-A', '2024-03-01 14:15:00 +07:00', 1),
('Dịch Vụ Lốp Xe Siêu Tốc', 'tires.fast@partner.com', '0912345678', 'hashed_gara_2_password', 2, 0, 'Admin-A', '2024-03-10 15:30:00 +07:00', 1),
('Phụ Tùng Chính Hãng A', 'spareparts.a@partner.com', '0913456789', 'hashed_gara_3_password', 2, 0, 'Admin-B', '2024-04-05 16:45:00 +07:00', 1),
('Gara Hoàn Mỹ', 'gara.hoanmy@partner.com', '0914567890', 'hashed_gara_4_password', 2, 0, 'Admin-B', '2024-04-20 17:00:00 +07:00', 1),
('Đại Lý Xe Cũ Minh', 'usedcar.minh@partner.com', '0915678901', 'hashed_gara_5_password', 2, 0, 'Admin-C', '2024-05-01 18:15:00 +07:00', 1),
('Trung Tâm Bảo Trì B', 'maintenance.b@partner.com', '0916789012', 'hashed_gara_6_password', 2, 0, 'Admin-C', '2024-05-15 19:30:00 +07:00', 1),
('Gara Bị Khóa 1', 'gara.locked.1@partner.com', '0917890123', 'hashed_gara_7_password', 2, 2, 'Admin-A', '2024-06-01 10:00:00 +07:00', 1), -- BLOCK
('Gara Chưa Kích Hoạt 1', 'gara.unactive.1@partner.com', '0918901234', 'hashed_gara_8_password', 2, 1, 'Admin-A', '2024-06-05 11:00:00 +07:00', 1), -- UNACTIVE
('Gara Bị Khóa 2', 'gara.locked.2@partner.com', '0919012345', 'hashed_gara_9_password', 2, 2, 'Admin-D', '2024-07-01 12:00:00 +07:00', 1), -- BLOCK
('Gara Thử Nghiệm 1', 'gara.test.1@partner.com', '0920123456', 'hashed_gara_10_password', 2, 0, 'Admin-D', '2024-07-15 13:00:00 +07:00', 1),
('Gara Đã Xóa', 'gara.deleted.2@partner.com', '0921234567', 'hashed_gara_11_password', 2, 1, 'Admin-E', '2023-12-01 14:00:00 +07:00', 0), -- UNACTIVE & IsActive=0
('Gara Hoạt Động Cũ', 'gara.old.active@partner.com', '0922345678', 'hashed_gara_12_password', 2, 0, 'Admin-E', '2024-02-01 15:00:00 +07:00', 1),
-- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- 35 TÀI KHOẢN CUSTOMER (Role=1)
-- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- 28 ACTIVE Customers
('Nguyễn Văn An', 'nguyen.van.an@example.com', '0931111111', 'hashed_customer_1_password', 1, 0, 'InitialScript', '2024-06-15 16:00:00 +07:00', 1),
('Trần Thị Bích', 'tran.thi.bich@example.com', '0932222222', 'hashed_customer_2_password', 1, 0, 'InitialScript', '2024-06-20 17:00:00 +07:00', 1),
('Lê Đình Chung', 'le.dinh.chung@example.com', '0933333333', 'hashed_customer_3_password', 1, 0, 'InitialScript', '2024-06-25 18:00:00 +07:00', 1),
('Phạm Thu Dung', 'pham.thu.dung@example.com', '0934444444', 'hashed_customer_4_password', 1, 0, 'InitialScript', '2024-07-01 19:00:00 +07:00', 1),
('Hoàng Văn Giang', 'hoang.van.giang@example.com', '0935555555', 'hashed_customer_5_password', 1, 0, 'InitialScript', '2024-07-05 09:00:00 +07:00', 1),
('Vũ Thị Hiền', 'vu.thi.hien@example.com', '0936666666', 'hashed_customer_6_password', 1, 0, 'InitialScript', '2024-07-10 10:00:00 +07:00', 1),
('Bùi Minh Khôi', 'bui.minh.khoi@example.com', '0937777777', 'hashed_customer_7_password', 1, 0, 'InitialScript', '2024-07-15 11:00:00 +07:00', 1),
('Đỗ Ngọc Lan', 'do.ngoc.lan@example.com', '0938888888', 'hashed_customer_8_password', 1, 0, 'InitialScript', '2024-07-20 12:00:00 +07:00', 1),
('Tô Văn Mạnh', 'to.van.manh@example.com', '0939999999', 'hashed_customer_9_password', 1, 0, 'InitialScript', '2024-07-25 13:00:00 +07:00', 1),
('Đặng Thị Nga', 'dang.thi.nga@example.com', '0810000000', 'hashed_customer_10_password', 1, 0, 'InitialScript', '2024-08-01 14:00:00 +07:00', 1),
('Lý Hữu Phúc', 'ly.huu.phuc@example.com', '0811111111', 'hashed_customer_11_password', 1, 0, 'InitialScript', '2024-08-05 15:00:00 +07:00', 1),
('Mai Thị Quỳnh', 'mai.thi.quynh@example.com', '0812222222', 'hashed_customer_12_password', 1, 0, 'InitialScript', '2024-08-10 16:00:00 +07:00', 1),
('Trần Quang Sơn', 'tran.quang.son@example.com', '0813333333', 'hashed_customer_13_password', 1, 0, 'InitialScript', '2024-08-15 17:00:00 +07:00', 1),
('Ngô Thu Thủy', 'ngo.thu.thuy@example.com', '0814444444', 'hashed_customer_14_password', 1, 0, 'InitialScript', '2024-08-20 18:00:00 +07:00', 1),
('Võ Văn Út', 'vo.van.ut@example.com', '0815555555', 'hashed_customer_15_password', 1, 0, 'InitialScript', '2024-08-25 19:00:00 +07:00', 1),
('Phan Thị Vân', 'phan.thi.van@example.com', '0816666666', 'hashed_customer_16_password', 1, 0, 'InitialScript', '2024-09-01 09:00:00 +07:00', 1),
('Tạ Chí Vinh', 'ta.chi.vinh@example.com', '0817777777', 'hashed_customer_17_password', 1, 0, 'InitialScript', '2024-09-05 10:00:00 +07:00', 1),
('Hồ Thị Xuân', 'ho.thi.xuan@example.com', '0818888888', 'hashed_customer_18_password', 1, 0, 'InitialScript', '2024-09-10 11:00:00 +07:00', 1),
('Cao Văn Yên', 'cao.van.yen@example.com', '0819999999', 'hashed_customer_19_password', 1, 0, 'InitialScript', '2024-09-15 12:00:00 +07:00', 1),
('Kiều Thị Ánh', 'kieu.thi.anh@example.com', '0321111111', 'hashed_customer_20_password', 1, 0, 'InitialScript', '2024-09-20 13:00:00 +07:00', 1),
('Dương Văn Bảo', 'duong.van.bao@example.com', '0322222222', 'hashed_customer_21_password', 1, 0, 'InitialScript', '2024-09-25 14:00:00 +07:00', 1),
('Lưu Thị Cẩm', 'luu.thi.cam@example.com', '0323333333', 'hashed_customer_22_password', 1, 0, 'InitialScript', '2024-10-01 15:00:00 +07:00', 1),
('Mạc Văn Dũng', 'mac.van.dung@example.com', '0324444444', 'hashed_customer_23_password', 1, 0, 'InitialScript', '2024-10-05 16:00:00 +07:00', 1),
('Nguyễn Phương Hà', 'nguyen.phuong.ha@example.com', '0325555555', 'hashed_customer_24_password', 1, 0, 'InitialScript', '2024-10-10 17:00:00 +07:00', 1),
('Tống Thị Hương', 'tong.thi.huong@example.com', '0326666666', 'hashed_customer_25_password', 1, 0, 'InitialScript', '2024-10-16 09:00:00 +07:00', 1),
('Trịnh Văn Khoa', 'trinh.van.khoa@example.com', '0327777777', 'hashed_customer_26_password', 1, 0, 'InitialScript', '2024-10-17 10:00:00 +07:00', 1),
('Út Thị Liên', 'ut.thi.lien@example.com', '0328888888', 'hashed_customer_27_password', 1, 0, 'InitialScript', '2024-10-17 11:00:00 +07:00', 1),
('Chung Văn Minh', 'chung.van.minh@example.com', '0329999999', 'hashed_customer_28_password', 1, 0, 'InitialScript', '2024-10-18 12:00:00 +07:00', 1),
-- 5 UNACTIVE Customers (Role=1, UserStatus=1)
('User Chưa Kích Hoạt 1', 'unactive.user.1@example.com', '0701111111', 'hashed_unactive_1_password', 1, 1, 'InitialScript', '2024-05-20 13:00:00 +07:00', 1),
('User Chưa Kích Hoạt 2', 'unactive.user.2@example.com', '0702222222', 'hashed_unactive_2_password', 1, 1, 'InitialScript', '2024-06-01 14:00:00 +07:00', 1),
('User Chưa Kích Hoạt 3', 'unactive.user.3@example.com', '0703333333', 'hashed_unactive_3_password', 1, 1, 'InitialScript', '2024-07-01 15:00:00 +07:00', 1),
('User Chưa Kích Hoạt 4', 'unactive.user.4@example.com', '0704444444', 'hashed_unactive_4_password', 1, 1, 'InitialScript', '2024-08-01 16:00:00 +07:00', 1),
('User Đã Xóa 1', 'deleted.user.1@example.com', '0705555555', 'hashed_deleted_1_password', 1, 0, 'Admin-G', '2023-01-01 17:00:00 +07:00', 0), -- IsActive=0
-- 2 BLOCK Customers (Role=1, UserStatus=2)
('User Bị Khóa Lâu', 'block.user.1@example.com', '0706666666', 'hashed_block_1_password', 1, 2, 'Admin-F', '2024-01-01 18:00:00 +07:00', 1),
('User Bị Khóa Mới', 'block.user.2@example.com', '0707777777', 'hashed_block_2_password', 1, 2, 'Admin-F', '2024-10-10 19:00:00 +07:00', 1);
-- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

GO


-- Chú ý: Bảng giả định là 'Vehicles'. ID sẽ tự động tăng.
-- BaseModel fields used: CreatedBy, CreatedAt
-- Enum values: 
-- FuelType: 0=GASOLINE, 1=DIESEL, 2=ELECTRIC, 3=HYBRID
-- TransmissionType: 0=AUTOMATIC, 1=MANUAL, 2=CTV, 3=DCT

-- Đảm bảo đã chạy script INSERT 50 User trước!

-- SCRIPT VEHICLES ĐÃ SỬA: UserId được điều chỉnh để bắt đầu từ 31

INSERT INTO [dbo].[Vehicle] 
    (LicensePlate, FuelType, TransmissionType, Brand, Model, Year, IsActive, UserId, CreatedBy, CreatedAt)
VALUES
-- ------------------------------------------------------------------------------------------------------------------
-- Gốc UserId 1-4 (Đã sửa thành 31-34)
-- ------------------------------------------------------------------------------------------------------------------
('51A-00101', 0, 0, 'Toyota', 'Camry', 2022, 1, 31, 'Admin-A', '2024-01-15 09:10:00 +07:00'), -- Gốc: 1
('60C-67890', 3, 0, 'Lexus', 'NX350h', 2023, 1, 31, 'Admin-A', '2024-01-15 09:15:00 +07:00'), -- Gốc: 1
('29Z-00001', 0, 1, 'Honda', 'Wave Alpha', 2018, 1, 32, 'Admin-B', '2024-01-20 10:40:00 +07:00'), -- Gốc: 2
('29B-99999', 1, 0, 'Ford', 'Transit', 2021, 1, 34, 'Gara-MK', '2024-03-05 14:00:00 +07:00'), -- Gốc: 4
('30K-12345', 2, 0, 'Vinfast', 'VF e34', 2023, 1, 34, 'Gara-MK', '2024-03-05 14:01:00 +07:00'), -- Gốc: 4

-- ------------------------------------------------------------------------------------------------------------------
-- Gốc UserId 16-40 (Đã sửa thành 46-50) & Bắt đầu từ 31 trở đi
-- ------------------------------------------------------------------------------------------------------------------
-- Khách hàng (Sử dụng 50 User cuối trong bảng)
('30E-11122', 0, 0, 'Mazda', '3', 2020, 1, 46, 'User-46', '2024-06-15 16:30:00 +07:00'), -- Gốc: 16 -> 46 (User Chưa Kích Hoạt 2)
('34F-33445', 0, 0, 'Hyundai', 'Accent', 2021, 1, 47, 'User-47', '2024-06-20 17:30:00 +07:00'),
('51G-55667', 0, 0, 'Kia', 'K3', 2022, 1, 48, 'User-48', '2024-06-25 18:30:00 +07:00'),
('70H-77889', 0, 0, 'Vinfast', 'Fadil', 2020, 1, 49, 'User-49', '2024-07-01 19:30:00 +07:00'),
('43I-99001', 0, 0, 'Mitsubishi', 'Xpander', 2023, 1, 50, 'User-50', '2024-07-05 09:30:00 +07:00'),

-- Tiếp tục điều chỉnh các ID còn lại về phạm vi 31-50 hoặc ID có sẵn.
-- Lưu ý: Từ đây, tôi sẽ dùng ID 31-50 vì chỉ có chúng tồn tại.
('29L-12312', 1, 0, 'Toyota', 'Fortuner', 2019, 1, 32, 'User-32', '2024-07-10 10:30:00 +07:00'),
('34M-34534', 1, 0, 'Ford', 'Everest', 2020, 1, 33, 'User-33', '2024-07-15 11:30:00 +07:00'),
('51N-56756', 1, 0, 'Hyundai', 'SantaFe', 2021, 1, 34, 'User-34', '2024-07-20 12:30:00 +07:00'),
('70P-78978', 0, 1, 'Kia', 'Morning', 2018, 1, 35, 'User-35', '2024-07-25 13:30:00 +07:00'),
('43Q-90190', 0, 1, 'Hyundai', 'i10', 2017, 1, 36, 'User-36', '2024-08-01 14:30:00 +07:00'),
('51R-11234', 0, 1, 'Chevrolet', 'Spark', 2016, 1, 37, 'User-37', '2024-08-05 15:30:00 +07:00'),
('29S-56789', 0, 1, 'Mazda', '2', 2019, 1, 38, 'User-38', '2024-08-10 16:30:00 +07:00'),
('30T-00005', 2, 3, 'Vinfast', 'VF8', 2024, 1, 39, 'User-39', '2024-08-15 17:30:00 +07:00'), 
('34U-10010', 2, 3, 'Porsche', 'Taycan', 2023, 1, 40, 'User-40', '2024-08-20 18:30:00 +07:00'), 
('51V-20020', 3, 2, 'Toyota', 'Corolla Cross', 2022, 1, 41, 'User-41', '2024-08-25 19:30:00 +07:00'), 
('70W-30030', 3, 2, 'Honda', 'CR-V Hybrid', 2024, 1, 42, 'User-42', '2024-09-01 09:30:00 +07:00'), 
('43X-40040', 1, 0, 'Hyundai', 'Tucson', 2016, 1, 43, 'User-43', '2024-09-05 10:30:00 +07:00'),
('51Y-50050', 0, 2, 'Toyota', 'Vios', 2017, 1, 44, 'User-44', '2024-09-10 11:30:00 +07:00'), 
('29Z-60060', 0, 2, 'Honda', 'Civic', 2018, 1, 45, 'User-45', '2024-09-15 12:30:00 +07:00'), 
('30A-70070', 0, 0, 'Mercedes', 'C200', 2019, 0, 46, 'Admin-G', '2024-09-20 13:30:00 +07:00'), 
('34B-80080', 1, 0, 'BMW', 'X5', 2020, 0, 47, 'Admin-G', '2024-09-25 14:30:00 +07:00'), 
('51C-90090', 0, 3, 'Audi', 'A4', 2020, 1, 48, 'User-48', '2024-10-01 15:30:00 +07:00'), 
('70D-10101', 0, 0, 'Volvo', 'XC60', 2023, 1, 49, 'User-49', '2024-10-05 16:30:00 +07:00'), 
('43E-20202', 1, 0, 'Jeep', 'Wrangler', 2022, 1, 50, 'User-50', '2024-10-10 17:30:00 +07:00'),
('51F-30303', 0, 0, 'Subaru', 'Forester', 2021, 1, 31, 'User-31', '2024-10-16 09:30:00 +07:00'); -- Dùng lại ID 31

GO