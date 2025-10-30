-- ============================================================
-- THÊM DỮ LIỆU MẪU - PHỤ TÙNG XE
-- (ĐÃ SỬA LỖI FONT TIẾNG VIỆT VỚI TIỀN TỐ N')
-- ============================================================

USE [TheVehicleEcosystem]
GO

-- ============================================================
-- 1. THÊM DANH MỤC (CATEGORY)
-- ============================================================

INSERT INTO [dbo].[Category] ([Name], [Description], [Image], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(N'Động Cơ & Truyền Động', N'Các bộ phận liên quan tới động cơ', N'engine.jpg', 1, N'Admin', GETDATE(), GETDATE()),
(N'Hệ Thống Phanh', N'Má phanh, đĩa phanh, dầu phanh', N'brake.jpg', 1, N'Admin', GETDATE(), GETDATE()),
(N'Hệ Thống Điện', N'Pin, bugi, máy phát điện, bộ điều hòa', N'electrical.jpg', 1, N'Admin', GETDATE(), GETDATE()),
(N'Hệ Thống Treo', N'Shock, lò xo, thanh tròn, đế các bánh', N'suspension.jpg', 1, N'Admin', GETDATE(), GETDATE()),
(N'Lốp Xe', N'Lốp tay, lốp máy, vỏ lốp', N'tires.jpg', 1, N'Admin', GETDATE(), GETDATE()),
(N'Dầu Nhớt', N'Dầu động cơ, dầu hộp số, dầu phanh', N'oil.jpg', 1, N'Admin', GETDATE(), GETDATE()),
(N'Lọc Gió & Lọc Dầu', N'Lọc gió động cơ, lọc dầu động cơ', N'filter.jpg', 1, N'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 2. THÊM NHÀ SỬ DỤNG (MANUFACTURER)
-- ============================================================

INSERT INTO [dbo].[Manufacturer] ([Name], [Country], [Website], [Description], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(N'Bosch', N'Germany', N'www.bosch.com', N'Nhà sản xuất phụ tùng xe hàng đầu thế giới', 1, N'Admin', GETDATE(), GETDATE()),
(N'Continental', N'Germany', N'www.continental.com', N'Chuyên về lốp xe và hệ thống điện', 1, N'Admin', GETDATE(), GETDATE()),
(N'Bridgestone', N'Japan', N'www.bridgestone.com', N'Nhà sản xuất lốp xe hàng đầu', 1, N'Admin', GETDATE(), GETDATE()),
(N'Denso', N'Japan', N'www.denso.com', N'Chuyên sản xuất linh kiện điện tử ô tô', 1, N'Admin', GETDATE(), GETDATE()),
(N'Shell', N'Netherlands', N'www.shell.com', N'Dầu nhớt chất lượng cao', 1, N'Admin', GETDATE(), GETDATE()),
(N'Castrol', N'United Kingdom', N'www.castrol.com', N'Dầu nhớt và chất lỏng ô tô', 1, N'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 3. THÊM THƯƠNG HIỆU (BRAND)
-- ============================================================

INSERT INTO [dbo].[Brand] ([Name], [Country], [Image], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(N'Toyota', N'Japan', N'toyota.jpg', 1, N'Admin', GETDATE(), GETDATE()),
(N'Honda', N'Japan', N'honda.jpg', 1, N'Admin', GETDATE(), GETDATE()),
(N'Ford', N'USA', N'ford.jpg', 1, N'Admin', GETDATE(), GETDATE()),
(N'BMW', N'Germany', N'bmw.jpg', 1, N'Admin', GETDATE(), GETDATE()),
(N'Kia', N'South Korea', N'kia.jpg', 1, N'Admin', GETDATE(), GETDATE()),
(N'Mazda', N'Japan', N'mazda.jpg', 1, N'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 4. THÊM SẢN PHẨM (PRODUCT)
-- ============================================================

INSERT INTO [dbo].[Product] ([CategoryId], [ManufacturerId], [BrandId], [Name], [Description], [WarrantyPeriod], [IsActive], [IsFeatured], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
-- Động Cơ & Truyền Động
(1, 1, 1, N'Bộ Lọc Gió Động Cơ Toyota Camry 2022', N'Lọc gió chính hãng cho Toyota Camry năm 2022', 12, 1, 1, N'Admin', GETDATE(), GETDATE()),
(1, 1, 2, N'Bộ Lọc Gió Honda Accord 2021', N'Lọc gió cao cấp cho Honda Accord', 12, 1, 0, N'Admin', GETDATE(), GETDATE()),
(1, 1, 3, N'Bộ Lọc Gió Ford Focus 2023', N'Lọc gió động cơ Ford Focus', 12, 1, 0, N'Admin', GETDATE(), GETDATE()),

-- Hệ Thống Phanh
(2, 1, 1, N'Bộ Má Phanh Trước Toyota Corolla', N'Má phanh chính hãng Bosch cho Toyota Corolla', 24, 1, 1, N'Admin', GETDATE(), GETDATE()),
(2, 1, 2, N'Bộ Má Phanh Sau Honda Civic', N'Má phanh cao cấp cho Honda Civic', 24, 1, 0, N'Admin', GETDATE(), GETDATE()),
(2, 1, 4, N'Đĩa Phanh BMW 3 Series', N'Đĩa phanh cao cấp Bosch cho BMW', 36, 1, 0, N'Admin', GETDATE(), GETDATE()),

-- Hệ Thống Điện
(3, 4, 1, N'Pin Ô Tô Denso 12V 60AH Toyota', N'Pin chính hãng Denso dung lượng 60AH', 24, 1, 1, N'Admin', GETDATE(), GETDATE()),
(3, 4, 2, N'Pin Ô Tô Denso 12V 70AH Honda', N'Pin Denso 70AH cho xe Honda', 24, 1, 0, N'Admin', GETDATE(), GETDATE()),
(3, 4, 4, N'Bugi Denso Platinum Toyota 4 cây', N'Bộ 4 bugi Platinum cho Toyota', 12, 1, 0, N'Admin', GETDATE(), GETDATE()),

-- Lốp Xe
(5, 3, 1, N'Lốp Bridgestone Turanza 185/60R15 Toyota', N'Lốp xe tiết kiệm xăng Bridgestone', 24, 1, 1, N'Admin', GETDATE(), GETDATE()),
(5, 2, 2, N'Lốp Continental 195/65R15 Honda', N'Lốp xe an toàn Continental', 24, 1, 1, N'Admin', GETDATE(), GETDATE()),
(5, 3, 3, N'Lốp Bridgestone 205/55R16 Ford', N'Lốp cao cấp Bridgestone cho Ford', 24, 1, 0, N'Admin', GETDATE(), GETDATE()),

-- Dầu Nhớt
(6, 5, 1, N'Dầu Shell Helix Ultra 5W-40 4L Toyota', N'Dầu động cơ cao cấp Shell 4 lít', 12, 1, 1, N'Admin', GETDATE(), GETDATE()),
(6, 6, 2, N'Dầu Castrol Edge 5W-30 4L Honda', N'Dầu Castrol cho các dòng xe Honda', 12, 1, 0, N'Admin', GETDATE(), GETDATE()),
(6, 5, 4, N'Dầu Shell Helix Ultra 0W-30 5L BMW', N'Dầu engine Shell cho BMW sang trọng', 12, 1, 0, N'Admin', GETDATE(), GETDATE()),

-- Lọc Gió & Lọc Dầu
(7, 1, 1, N'Lọc Dầu Toyota Camry OEM', N'Lọc dầu chính hãng Toyota', 12, 1, 1, N'Admin', GETDATE(), GETDATE()),
(7, 1, 2, N'Lọc Dầu Honda Accord OEM', N'Lọc dầu Honda chính hãng', 12, 1, 0, N'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 5. THÊM PRODUCT OPTION (Các tuỳ chọn như kích cỡ, màu sắc)
-- ============================================================

-- Lọc Gió Toyota Camry
INSERT INTO [dbo].[ProductOption] ([ProductId], [Name], [Type], [Unit], [IsRequired], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(1, N'Kích Cỡ', N'select', NULL, 1, N'Admin', GETDATE(), GETDATE());

-- Pin Ô Tô Denso
INSERT INTO [dbo].[ProductOption] ([ProductId], [Name], [Type], [Unit], [IsRequired], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(7, N'Dung Lượng', N'select', N'AH', 1, N'Admin', GETDATE(), GETDATE());

-- Lốp Bridgestone
INSERT INTO [dbo].[ProductOption] ([ProductId], [Name], [Type], [Unit], [IsRequired], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(9, N'Số Lốp', N'select', NULL, 1, N'Admin', GETDATE(), GETDATE());

-- Dầu Shell
INSERT INTO [dbo].[ProductOption] ([ProductId], [Name], [Type], [Unit], [IsRequired], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(15, N'Thể Tích', N'select', N'L', 1, N'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 6. THÊM OPTION VALUE (Giá trị của các tuỳ chọn)
-- ============================================================

-- Kích cỡ lọc gió Toyota
INSERT INTO [dbo].[OptionValue] ([OptionId], [Value], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(1, N'Standard OEM', N'Admin', GETDATE(), GETDATE()),
(1, N'Premium', N'Admin', GETDATE(), GETDATE());

-- Dung lượng pin
INSERT INTO [dbo].[OptionValue] ([OptionId], [Value], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(2, N'60AH', N'Admin', GETDATE(), GETDATE()),
(2, N'70AH', N'Admin', GETDATE(), GETDATE()),
(2, N'80AH', N'Admin', GETDATE(), GETDATE());

-- Số lốp
INSERT INTO [dbo].[OptionValue] ([OptionId], [Value], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(3, N'Lốp 1', N'Admin', GETDATE(), GETDATE()),
(3, N'Lốp 2', N'Admin', GETDATE(), GETDATE()),
(3, N'Lốp 3', N'Admin', GETDATE(), GETDATE()),
(3, N'Lốp 4', N'Admin', GETDATE(), GETDATE());

-- Thể tích dầu
INSERT INTO [dbo].[OptionValue] ([OptionId], [Value], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(4, N'4L', N'Admin', GETDATE(), GETDATE()),
(4, N'5L', N'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 7. THÊM PRODUCT VARIANT (Biến thể sản phẩm với giá khác nhau)
-- ============================================================

-- Lọc Gió Toyota Camry
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(1, N'Lọc Gió Toyota Camry - Standard', 250000, 150000, 0.3, N'FILTER-TOYOTA-STD-001', 50, 0, N'20x10x5', 1, 1, N'Admin', GETDATE(), GETDATE()),
(1, N'Lọc Gió Toyota Camry - Premium', 350000, 200000, 0.35, N'FILTER-TOYOTA-PRE-001', 30, 0, N'20x10x5', 0, 1, N'Admin', GETDATE(), GETDATE());

-- Má Phanh Trước Toyota Corolla
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(4, N'Bộ Má Phanh Trước Toyota Corolla', 1200000, 700000, 2.5, N'BRAKE-COROLLA-001', 20, 0, N'30x20x10', 1, 1, N'Admin', GETDATE(), GETDATE());

-- Pin Denso 60AH
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(7, N'Pin Denso 12V 60AH', 1500000, 900000, 15, N'PIN-DENSO-60AH-001', 40, 0, N'25x17x20', 1, 1, N'Admin', GETDATE(), GETDATE()),
(7, N'Pin Denso 12V 70AH', 1800000, 1000000, 18, N'PIN-DENSO-70AH-001', 35, 0, N'25x17x22', 0, 1, N'Admin', GETDATE(), GETDATE()),
(7, N'Pin Denso 12V 80AH', 2100000, 1200000, 20, N'PIN-DENSO-80AH-001', 25, 0, N'25x17x24', 0, 1, N'Admin', GETDATE(), GETDATE());

-- Lốp Bridgestone 185/60R15
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(9, N'Bridgestone Turanza 185/60R15 - 1 lốp', 800000, 450000, 10, N'TIRE-BRIDGE-185-1-001', 100, 0, N'68cm', 0, 1, N'Admin', GETDATE(), GETDATE()),
(9, N'Bridgestone Turanza 185/60R15 - 2 lốp', 1500000, 850000, 20, N'TIRE-BRIDGE-185-2-001', 50, 0, N'68cm x2', 0, 1, N'Admin', GETDATE(), GETDATE()),
(9, N'Bridgestone Turanza 185/60R15 - 4 lốp', 2900000, 1600000, 40, N'TIRE-BRIDGE-185-4-001', 30, 0, N'68cm x4', 1, 1, N'Admin', GETDATE(), GETDATE());

-- Dầu Shell 4L
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(15, N'Shell Helix Ultra 5W-40 - 4L', 450000, 280000, 4, N'OIL-SHELL-4L-001', 100, 0, N'20x10x10', 1, 1, N'Admin', GETDATE(), GETDATE()),
(15, N'Shell Helix Ultra 5W-40 - 5L', 550000, 330000, 5, N'OIL-SHELL-5L-001', 80, 0, N'20x10x15', 0, 1, N'Admin', GETDATE(), GETDATE());

-- Lọc Dầu Toyota
INSERT INTO [dbo].[ProductVariant] ([ProductId], [Name], [Price], [CostPrice], [Weight], [SKU], [StockQuantity], [HoldQuantity], [Dimensions], [IsDefault], [IsActive], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(17, N'Lọc Dầu Toyota Camry OEM', 180000, 100000, 0.5, N'OILFILTER-TOYOTA-001', 80, 0, N'10x8x8', 1, 1, N'Admin', GETDATE(), GETDATE());

-- ============================================================
-- 8. THÊM PRODUCT VARIANT OPTION (Liên kết Variant với OptionValue)
-- ============================================================

-- Lọc Gió Standard
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(1, 1, N'Admin', GETDATE(), GETDATE());

-- Lọc Gió Premium
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(2, 2, N'Admin', GETDATE(), GETDATE());

-- Pin 60AH
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(4, 3, N'Admin', GETDATE(), GETDATE());

-- Pin 70AH
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(5, 4, N'Admin', GETDATE(), GETDATE());

-- Pin 80AH
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(6, 5, N'Admin', GETDATE(), GETDATE());

-- Lốp 1 cái
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(7, 6, N'Admin', GETDATE(), GETDATE());

-- Lốp 2 cái
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(8, 7, N'Admin', GETDATE(), GETDATE());

-- Lốp 4 cái
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(9, 9, N'Admin', GETDATE(), GETDATE());

-- Dầu 4L
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(10, 10, N'Admin', GETDATE(), GETDATE());

-- Dầu 5L
INSERT INTO [dbo].[ProductVariantOption] ([VariantId], [OptionValueId], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
(11, 11, N'Admin', GETDATE(), GETDATE());

-- ============================================================
-- HOÀN THÀNH
-- ============================================================