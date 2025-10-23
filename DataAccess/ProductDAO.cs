using BusinessObjects;
using BusinessObjects.Models;
using BusinessObjects.Models.DTOs; // Giả định bạn có DTOs cho việc lọc và tạo/cập nhật
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
                .Include(p => p.ProductVariants.Where(v => v.IsDefault && v.IsActive))
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
        /// [Admin] Cập nhật thông tin cơ bản của sản phẩm (tên, mô tả...).
        /// </summary>
        public async Task<bool> UpdateProductAsync(Product productToUpdate)
        {
            // Chỉ cập nhật các trường cơ bản, không cập nhật list (variants, options)
            _context.Entry(productToUpdate).State = EntityState.Modified;
            _context.Entry(productToUpdate).Property(p => p.Id).IsModified = false;
            // ... (có thể cần detach các navigation properties)

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi concurrency khi cập nhật Product {ProductId}", productToUpdate.Id);
                return false;
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
            var product = await _context.Product.FindAsync(productId);
            if (product == null)
            {
                throw new Exception($"Không tìm thấy sản phẩm với ID: {productId}");
            }

            product.IsActive = false; // Soft delete

            // Xóa mềm tất cả các biến thể của nó
            var variants = await _context.ProductVariant.Where(v => v.ProductId == productId).ToListAsync();
            foreach (var variant in variants)
            {
                variant.IsActive = false;
            }

            _context.Product.Update(product);
            _context.ProductVariant.UpdateRange(variants);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}