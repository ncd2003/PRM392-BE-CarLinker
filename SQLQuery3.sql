-- ============================================================
-- THÊM DỮ LIỆU MẪU - PHỤ TÙNG XE
-- ============================================================

USE [TheVehicleEcosystem]
GO

-- ============================================================
-- 1. THÊM DANH MỤC (CATEGORY)
-- ============================================================

INSERT INTO [dbo].[Category] ([Name], [Description], [Image], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
('Động Cơ & Truyền Động', 'Các bộ phận liên quan tới động cơ', 'engine.jpg', 1, 'Admin', GETDATE(), GETDATE()),
('Hệ Thống Phanh', 'Má phanh, đĩa phanh, dầu phanh', 'brake.jpg', 1, 'Admin', GETDATE(), GETDATE()),
('Hệ Thống Điện', 'Pin, bugi, máy phát điện, bộ điều hòa', 'electrical.jpg', 1, 'Admin', GETDATE(), GETDATE()),
('Hệ Thống Treo', 'Shock, lò xo, thanh tròn, đế các bánh', 'suspension.jpg', 1, 'Admin', GETDATE(), GETDATE()),
('Lốp Xe', 'Lốp tay, lốp máy, vỏ lốp', 'tires.jpg', 1, 'Admin', GETDATE(), GETDATE()),
('Dầu Nhớt', 'Dầu động cơ, dầu hộp số, dầu phanh', 'oil.jpg', 1, 'Admin', GETDATE(), GETDATE()),
('Lọc Gió & Lọc Dầu', 'Lọc gió động cơ, lọc dầu động cơ', 'filter.jpg', 1, 'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 2. THÊM NHÀ SỬ DỤNG (MANUFACTURER)
-- ============================================================

INSERT INTO [dbo].[Manufacturer] ([Name], [Country], [Website], [Description], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
('Bosch', 'Germany', 'www.bosch.com', 'Nhà sản xuất phụ tùng xe hàng đầu thế giới', 1, 'Admin', GETDATE(), GETDATE()),
('Continental', 'Germany', 'www.continental.com', 'Chuyên về lốp xe và hệ thống điện', 1, 'Admin', GETDATE(), GETDATE()),
('Bridgestone', 'Japan', 'www.bridgestone.com', 'Nhà sản xuất lốp xe hàng đầu', 1, 'Admin', GETDATE(), GETDATE()),
('Denso', 'Japan', 'www.denso.com', 'Chuyên sản xuất linh kiện điện tử ô tô', 1, 'Admin', GETDATE(), GETDATE()),
('Shell', 'Netherlands', 'www.shell.com', 'Dầu nhớt chất lượng cao', 1, 'Admin', GETDATE(), GETDATE()),
('Castrol', 'United Kingdom', 'www.castrol.com', 'Dầu nhớt và chất lỏng ô tô', 1, 'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 3. THÊM THƯƠNG HIỆU (BRAND)
-- ============================================================

INSERT INTO [dbo].[Brand] ([Name], [Country], [Image], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
('Toyota', 'Japan', 'toyota.jpg', 1, 'Admin', GETDATE(), GETDATE()),
('Honda', 'Japan', 'honda.jpg', 1, 'Admin', GETDATE(), GETDATE()),
('Ford', 'USA', 'ford.jpg', 1, 'Admin', GETDATE(), GETDATE()),
('BMW', 'Germany', 'bmw.jpg', 1, 'Admin', GETDATE(), GETDATE()),
('Kia', 'South Korea', 'kia.jpg', 1, 'Admin', GETDATE(), GETDATE()),
('Mazda', 'Japan', 'mazda.jpg', 1, 'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 4. THÊM SẢN PHẨM (PRODUCT)
-- ============================================================

INSERT INTO [dbo].[Product] ([CategoryId], [ManufacturerId], [BrandId], [Name], [Description], [WarrantyPeriod], [IsActive], [IsFeatured], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
-- Động Cơ & Truyền Động
(1, 1, 1, 'Bộ Lọc Gió Động Cơ Toyota Camry 2022', 'Lọc gió chính hãng cho Toyota Camry năm 2022', 12, 1, 1, 'Admin', GETDATE(), GETDATE()),
(1, 1, 2, 'Bộ Lọc Gió Honda Accord 2021', 'Lọc gió cao cấp cho Honda Accord', 12, 1, 0, 'Admin', GETDATE(), GETDATE()),
(1, 1, 3, 'Bộ Lọc Gió Ford Focus 2023', 'Lọc gió động cơ Ford Focus', 12, 1, 0, 'Admin', GETDATE(), GETDATE()),

-- Hệ Thống Phanh
(2, 1, 1, 'Bộ Má Phanh Trước Toyota Corolla', 'Má phanh chính hãng Bosch cho Toyota Corolla', 24, 1, 1, 'Admin', GETDATE(), GETDATE()),
(2, 1, 2, 'Bộ Má Phanh Sau Honda Civic', 'Má phanh cao cấp cho Honda Civic', 24, 1, 0, 'Admin', GETDATE(), GETDATE()),
(2, 1, 4, 'Đĩa Phanh BMW 3 Series', 'Đĩa phanh cao cấp Bosch cho BMW', 36, 1, 0, 'Admin', GETDATE(), GETDATE()),

-- Hệ Thống Điện
(3, 4, 1, 'Pin Ô Tô Denso 12V 60AH Toyota', 'Pin chính hãng Denso dung lượng 60AH', 24, 1, 1, 'Admin', GETDATE(), GETDATE()),
(3, 4, 2, 'Pin Ô Tô Denso 12V 70AH Honda', 'Pin Denso 70AH cho xe Honda', 24, 1, 0, 'Admin', GETDATE(), GETDATE()),
(3, 4, 4, 'Bugi Denso Platinum Toyota 4 cây', 'Bộ 4 bugi Platinum cho Toyota', 12, 1, 0, 'Admin', GETDATE(), GETDATE()),

-- Lốp Xe
(5, 3, 1, 'Lốp Bridgestone Turanza 185/60R15 Toyota', 'Lốp xe tiết kiệm xăng Bridgestone', 24, 1, 1, 'Admin', GETDATE(), GETDATE()),
(5, 2, 2, 'Lốp Continental 195/65R15 Honda', 'Lốp xe an toàn Continental', 24, 1, 1, 'Admin', GETDATE(), GETDATE()),
(5, 3, 3, 'Lốp Bridgestone 205/55R16 Ford', 'Lốp cao cấp Bridgestone cho Ford', 24, 1, 0, 'Admin', GETDATE(), GETDATE()),

-- Dầu Nhớt
(6, 5, 1, 'Dầu Shell Helix Ultra 5W-40 4L Toyota', 'Dầu động cơ cao cấp Shell 4 lít', 12, 1, 1, 'Admin', GETDATE(), GETDATE()),
(6, 6, 2, 'Dầu Castrol Edge 5W-30 4L Honda', 'Dầu Castrol cho các dòng xe Honda', 12, 1, 0, 'Admin', GETDATE(), GETDATE()),
(6, 5, 4, 'Dầu Shell Helix Ultra 0W-30 5L BMW', 'Dầu engine Shell cho BMW sang trọng', 12, 1, 0, 'Admin', GETDATE(), GETDATE()),

-- Lọc Gió & Lọc Dầu
(7, 1, 1, 'Lọc Dầu Toyota Camry OEM', 'Lọc dầu chính hãng Toyota', 12, 1, 1, 'Admin', GETDATE(), GETDATE()),
(7, 1, 2, 'Lọc Dầu Honda Accord OEM', 'Lọc dầu Honda chính hãng', 12, 1, 0, 'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 5. THÊM PRODUCT OPTION (Các tuỳ chọn như kích cỡ, màu sắc)
-- ============================================================

-- Lọc Gió Toyota Camry
INSERT INTO [dbo].[ProductOption] ([ProductId], [Name], [Type], [Unit], [IsRequired], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(1, 'Kích Cỡ', 'select', NULL, 1, 'Admin', GETDATE(), GETDATE());

-- Pin Ô Tô Denso
INSERT INTO [dbo].[ProductOption] ([ProductId], [Name], [Type], [Unit], [IsRequired], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(7, 'Dung Lượng', 'select', 'AH', 1, 'Admin', GETDATE(), GETDATE());

-- Lốp Bridgestone
INSERT INTO [dbo].[ProductOption] ([ProductId], [Name], [Type], [Unit], [IsRequired], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(9, 'Số Lốp', 'select', NULL, 1, 'Admin', GETDATE(), GETDATE());

-- Dầu Shell
INSERT INTO [dbo].[ProductOption] ([ProductId], [Name], [Type], [Unit], [IsRequired], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(15, 'Thể Tích', 'select', 'L', 1, 'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 6. THÊM OPTION VALUE (Giá trị của các tuỳ chọn)
-- ============================================================

-- Kích cỡ lọc gió Toyota
INSERT INTO [dbo].[OptionValue] ([OptionId], [Value], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(1, 'Standard OEM', 'Admin', GETDATE(), GETDATE()),
(1, 'Premium', 'Admin', GETDATE(), GETDATE());

-- Dung lượng pin
INSERT INTO [dbo].[OptionValue] ([OptionId], [Value], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(2, '60AH', 'Admin', GETDATE(), GETDATE()),
(2, '70AH', 'Admin', GETDATE(), GETDATE()),
(2, '80AH', 'Admin', GETDATE(), GETDATE());

-- Số lốp
INSERT INTO [dbo].[OptionValue] ([OptionId], [Value], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(3, 'Lốp 1', 'Admin', GETDATE(), GETDATE()),
(3, 'Lốp 2', 'Admin', GETDATE(), GETDATE()),
(3, 'Lốp 3', 'Admin', GETDATE(), GETDATE()),
(3, 'Lốp 4', 'Admin', GETDATE(), GETDATE());

-- Thể tích dầu
INSERT INTO [dbo].[OptionValue] ([OptionId], [Value], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(4, '4L', 'Admin', GETDATE(), GETDATE()),
(4, '5L', 'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 7. THÊM PRODUCT VARIANT (Biến thể sản phẩm với giá khác nhau)
-- ============================================================

-- Lọc Gió Toyota Camry
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(1, 'Lọc Gió Toyota Camry - Standard', 250000, 150000, 0.3, 'FILTER-TOYOTA-STD-001', 50, 0, '20x10x5', 1, 1, 'Admin', GETDATE(), GETDATE()),
(1, 'Lọc Gió Toyota Camry - Premium', 350000, 200000, 0.35, 'FILTER-TOYOTA-PRE-001', 30, 0, '20x10x5', 0, 1, 'Admin', GETDATE(), GETDATE());

-- Má Phanh Trước Toyota Corolla
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(4, 'Bộ Má Phanh Trước Toyota Corolla', 1200000, 700000, 2.5, 'BRAKE-COROLLA-001', 20, 0, '30x20x10', 1, 1, 'Admin', GETDATE(), GETDATE());

-- Pin Denso 60AH
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(7, 'Pin Denso 12V 60AH', 1500000, 900000, 15, 'PIN-DENSO-60AH-001', 40, 0, '25x17x20', 1, 1, 'Admin', GETDATE(), GETDATE()),
(7, 'Pin Denso 12V 70AH', 1800000, 1000000, 18, 'PIN-DENSO-70AH-001', 35, 0, '25x17x22', 0, 1, 'Admin', GETDATE(), GETDATE()),
(7, 'Pin Denso 12V 80AH', 2100000, 1200000, 20, 'PIN-DENSO-80AH-001', 25, 0, '25x17x24', 0, 1, 'Admin', GETDATE(), GETDATE());

-- Lốp Bridgestone 185/60R15
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(9, 'Bridgestone Turanza 185/60R15 - 1 lốp', 800000, 450000, 10, 'TIRE-BRIDGE-185-1-001', 100, 0, '68cm', 0, 1, 'Admin', GETDATE(), GETDATE()),
(9, 'Bridgestone Turanza 185/60R15 - 2 lốp', 1500000, 850000, 20, 'TIRE-BRIDGE-185-2-001', 50, 0, '68cm x2', 0, 1, 'Admin', GETDATE(), GETDATE()),
(9, 'Bridgestone Turanza 185/60R15 - 4 lốp', 2900000, 1600000, 40, 'TIRE-BRIDGE-185-4-001', 30, 0, '68cm x4', 1, 1, 'Admin', GETDATE(), GETDATE());

-- Dầu Shell 4L
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(15, 'Shell Helix Ultra 5W-40 - 4L', 450000, 280000, 4, 'OIL-SHELL-4L-001', 100, 0, '20x10x10', 1, 1, 'Admin', GETDATE(), GETDATE()),
(15, 'Shell Helix Ultra 5W-40 - 5L', 550000, 330000, 5, 'OIL-SHELL-5L-001', 80, 0, '20x10x15', 0, 1, 'Admin', GETDATE(), GETDATE());

-- Lọc Dầu Toyota
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(17, 'Lọc Dầu Toyota Camry OEM', 180000, 100000, 0.5, 'OILFILTER-TOYOTA-001', 80, 0, '10x8x8', 1, 1, 'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 8. THÊM PRODUCT VARIANT OPTION (Liên kết Variant với OptionValue)
-- ============================================================

-- Lọc Gió Standard
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(1, 1, 'Admin', GETDATE(), GETDATE());

-- Lọc Gió Premium
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(2, 2, 'Admin', GETDATE(), GETDATE());

-- Pin 60AH
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(4, 3, 'Admin', GETDATE(), GETDATE());

-- Pin 70AH
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(5, 4, 'Admin', GETDATE(), GETDATE());

-- Pin 80AH
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(6, 5, 'Admin', GETDATE(), GETDATE());

-- Lốp 1 cái
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(7, 6, 'Admin', GETDATE(), GETDATE());

-- Lốp 2 cái
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(8, 7, 'Admin', GETDATE(), GETDATE());

-- Lốp 4 cái
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(9, 9, 'Admin', GETDATE(), GETDATE());

-- Dầu 4L
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(10, 10, 'Admin', GETDATE(), GETDATE());

-- Dầu 5L
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(11, 11, 'Admin', GETDATE(), GETDATE());

-- ============================================================
-- HOÀN THÀNH
-- ============================================================

