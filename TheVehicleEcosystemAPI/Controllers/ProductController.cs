using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Order;
using BusinessObjects.Models.DTOs.Product;
using DataAccess;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using TheVehicleEcosystemAPI.Response.DTOs;
using TheVehicleEcosystemAPI.Utils;

namespace TheVehicleEcosystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductController> _logger;
        private readonly CloudflareR2Storage _r2Storage;

        public ProductController(IProductRepository ProductRepository, ILogger<ProductController> logger, CloudflareR2Storage r2Storage)
        {
            _productRepository = ProductRepository;
            _logger = logger;
            _r2Storage = r2Storage;
        }

        //Get products with filter params
        [HttpGet("product-list")]
        public async Task<IActionResult> GetListProduct([FromQuery] ProductFilterParams productFilterParams)
        {
            try
            {
                //var userId = GetUserId();
                var listProduct = await _productRepository.GetProductsAsync(productFilterParams);
                var productVariantDefaults = await _productRepository.GetProductVariantDefault();

                // Map sang DTO
                var listProductDto = listProduct.Adapt<List<ListProductDto>>();

                foreach (var productDto in listProductDto)
                {
                    var defaultVariant = productVariantDefaults
                        .FirstOrDefault(v => v.ProductId == productDto.Id && v.IsDefault);

                    if (defaultVariant != null)
                    {
                        productDto.Price = defaultVariant.Price;
                    }
                }


                return Ok(ApiResponse<List<ListProductDto>>.Success("Lấy list sản phẩm thành công", listProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy list sản phẩm");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Lấy chi tiết một sản phẩm theo ID.
        /// </summary>
        [HttpGet("{productId:int}")]
        public async Task<IActionResult> GetProductDetailsAsync(int productId)
        {
            try
            {
                // 1. Gọi repository để lấy dữ liệu product entity
                var product = await _productRepository.GetProductDetailsAsync(productId);

                // 2. Kiểm tra sản phẩm có tồn tại không
                if (product == null)
                {
                    _logger.LogWarning($"Không tìm thấy sản phẩm với ID: {productId}");
                    return NotFound(ApiResponse<string>.NotFound($"Không tìm thấy sản phẩm với ID: {productId}"));
                }
                var productVariantDefaults = await _productRepository.GetProductVariantDefault();

                // 3. Map từ entity sang DTO (Giả định bạn có lớp ProductDetailDto)
                var productDetailDto = product.Adapt<ProductDetailDto>();
                var defaultVariant = productVariantDefaults
                        .FirstOrDefault(v => v.ProductId == productDetailDto.Id && v.IsDefault);
                if (defaultVariant != null)
                {
                    productDetailDto.Price = defaultVariant.Price;
                }


                // 4. Trả về kết quả thành công
                return Ok(ApiResponse<ProductDetailDto>.Success("Lấy chi tiết sản phẩm thành công", productDetailDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy chi tiết sản phẩm ID: {productId}");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo tên.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProductsAsync([FromQuery] string searchTerm)
        {
            try
            {
                // 1. Kiểm tra xem searchTerm có rỗng không
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    // Trả về danh sách rỗng nếu không có từ khóa tìm kiếm
                    return Ok(ApiResponse<List<ListProductDto>>.Success("Tìm kiếm thành công", new List<ListProductDto>()));
                }

                // 2. Gọi repository để tìm kiếm
                var listProduct = await _productRepository.SearchProductsAsync(searchTerm);

                // 3. Map sang DTO (Sử dụng DTO giống như GetListProduct)
                var listProductDto = listProduct.Adapt<List<ListProductDto>>();

                // 4. Trả về kết quả
                return Ok(ApiResponse<List<ListProductDto>>.Success("Tìm kiếm sản phẩm thành công", listProductDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tìm kiếm sản phẩm với từ khóa: {searchTerm}");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Thêm sản phẩm mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddProductAsync([FromForm] CreateProductDto createProductDto, IFormFile imageFile)
        {
            try
            {
                // 2. Validate dữ liệu đầu vào
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ"));
                }

                // 3. Xử lý upload ảnh (tương tự CreateVehicle)
                string? imageUrl = null;
                if (imageFile != null)
                {
                    try
                    {
                        // Upload vào thư mục "products" (hoặc thư mục bạn muốn)
                        imageUrl = await _r2Storage.UploadImageAsync(imageFile, "products");
                        // Giả sử createProductDto có thuộc tính Name để log
                        _logger.LogInformation("Image uploaded successfully to R2 for product {ProductName}", createProductDto.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload image to R2 for product");
                        return BadRequest(ApiResponse<string>.BadRequest("Lỗi khi tải ảnh lên. Vui lòng thử lại."));
                    }
                }

                // 4. Map từ DTO sang entity
                var product = createProductDto.Adapt<Product>();

                // 5. Gán link ảnh đã upload vào entity
                // (Giả sử Product entity có thuộc tính ImageUrl)
                product.Image = imageUrl ?? string.Empty;

                // 6. Gọi repository để thêm sản phẩm
                var addedProduct = await _productRepository.AddProductAsync(product);

                // 7. Map kết quả sang DTO để trả về
                var productDto = addedProduct.Adapt<CreateProductDto>();

                // 8. Trả về kết quả thành công
                return Ok(ApiResponse<CreateProductDto>.Success("Thêm sản phẩm thành công", productDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm mới");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Cập nhật thông tin sản phẩm.
        /// </summary>
        [HttpPut("{productId:int}")]
        public async Task<IActionResult> UpdateProductAsync(
            int productId,
            [FromForm] UpdateProductDto updateProductDto,
            IFormFile? imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ."));

                var existingProduct = await _productRepository.GetProductDetailsAsync(productId);
                if (existingProduct == null)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm ID: {ProductId}", productId);
                    return NotFound(ApiResponse<string>.NotFound($"Không tìm thấy sản phẩm ID: {productId}"));
                }

                string? imageUrl = existingProduct.Image; // giữ ảnh cũ mặc định
                if (imageFile != null)
                {
                    try
                    {
                        // Upload ảnh mới lên Cloudflare R2
                        imageUrl = await _r2Storage.UploadImageAsync(imageFile, "products");
                        _logger.LogInformation("Uploaded new image for product {ProductName}", updateProductDto.Name);

                        // (Tùy chọn) Xóa ảnh cũ nếu bạn muốn
                        // await _r2Storage.DeleteImageAsync(existingProduct.Image);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi upload ảnh mới cho sản phẩm {ProductId}", productId);
                        return BadRequest(ApiResponse<string>.BadRequest("Lỗi khi tải ảnh mới lên. Vui lòng thử lại."));
                    }
                }

                // 4️⃣ Cập nhật dữ liệu cho entity
                // Sử dụng Mapster để map DTO -> entity (giữ ID cũ)
                updateProductDto.Adapt(existingProduct);
                existingProduct.Image = imageUrl ?? string.Empty;

                // 5️⃣ Gọi repository để cập nhật
                var updatedProduct = await _productRepository.UpdateProductAsync(existingProduct);
                if (updatedProduct == null)
                {
                    return StatusCode(500, ApiResponse<string>.InternalError("Không thể cập nhật sản phẩm."));
                }

                // 6️⃣ Map sang DTO để trả về
                var productDto = updatedProduct.Adapt<UpdateProductDto>();

                // 7️⃣ Log và trả về
                _logger.LogInformation("Cập nhật sản phẩm thành công. ID: {ProductId}", productId);
                return Ok(ApiResponse<UpdateProductDto>.Success("Cập nhật sản phẩm thành công", productDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm ID: {ProductId}", productId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> DeleteProductAsync(int productId)
        {
            try
            {
                // 1. Gọi repository
                var success = await _productRepository.DeleteProductAsync(productId);

                // 2. Xử lý nếu repository trả về false (không tìm thấy)
                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm ID: {ProductId} để xóa.", productId);
                    return NotFound(ApiResponse<string>.NotFound($"Không tìm thấy sản phẩm ID: {productId}"));
                }

                // 3. Trả về thành công
                _logger.LogInformation("Xóa mềm sản phẩm thành công. ID: {ProductId}", productId);
                // Bạn có thể trả về Ok() hoặc NoContent()
                // Dùng Ok() để nhất quán với format ApiResponse
                return Ok(ApiResponse<string>.Success("Xóa sản phẩm thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm ID: {ProductId}", productId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Thêm một biến thể mới cho sản phẩm (ví dụ: thêm màu mới, size mới).
        /// </summary>
        /// <param name="productId">ID của sản phẩm cha</param>
        /// <param name="createVariantDto">Thông tin của biến thể mới</param>
        [HttpPost("{productId:int}/variants")]
        public async Task<IActionResult> AddProductVariantAsync(int productId, [FromBody] CreateProductVariantDto createVariantDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ."));
                }

                // 1. Kiểm tra xem sản phẩm (cha) có tồn tại không
                var parentProduct = await _productRepository.GetProductDetailsAsync(productId);
                if (parentProduct == null)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm (parent) ID: {ProductId} để thêm biến thể.", productId);
                    return NotFound(ApiResponse<string>.NotFound($"Không tìm thấy sản phẩm với ID: {productId} để thêm biến thể."));
                }

                // 2. Map từ DTO (CreateProductVariantDto) sang Entity (ProductVariant)
                var newVariant = createVariantDto.Adapt<ProductVariant>();

                // 3. Gán ID của sản phẩm cha cho biến thể mới
                newVariant.ProductId = productId;

                // 4. Gọi repository để thêm vào DB
                var addedVariant = await _productRepository.AddProductVariantAsync(newVariant);

                // 5. Map kết quả (đã có Id) sang DTO để trả về cho client
                var variantDto = addedVariant.Adapt<ProductVariantDto>();

                // 6. Trả về kết quả thành công
                _logger.LogInformation("Đã thêm biến thể {VariantId} cho sản phẩm {ProductId}", addedVariant.Id, productId);
                return Ok(ApiResponse<ProductVariantDto>.Success("Thêm biến thể sản phẩm thành công", variantDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm biến thể cho sản phẩm ID: {ProductId}", productId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

    }
}
