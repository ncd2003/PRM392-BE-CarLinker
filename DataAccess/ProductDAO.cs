using BusinessObjects;
using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ProductDAO
    {
        private readonly MyDbContext _context;
        private readonly ILogger<ProductDAO> _logger;

        public ProductDAO(MyDbContext context, ILogger<ProductDAO> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Lấy danh sách sản phẩm (đã lọc, sắp xếp, phân trang) để hiển thị.
        /// Chỉ tải biến thể mặc định (IsDefault) để hiển thị giá.
        /// </summary>
        public async Task<List<Product>> GetProductsAsync(ProductFilterParams filterParams)
        {
            // (Giả định ProductFilterParams chứa: CategoryId, BrandId, SortBy, Page, PageSize...)

            var query = _context.Product
                //.Include(p => p.ProductVariants.Where(v => v.IsDefault && v.IsActive))
                //.Where(p => p.IsActive);
                .Include(p => p.ProductImages)
                .AsQueryable();

            // --- Lọc (Filtering) ---
            if (filterParams.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filterParams.CategoryId.Value);
            }
            if (filterParams.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == filterParams.BrandId.Value);
            }
            //if (filterParams.IsFeatured.HasValue)
            //{
            //    query = query.Where(p => p.IsFeatured == filterParams.IsFeatured.Value);
            //}

            // --- Sắp xếp (Sorting) ---
            // (Thêm logic sắp xếp phức tạp hơn nếu cần)
            query = query.OrderByDescending(p => p.Id);

            // --- Phân trang (Paging) ---
            // (Bạn nên dùng một PagedList helper, nhưng đây là logic cơ bản)
            //query = query.Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            //             .Take(filterParams.PageSize);

            return await query.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Lấy chi tiết MỘT sản phẩm (HÀM QUAN TRỌNG NHẤT).
        /// Tải tất cả Options, Values, và Variants để Frontend xử lý logic chọn.
        /// </summary>
        public async Task<Product?> GetProductDetailsAsync(int productId)
        {
            return await _context.Product
                // 1. Lấy thông tin chung
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Manufacturer)
                .Include(p => p.ProductImages)

                // 2. Tải Options và Values
                .Include(p => p.ProductOptions)
                    .ThenInclude(po => po.OptionValues)

                // 3. Tải Variants đang hoạt động
                .Include(p => p.ProductVariants.Where(v => v.IsActive))
                    .ThenInclude(pv => pv.ProductVariantOptions)
                        .ThenInclude(pvo => pvo.OptionValue)

                .AsNoTracking()
                .AsSplitQuery() // <-- Thêm dòng này để tối ưu
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo tên.
        /// </summary>
        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _context.Product
               .Include(p => p.ProductVariants.Where(v => v.IsDefault && v.IsActive))
               .Where(p => p.IsActive && p.Name.Contains(searchTerm))
               .OrderBy(p => p.Name)
               .AsNoTracking()
               .ToListAsync();
        }

        /// <summary>
        /// [Admin] Tạo sản phẩm mới (Phiên bản đơn giản).
        /// Nghiệp vụ phức tạp (tạo cả options, variants) nên được xử lý ở Service.
        /// </summary>
        public async Task<Product> CreateProductAsync(Product product)
        {
            await _context.Product.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }
        /// <summary>
        /// [Admin] Cập nhật thông tin cơ bản của sản phẩm (tên, mô tả, ảnh, trạng thái...).
        /// </summary>
        public async Task<Product?> UpdateProductAsync(Product productToUpdate)
        {
            try
            {
                // 1. Tải sản phẩm gốc (existingProduct) VÀ CẢ ẢNH CŨ
                var existingProduct = await _context.Product
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == productToUpdate.Id);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Không tìm thấy Product {ProductId} để cập nhật", productToUpdate.Id);
                    return null;
                }

                // 2. Cập nhật thủ công TỪNG TRƯỜNG CƠ BẢN VÀ FOREIGN KEY
                existingProduct.CategoryId = productToUpdate.CategoryId;
                existingProduct.ManufacturerId = productToUpdate.ManufacturerId;
                existingProduct.BrandId = productToUpdate.BrandId;
                existingProduct.Name = productToUpdate.Name;
                existingProduct.Description = productToUpdate.Description;
                existingProduct.WarrantyPeriod = productToUpdate.WarrantyPeriod;
                existingProduct.IsActive = productToUpdate.IsActive;
                existingProduct.IsFeatured = productToUpdate.IsFeatured;

                // Cập nhật thời gian
                if (existingProduct is BaseModel baseModel)
                {
                    baseModel.UpdatedAt = DateTime.UtcNow;
                }

                // 3. SỬA: XỬ LÝ CẬP NHẬT ẢNH (Thay thế hoàn toàn)

                // 3a. Xóa tất cả ảnh cũ (báo cho EF biết)
                existingProduct.ProductImages.Clear();

                // 3b. Thêm tất cả ảnh mới (từ đối tượng controller gửi lên)
                if (productToUpdate.ProductImages != null)
                {
                    foreach (var newImage in productToUpdate.ProductImages)
                    {
                        // newImage là đối tượng mới, chưa được theo dõi
                        // Thêm vào collection của existingProduct để EF Core biết
                        // là cần 'INSERT' các record mới này
                        existingProduct.ProductImages.Add(newImage);
                    }
                }

                // 4. SaveChangesAsync
                // (EF sẽ tự động xóa ảnh cũ, thêm ảnh mới, và update product)
                await _context.SaveChangesAsync();

                // 5. Trả về đối tượng đã được cập nhật
                return existingProduct;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi concurrency (tranh chấp) khi cập nhật Product {ProductId}", productToUpdate.Id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi cập nhật Product {ProductId}", productToUpdate.Id);
                return null;
            }
        }

        //// <summary>
        /// [Admin] Thêm một biến thể mới VÀ các liên kết OptionValue của nó.
        /// Tự động tạo 'Name' cho biến thể từ các giá trị được chọn.
        /// Hàm này sử dụng Transaction.
        /// </summary>
        public async Task<ProductVariant?> AddProductVariantAsync(ProductVariant newVariant, List<int> selectedOptionValueIds)
        {
            // Bắt đầu một Transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // --- BƯỚC 1A: Kiểm tra ProductId hợp lệ ---
                var productExists = await _context.Product.AnyAsync(p => p.Id == newVariant.ProductId);
                if (!productExists)
                {
                    _logger.LogWarning("Thêm biến thể thất bại: ProductId {ProductId} không tồn tại.", newVariant.ProductId);
                    await transaction.RollbackAsync();
                    return null;
                }


                // --- BƯỚC 1B: TỰ ĐỘNG TẠO TÊN (ĐÂY LÀ PHẦN BỔ SUNG QUAN TRỌNG) ---
                // Chỉ tạo tên nếu nó chưa được cung cấp (mặc dù DTO của chúng ta không có)
                if (string.IsNullOrWhiteSpace(newVariant.Name))
                {
                    // 1. Lấy "Value" (string) từ các ID
                    // Sắp xếp theo OptionId để đảm bảo thứ tự (ví dụ: Size luôn trước Màu)
                    var selectedValues = await _context.OptionValue
                        .Where(ov => selectedOptionValueIds.Contains(ov.Id))
                        .OrderBy(ov => ov.OptionId)
                        .Select(ov => ov.Value)
                        .ToListAsync();

                    // 2. Nối chúng lại
                    if (selectedValues.Any())
                    {
                        newVariant.Name = string.Join(" - ", selectedValues);
                        // Kết quả: "128GB - Đen"
                    }
                    else
                    {
                        // Điều này không nên xảy ra nếu DTO [MinLength(1)]
                        _logger.LogError("Không thể tạo tên biến thể vì không có giá trị nào được chọn.");
                        await transaction.RollbackAsync();
                        return null;
                    }
                }
                // --- KẾT THÚC BƯỚC 1B ---


                // --- BƯỚC 2: Thêm ProductVariant chính (để lấy Id) ---
                // Giờ đây 'newVariant.Name' đã có giá trị!

                if (newVariant is BaseModel baseModel)
                {
                    baseModel.CreatedAt = DateTime.UtcNow;
                    baseModel.UpdatedAt = DateTime.UtcNow;
                }
                newVariant.IsActive = true;

                // Dòng này sẽ không còn lỗi
                await _context.ProductVariant.AddAsync(newVariant);
                await _context.SaveChangesAsync();
                // Sau bước này, newVariant.Id đã có giá trị

                // --- BƯỚC 3: Tạo các liên kết trong ProductVariantOption ---
                if (selectedOptionValueIds == null || !selectedOptionValueIds.Any()) // Kiểm tra lại
                {
                    _logger.LogError("Thêm biến thể thất bại: Không có OptionValueId nào được chọn.");
                    await transaction.RollbackAsync();
                    return null;
                }

                foreach (var valueId in selectedOptionValueIds)
                {
                    var variantOption = new ProductVariantOption
                    {
                        VariantId = newVariant.Id,
                        OptionValueId = valueId
                    };
                    // Thêm vào DbSet, nhưng dùng AddRangeAsync sau sẽ hiệu quả hơn
                    await _context.ProductVariantOption.AddAsync(variantOption);
                }

                // --- BƯỚC 4: Lưu các liên kết ---
                await _context.SaveChangesAsync();

                // --- BƯỚC 5: Commit Transaction ---
                await transaction.CommitAsync();

                _logger.LogInformation("Đã thêm thành công biến thể {VariantId} cho Product {ProductId}", newVariant.Id, newVariant.ProductId);
                return newVariant; // Trả về variant đã có Id
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm mới ProductVariant (dùng Transaction) cho Product {ProductId}", newVariant.ProductId);
                await transaction.RollbackAsync(); // Rất quan trọng: Hoàn tác nếu có lỗi
                return null;
            }
        }

        /// <summary>
        /// [Admin] Cập nhật một biến thể (giá, tồn kho).
        /// </summary>
        public async Task<bool> UpdateVariantAsync(ProductVariant variantToUpdate)
        {
            _context.ProductVariant.Update(variantToUpdate);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// [Admin] Xóa mềm một sản phẩm (set IsActive = false).
        /// Không nên xóa cứng vì còn liên quan đến các đơn hàng cũ.
        /// </summary>
        /// <summary>
        /// [Admin] Xóa mềm một sản phẩm (set IsActive = false).
        /// Thao tác này sẽ tự động xóa mềm TẤT CẢ các biến thể con.
        /// KHÔNG cần xóa ProductOption/OptionValue vì chúng sẽ bị ẩn theo Product.
        /// </summary>
        public async Task<bool> DeleteProductAsync(int productId)
        {
            // Dùng Transaction để đảm bảo an toàn
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tìm sản phẩm VÀ các biến thể liên quan
                var product = await _context.Product
                    .Include(p => p.ProductVariants)
                    .FirstOrDefaultAsync(a => a.Id == productId);

                if (product == null)
                {
                    _logger.LogWarning("Không tìm thấy Product {ProductId} để xóa mềm.", productId);
                    await transaction.RollbackAsync();
                    return false;
                }

                // 2. Đánh dấu xóa mềm sản phẩm cha
                product.IsActive = false;
                if (product is BaseModel baseProduct)
                {
                    baseProduct.UpdatedAt = DateTime.UtcNow;
                }

                // 3. Xóa mềm TẤT CẢ các biến thể của nó
                foreach (var variant in product.ProductVariants)
                {
                    variant.IsActive = false;
                    if (variant is BaseModel baseVariant)
                    {
                        baseVariant.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // 4. Lưu tất cả thay đổi
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Đã xóa mềm thành công Product {ProductId} và {VariantCount} biến thể.",
                    productId, product.ProductVariants.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mềm Product {ProductId}", productId);
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<ProductVariant>> GetProductVariantDefault()
        {
            return await _context.ProductVariant.Where(p => p.IsDefault == true && p.IsActive == true).ToListAsync();
        }

        // <summary>
        /// Lấy TẤT CẢ các biến thể (bao gồm cả biến thể bị ẩn/IsActive = false)
        /// thuộc về một sản phẩm cụ thể.
        /// </summary>
        public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariant
                .Where(v => v.ProductId == productId) // Lọc theo ProductId
                .AsNoTracking()
                .ToListAsync();
        }


        /// <summary>
        /// [Admin] Cập nhật thông tin chi tiết của MỘT biến thể (giá, tồn kho, SKU...).
        /// (Hàm này thay thế hàm UpdateVariantAsync cũ, dùng cách fetch-then-update an toàn hơn,
        /// giống với hàm UpdateProductAsync)
        /// </summary>
        public async Task<ProductVariant?> UpdateProductVariantAsync(ProductVariant variantToUpdate)
        {
            try
            {
                // 1. Tải biến thể gốc từ DB
                var existingVariant = await _context.ProductVariant
                    .FirstOrDefaultAsync(v => v.Id == variantToUpdate.Id);

                if (existingVariant == null)
                {
                    _logger.LogWarning("Không tìm thấy ProductVariant {VariantId} để cập nhật", variantToUpdate.Id);
                    return null;
                }

                // 2. Cập nhật thủ công các trường (KHÔNG BAO GỒM NAME)
                //    'Name' được gắn liền với các thuộc tính khi tạo, không nên thay đổi ở đây.

                // existingVariant.Name = variantToUpdate.Name; // <-- DÒNG NÀY ĐÃ BỊ XÓA

                existingVariant.Price = variantToUpdate.Price;
                existingVariant.CostPrice = variantToUpdate.CostPrice;
                existingVariant.Weight = variantToUpdate.Weight;
                existingVariant.SKU = variantToUpdate.SKU;
                existingVariant.StockQuantity = variantToUpdate.StockQuantity;
                existingVariant.HoldQuantity = variantToUpdate.HoldQuantity;
                existingVariant.Dimensions = variantToUpdate.Dimensions;
                existingVariant.IsDefault = variantToUpdate.IsDefault;
                existingVariant.IsActive = variantToUpdate.IsActive;

                // 3. Cập nhật thời gian
                if (existingVariant is BaseModel baseModel)
                {
                    baseModel.UpdatedAt = DateTime.UtcNow;
                }

                // 4. Lưu thay đổi
                await _context.SaveChangesAsync();
                return existingVariant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi cập nhật ProductVariant {VariantId}", variantToUpdate.Id);
                return null;
            }
        }


        /// <summary>
        /// [Admin] Xóa mềm một biến thể (set IsActive = false).
        /// Tương tự như DeleteProductAsync, không nên xóa cứng.
        /// </summary>
        public async Task<bool> DeleteProductVariantAsync(int productVariantId)
        {
            try
            {
                var variant = await _context.ProductVariant
                    .FirstOrDefaultAsync(v => v.Id == productVariantId);

                if (variant == null)
                {
                    _logger.LogWarning("Không tìm thấy ProductVariant {VariantId} để xóa mềm.", productVariantId);
                    return false; // Không tìm thấy
                }

                // Đánh dấu xóa mềm
                variant.IsActive = false;

                // Cập nhật thời gian
                if (variant is BaseModel baseModel)
                {
                    baseModel.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true; // Xóa mềm thành công
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mềm ProductVariant {VariantId}", productVariantId);
                return false;
            }

        }

        /// <summary>
        /// Thêm một thuộc tính mới (ví dụ: Màu sắc, Dung lượng) cho một sản phẩm.
        /// </summary>
        /// <param name="productOption">Đối tượng ProductOption đã được khởi tạo với ProductId, Name, Type...</param>
        /// <returns>Trả về ProductOption đã được tạo (với Id mới) hoặc null nếu thất bại.</returns>
        public async Task<ProductOption?> AddProductOptionAsync(ProductOption productOption)
        {
            // 1. Kiểm tra đầu vào cơ bản
            if (productOption == null || productOption.ProductId <= 0 || string.IsNullOrWhiteSpace(productOption.Name))
            {
                _logger.LogWarning("Thêm ProductOption thất bại: Dữ liệu đầu vào không hợp lệ.");
                return null;
            }

            try
            {
                // 2. Kiểm tra xem Product cha có tồn tại không
                var productExists = await _context.Product.AnyAsync(p => p.Id == productOption.ProductId);
                if (!productExists)
                {
                    _logger.LogWarning("Thêm ProductOption thất bại: ProductId {ProductId} không tồn tại.", productOption.ProductId);
                    return null;
                }

                // 3. Kiểm tra trùng lặp: Không cho phép 2 option CÙNG TÊN trên CÙNG 1 SẢN PHẨM
                var duplicateExists = await _context.ProductOption
                    .AnyAsync(po => po.ProductId == productOption.ProductId &&
                                   po.Name.ToLower() == productOption.Name.ToLower());

                if (duplicateExists)
                {
                    _logger.LogWarning("Thêm ProductOption thất bại: Tên thuộc tính '{OptionName}' đã tồn tại cho ProductId {ProductId}.",
                        productOption.Name, productOption.ProductId);
                    return null;
                }

                // 4. Thêm vào DbContext và Lưu
                _context.ProductOption.Add(productOption);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã thêm ProductOption (ID: {OptionId}, Name: {OptionName}) cho ProductId {ProductId}.",
                    productOption.Id, productOption.Name, productOption.ProductId);

                // 5. Trả về đối tượng vừa tạo (lúc này đã có Id)
                return productOption;
            }
            catch (Exception ex)
            {
                // Sửa lại log message của bạn cho đúng ngữ cảnh
                _logger.LogError(ex, "Lỗi khi thêm ProductOption (Name: {OptionName}) cho ProductId {ProductId}",
                    productOption.Name, productOption.ProductId);
                return null; // Trả về null khi có lỗi
            }
        }

        /// <summary>
        /// [Admin] Thêm một giá trị mới (ví dụ: "Đỏ", "128GB") cho một thuộc tính.
        /// </summary>
        /// <param name="optionValue">Đối tượng OptionValue đã được khởi tạo với OptionId và Value</param>
        /// <returns>Trả về OptionValue đã được tạo (với Id mới) hoặc null nếu thất bại.</returns>
        public async Task<OptionValue?> AddOptionValueAsync(OptionValue optionValue)
        {
            // 1. Kiểm tra đầu vào cơ bản
            if (optionValue == null || optionValue.OptionId <= 0 || string.IsNullOrWhiteSpace(optionValue.Value))
            {
                _logger.LogWarning("Thêm OptionValue thất bại: Dữ liệu đầu vào không hợp lệ.");
                return null;
            }

            try
            {
                // 2. Kiểm tra xem ProductOption (cha) có tồn tại không
                var optionExists = await _context.ProductOption.AnyAsync(po => po.Id == optionValue.OptionId);
                if (!optionExists)
                {
                    _logger.LogWarning("Thêm OptionValue thất bại: ProductOption (cha) với ID {OptionId} không tồn tại.", optionValue.OptionId);
                    return null;
                }

                // 3. Kiểm tra trùng lặp: Không cho phép 2 value CÙNG TÊN trên CÙNG 1 OPTION
                var duplicateValueExists = await _context.OptionValue
                    .AnyAsync(ov => ov.OptionId == optionValue.OptionId &&
                                   ov.Value.ToLower() == optionValue.Value.ToLower());

                if (duplicateValueExists)
                {
                    _logger.LogWarning("Thêm OptionValue thất bại: Giá trị '{Value}' đã tồn tại cho OptionId {OptionId}.",
                        optionValue.Value, optionValue.OptionId);
                    return null;
                }

                // 4. Thêm vào DbContext
                // (Giả định OptionValue cũng kế thừa từ BaseModel)
                if (optionValue is BaseModel baseModel)
                {
                    baseModel.CreatedAt = DateTime.UtcNow;
                    baseModel.UpdatedAt = DateTime.UtcNow;
                }

                _context.OptionValue.Add(optionValue); // (Giả định DbSet tên là OptionValue)

                // 5. Lưu
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã thêm OptionValue (ID: {ValueId}, Value: {Value}) cho OptionId {OptionId}.",
                    optionValue.Id, optionValue.Value, optionValue.OptionId);

                // 6. Trả về đối tượng vừa tạo
                return optionValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm OptionValue (Value: {Value}) cho OptionId {OptionId}",
                    optionValue.Value, optionValue.OptionId);
                return null; // Trả về null khi có lỗi
            }
        }

        /// <summary>
        /// [Admin] Lấy một thuộc tính (ProductOption) bằng ID.
        /// </summary>
        public async Task<ProductOption?> GetProductOptionAsync(int optionId)
        {
            try
            {
                return await _context.ProductOption.AsNoTracking()
                    .FirstOrDefaultAsync(po => po.Id == optionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy ProductOption {OptionId}", optionId);
                return null;
            }
        }

        /// <summary>
        /// [Admin] Cập nhật một thuộc tính (ví dụ: đổi tên "Màu sắc" thành "Color").
        /// </summary>
        public async Task<ProductOption?> UpdateProductOptionAsync(ProductOption optionToUpdate)
        {
            try
            {
                // 1. Tải đối tượng gốc
                var existingOption = await _context.ProductOption
                    .FirstOrDefaultAsync(po => po.Id == optionToUpdate.Id);

                if (existingOption == null)
                {
                    _logger.LogWarning("Không tìm thấy ProductOption {OptionId} để cập nhật", optionToUpdate.Id);
                    return null;
                }

                // 2. Kiểm tra trùng lặp tên (trong cùng một sản phẩm)
                var duplicateExists = await _context.ProductOption.AnyAsync(po =>
                    po.ProductId == existingOption.ProductId && // Cùng sản phẩm
                    po.Id != existingOption.Id &&               // Khác ID hiện tại
                    po.Name.ToLower() == optionToUpdate.Name.ToLower()); // Cùng tên

                if (duplicateExists)
                {
                    _logger.LogWarning("Cập nhật ProductOption thất bại: Tên '{OptionName}' đã tồn tại cho ProductId {ProductId}",
                        optionToUpdate.Name, existingOption.ProductId);
                    return null;
                }

                // 3. Cập nhật các trường
                existingOption.Name = optionToUpdate.Name;
                existingOption.Type = optionToUpdate.Type;
                existingOption.Unit = optionToUpdate.Unit;
                existingOption.IsRequired = optionToUpdate.IsRequired;

                if (existingOption is BaseModel baseModel)
                {
                    baseModel.UpdatedAt = DateTime.UtcNow;
                }

                // 4. Lưu
                await _context.SaveChangesAsync();
                return existingOption;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật ProductOption {OptionId}", optionToUpdate.Id);
                return null;
            }
        }

        /// <summary>
        /// [Admin] Xóa một thuộc tính (ProductOption).
        /// Chỉ xóa được nếu các giá trị (OptionValue) của nó không đang được
        /// sử dụng bởi bất kỳ biến thể (ProductVariant) nào.
        /// </summary>
        public async Task<bool> DeleteProductOptionAsync(int optionId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tìm thuộc tính và các giá trị con của nó
                var optionToDelete = await _context.ProductOption
                    .Include(po => po.OptionValues)
                    .FirstOrDefaultAsync(po => po.Id == optionId);

                if (optionToDelete == null)
                {
                    _logger.LogWarning("Không tìm thấy ProductOption {OptionId} để xóa", optionId);
                    await transaction.RollbackAsync();
                    return false;
                }

                // 2. (An toàn) Kiểm tra xem CÓ GIÁ TRỊ CON NÀO đang được dùng không
                var valueIds = optionToDelete.OptionValues.Select(ov => ov.Id).ToList();
                if (valueIds.Any())
                {
                    var isInUse = await _context.ProductVariantOption
                        .AnyAsync(pvo => valueIds.Contains(pvo.OptionValueId));

                    if (isInUse)
                    {
                        _logger.LogWarning("Không thể xóa ProductOption {OptionId} vì giá trị của nó đang được sử dụng bởi một biến thể.", optionId);
                        await transaction.RollbackAsync();
                        return false; // Báo thất bại
                    }
                }

                // 3. Nếu không có giá trị nào, hoặc có nhưng không bị dùng -> Xóa

                // 3a. Xóa các giá trị con (OptionValue) trước
                if (valueIds.Any())
                {
                    _context.OptionValue.RemoveRange(optionToDelete.OptionValues);
                }

                // 3b. Xóa thuộc tính cha (ProductOption)
                _context.ProductOption.Remove(optionToDelete);

                // 4. Lưu
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Đã xóa thành công ProductOption {OptionId} và các giá trị con của nó.", optionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa ProductOption {OptionId}", optionId);
                await transaction.RollbackAsync();
                return false;
            }


        }

        /// <summary>
        /// [Admin] Lấy một giá trị (OptionValue) bằng ID.
        /// </summary>
        public async Task<OptionValue?> GetOptionValueAsync(int valueId)
        {
            try
            {
                return await _context.OptionValue.AsNoTracking()
                    .FirstOrDefaultAsync(ov => ov.Id == valueId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy OptionValue {ValueId}", valueId);
                return null;
            }
        }

        /// <summary>
        /// [Admin] Cập nhật một giá trị (ví dụ: đổi "Đo" thành "Đỏ").
        /// </summary>
        public async Task<OptionValue?> UpdateOptionValueAsync(OptionValue valueToUpdate)
        {
            try
            {
                // 1. Tải đối tượng gốc
                var existingValue = await _context.OptionValue
                    .FirstOrDefaultAsync(ov => ov.Id == valueToUpdate.Id);

                if (existingValue == null)
                {
                    _logger.LogWarning("Không tìm thấy OptionValue {ValueId} để cập nhật", valueToUpdate.Id);
                    return null;
                }

                // 2. Kiểm tra trùng lặp (trong cùng một thuộc tính)
                var duplicateExists = await _context.OptionValue.AnyAsync(ov =>
                    ov.OptionId == existingValue.OptionId && // Cùng thuộc tính
                    ov.Id != existingValue.Id &&             // Khác ID hiện tại
                    ov.Value.ToLower() == valueToUpdate.Value.ToLower()); // Cùng giá trị

                if (duplicateExists)
                {
                    _logger.LogWarning("Cập nhật OptionValue thất bại: Giá trị '{Value}' đã tồn tại cho OptionId {OptionId}",
                        valueToUpdate.Value, existingValue.OptionId);
                    return null;
                }

                // 3. Cập nhật
                existingValue.Value = valueToUpdate.Value;

                if (existingValue is BaseModel baseModel)
                {
                    baseModel.UpdatedAt = DateTime.UtcNow;
                }

                // 4. Lưu
                await _context.SaveChangesAsync();
                return existingValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật OptionValue {ValueId}", valueToUpdate.Id);
                return null;
            }
        }

        /// <summary>
        /// [Admin] Xóa một giá trị (OptionValue).
        /// Chỉ xóa được nếu nó không đang được sử dụng bởi bất kỳ biến thể nào.
        /// </summary>
        public async Task<bool> DeleteOptionValueAsync(int valueId)
        {
            try
            {
                // 1. Tìm giá trị
                var valueToDelete = await _context.OptionValue
                    .FirstOrDefaultAsync(ov => ov.Id == valueId);

                if (valueToDelete == null)
                {
                    _logger.LogWarning("Không tìm thấy OptionValue {ValueId} để xóa", valueId);
                    return false;
                }

                // 2. (An toàn) Kiểm tra xem có đang bị dùng không
                var isInUse = await _context.ProductVariantOption
                    .AnyAsync(pvo => pvo.OptionValueId == valueId);

                if (isInUse)
                {
                    _logger.LogWarning("Không thể xóa OptionValue {ValueId} vì nó đang được sử dụng bởi một biến thể.", valueId);
                    return false; // Báo thất bại
                }

                // 3. Nếu không bị dùng -> Xóa
                _context.OptionValue.Remove(valueToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã xóa thành công OptionValue {ValueId}.", valueId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa OptionValue {ValueId}", valueId);
                return false;
            }
        }
    }
}