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
                .Where(p => p.IsActive);

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

                // 2. Tải Options và Values
                .Include(p => p.ProductOptions)
                    .ThenInclude(po => po.OptionValues)

                // 3. Tải Variants đang hoạt động
                .Include(p => p.ProductVariants.Where(v => v.IsActive))
                    .ThenInclude(pv => pv.ProductVariantOptions)
                        .ThenInclude(pvo => pvo.OptionValue)

                .AsNoTracking()
                .AsSplitQuery() // <-- Thêm dòng này để tối ưu
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
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
                // 1. Tải sản phẩm gốc (existingProduct) từ Database
                var existingProduct = await _context.Product
                    .FirstOrDefaultAsync(p => p.Id == productToUpdate.Id);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Không tìm thấy Product {ProductId} để cập nhật", productToUpdate.Id);
                    return null; // Sửa: Trả về null nếu không tìm thấy
                }

                // 2. Cập nhật thủ công TỪNG TRƯỜNG CƠ BẢN VÀ FOREIGN KEY
                existingProduct.CategoryId = productToUpdate.CategoryId;
                existingProduct.ManufacturerId = productToUpdate.ManufacturerId;
                existingProduct.BrandId = productToUpdate.BrandId;
                existingProduct.Name = productToUpdate.Name;
                existingProduct.Description = productToUpdate.Description;
                existingProduct.Image = productToUpdate.Image;
                existingProduct.WarrantyPeriod = productToUpdate.WarrantyPeriod;
                existingProduct.IsActive = productToUpdate.IsActive;
                existingProduct.IsFeatured = productToUpdate.IsFeatured;

                // Cập nhật thời gian
                if (existingProduct is BaseModel baseModel)
                {
                    baseModel.UpdatedAt = DateTime.UtcNow;
                }

                // 3. SaveChangesAsync
                await _context.SaveChangesAsync();

                // 4. Sửa: Trả về đối tượng đã được cập nhật
                return existingProduct;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi concurrency (tranh chấp) khi cập nhật Product {ProductId}", productToUpdate.Id);
                return null; // Sửa: Trả về null khi có lỗi
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi cập nhật Product {ProductId}", productToUpdate.Id);
                return null; // Sửa: Trả về null khi có lỗi
            }
        }

        // <summary>
        /// [Admin] Thêm một biến thể mới cho sản phẩm.
        /// </summary>
        public async Task<ProductVariant> AddProductVariantAsync(ProductVariant newVariant)
        {
            try
            {
                // Logic nghiệp vụ (như kiểm tra ProductId có tồn tại không) nên ở Service
                // DAO chỉ thực hiện thêm.

                // Set thời gian và trạng thái mặc định
                if (newVariant is BaseModel baseModel)
                {
                    baseModel.CreatedAt = DateTime.UtcNow;
                    baseModel.UpdatedAt = DateTime.UtcNow;
                }
                // Đảm bảo biến thể mới luôn active khi tạo
                newVariant.IsActive = true;

                await _context.ProductVariant.AddAsync(newVariant);
                await _context.SaveChangesAsync();
                return newVariant; // Trả về variant đã có Id
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm mới ProductVariant cho Product {ProductId}", newVariant.ProductId);
                // Ném lại lỗi để Service xử lý
                throw;
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
        public async Task<bool> DeleteProductAsync(int productId)
        {
            // 1. Tìm sản phẩm VÀ các biến thể liên quan trong 1 truy vấn
            var product = await _context.Product
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(a => a.Id == productId);

            if (product == null)
            {
                return false;
            }

            // 2. Đánh dấu xóa mềm
            product.IsActive = false;

            // 3. Xóa mềm tất cả các biến thể của nó
            foreach (var variant in product.ProductVariants)
            {
                variant.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return true; // Tìm thấy và đã xử lý -> trả về true
        }

        public async Task<List<ProductVariant>> GetProductVariantDefault()
        {
            return await _context.ProductVariant.Where(p => p.IsDefault == true && p.IsActive == true).ToListAsync();
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

                // 2. Cập nhật thủ công các trường
                // Không cập nhật ProductId hoặc Id
                existingVariant.Name = variantToUpdate.Name;
                existingVariant.Price = variantToUpdate.Price;
                existingVariant.CostPrice = variantToUpdate.CostPrice;
                existingVariant.Weight = variantToUpdate.Weight;
                existingVariant.SKU = variantToUpdate.SKU;
                existingVariant.StockQuantity = variantToUpdate.StockQuantity;
                existingVariant.HoldQuantity = variantToUpdate.HoldQuantity; // Logic Hold nên ở Service, nhưng DAO cho phép cập nhật
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
    }
}