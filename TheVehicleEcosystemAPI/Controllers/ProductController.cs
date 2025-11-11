using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Order;
using BusinessObjects.Models.DTOs.Product;
using BusinessObjects.Models.DTOs.Product.OptionValue;
using BusinessObjects.Models.DTOs.Product.ProductOption;
using BusinessObjects.Models.DTOs.Product.ProductVariant;
using DataAccess;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using TheVehicleEcosystemAPI.Response.DTOs;
using TheVehicleEcosystemAPI.Utils;
using static System.Net.Mime.MediaTypeNames;

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
        public async Task<IActionResult> AddProductAsync([FromForm] CreateProductDto createProductDto, List<IFormFile> imageFiles)
        {
            try
            {
                // 2. Validate
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ"));
                }

                // 3. Xử lý upload nhiều ảnh
                var imageUrls = new List<string>(); // List để chứa các URL
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    foreach (var imageFile in imageFiles) // <-- Lặp qua danh sách
                    {
                        try
                        {
                            // Upload từng file
                            var imageUrl = await _r2Storage.UploadImageAsync(imageFile, "products");
                            imageUrls.Add(imageUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to upload one of the images to R2 for product {ProductName}", createProductDto.Name);
                            // Quyết định: dừng lại hay bỏ qua file lỗi và tiếp tục?
                            // Ở đây ví dụ là dừng lại:
                            return BadRequest(ApiResponse<string>.BadRequest($"Lỗi khi tải ảnh {imageFile.FileName}."));
                        }
                    }
                    _logger.LogInformation("Uploaded {Count} images for product {ProductName}", imageUrls.Count, createProductDto.Name);
                }

                // 4. Map từ DTO sang entity
                var product = createProductDto.Adapt<Product>();

                // 5. Gán CÁC link ảnh đã upload vào entity
                product.ProductImages = imageUrls.Select((url, index) => new ProductImage
                {
                    ImageUrl = url,
                    IsFeatured = (index == 0)
                }).ToList(); ;


                // 6. Gọi repository
                var addedProduct = await _productRepository.AddProductAsync(product);

                // 7. Map kết quả sang DTO
                var productDto = addedProduct.Adapt<CreateProductDto>(); // Cân nhắc dùng ProductDto (response) thay vì CreateProductDto

                // 8. Trả về
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
        public async Task<IActionResult> UpdateProductAsync(int productId, [FromForm] UpdateProductDto updateProductDto, List<IFormFile>? imageFiles)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ."));

                // 1. Lấy sản phẩm hiện có (Bao gồm cả ảnh cũ)
                // (Giả định GetProductDetailsAsync đã Include(p => p.ProductImages)
                // giống như trong ví dụ UpdateProductAsync của Repository)
                var existingProduct = await _productRepository.GetProductDetailsAsync(productId);
                if (existingProduct == null)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm ID: {ProductId} để cập nhật", productId);
                    return NotFound(ApiResponse<string>.NotFound($"Không tìm thấy sản phẩm ID: {productId}"));
                }

                // Lưu trữ link ảnh cũ để xóa khỏi R2 SAU KHI cập nhật DB thành công
                var oldImageUrls = existingProduct.ProductImages.Select(img => img.ImageUrl).ToList();
                var newImageUrls = new List<string>();
                bool hasNewImages = (imageFiles != null && imageFiles.Count > 0);

                // 2. Xử lý upload ảnh MỚI (nếu có)
                // (Logic tương tự AddProductAsync)
                if (hasNewImages)
                {
                    foreach (var imageFile in imageFiles!) // '!' vì đã check Count > 0
                    {
                        try
                        {
                            // Upload ảnh mới lên Cloudflare R2
                            var imageUrl = await _r2Storage.UploadImageAsync(imageFile, "products");
                            newImageUrls.Add(imageUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi upload ảnh mới {FileName} cho sản phẩm {ProductId}", imageFile.FileName, productId);
                            return BadRequest(ApiResponse<string>.BadRequest($"Lỗi khi tải ảnh mới {imageFile.FileName}. Vui lòng thử lại."));
                        }
                    }
                    _logger.LogInformation("Đã tải lên {Count} ảnh mới cho sản phẩm {ProductId}", newImageUrls.Count, productId);
                }

                // 3. Cập nhật dữ liệu cho entity
                // 3a. Map các trường cơ bản từ DTO (Tên, Mô tả, v.v.)
                updateProductDto.Adapt(existingProduct);

                // 3b. Xử lý logic thay thế ảnh (nếu có ảnh mới)
                // (Logic tương tự UpdateProductAsync của Repository)
                if (hasNewImages)
                {
                    // Xóa các liên kết ảnh cũ trong DB (báo cho EF Core biết)
                    existingProduct.ProductImages.Clear();

                    // Thêm các liên kết ảnh mới vào DB (logic từ AddProductAsync)
                    existingProduct.ProductImages = newImageUrls.Select((url, index) => new ProductImage
                    {
                        ImageUrl = url,
                        IsFeatured = (index == 0) // Đặt ảnh đầu tiên làm ảnh nổi bật
                    }).ToList();
                }
                // (Nếu không có ảnh mới, existingProduct.ProductImages không bị chạm vào, giữ nguyên ảnh cũ)

                // 4. Gọi repository để cập nhật
                // (Hàm này sẽ gọi SaveChangesAsync)
                var updatedProduct = await _productRepository.UpdateProductAsync(existingProduct);
                if (updatedProduct == null)
                {
                    // Lỗi này đã được log bên trong Repository, chỉ cần trả về
                    return StatusCode(500, ApiResponse<string>.InternalError("Không thể cập nhật sản phẩm."));
                }

                // 5. (Tùy chọn nhưng khuyến nghị) Xóa ảnh cũ khỏi R2 Storage
                // Chỉ xóa nếu: có ảnh mới upload LÊN và cập nhật DB thành CÔNG
                if (hasNewImages && oldImageUrls.Count > 0)
                {
                    _logger.LogInformation("Đang xóa {Count} ảnh cũ khỏi R2 cho sản phẩm {ProductId}", oldImageUrls.Count, productId);
                    foreach (var oldUrl in oldImageUrls)
                    {
                        if (string.IsNullOrEmpty(oldUrl)) continue;

                        try
                        {
                            // Không cần 'await' nếu bạn muốn nó chạy nền và không chặn response
                            // Hoặc 'await' nếu bạn muốn đảm bảo nó cố gắng xóa trước khi trả về
                            await _r2Storage.DeleteImageAsync(oldUrl);
                        }
                        catch (Exception ex)
                        {
                            // Ghi log lỗi xóa ảnh cũ, nhưng KHÔNG làm hỏng request
                            _logger.LogWarning(ex, "Lỗi khi xóa ảnh cũ {ImageUrl} khỏi R2", oldUrl);
                        }
                    }
                }

                // 6. Map sang DTO để trả về
                // (Giữ nguyên theo template của bạn)
                var productDto = updatedProduct.Adapt<UpdateProductDto>();

                // 7. Log và trả về
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

        [HttpPost("{productId:int}/variants")]
        public async Task<IActionResult> AddProductVariantAsync(int productId, [FromBody] CreateProductVariantDto createVariantDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Lấy lỗi validation
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<string>.BadRequest(string.Join(", ", errors)));
                }

                // 1. Map thông tin chính của biến thể từ DTO
                var newVariant = createVariantDto.Adapt<ProductVariant>();

                // 2. Gán ProductId từ URL
                newVariant.ProductId = productId;

                // 3. Gọi repository với CẢ HAI tham số
                var addedVariant = await _productRepository.AddProductVariantAsync(
                    newVariant,
                    createVariantDto.SelectedOptionValueIds // <-- Truyền danh sách ID
                );

                // 4. Kiểm tra lỗi (ví dụ: trùng lặp, productId không tồn tại...)
                if (addedVariant == null)
                {
                    _logger.LogWarning("Thêm biến thể thất bại cho ProductId {ProductId}. Kiểm tra log của DAO.", productId);
                    return BadRequest(ApiResponse<string>.BadRequest("Thêm biến thể thất bại. Có thể do sản phẩm không tồn tại hoặc tổ hợp thuộc tính đã tồn tại."));
                }

                // 5. Map kết quả sang DTO để trả về
                // (Bạn nên tạo một ProductVariantDetailDto để trả về bao gồm cả các option đã chọn)
                var variantDto = addedVariant.Adapt<ProductVariantDto>();

                _logger.LogInformation("Đã thêm biến thể {VariantId} cho sản phẩm {ProductId}", addedVariant.Id, productId);
                return Ok(ApiResponse<ProductVariantDto>.Success("Thêm biến thể sản phẩm thành công", variantDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm biến thể cho sản phẩm ID: {ProductId}", productId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Thêm một thuộc tính mới (ví dụ: Màu sắc, Dung lượng) cho một sản phẩm.
        /// </summary>
        /// <param name="productId">ID của sản phẩm cha</param>
        /// <param name="createDto">Thông tin của thuộc tính mới</param>
        [HttpPost("{productId:int}/options")]
        public async Task<IActionResult> AddProductOptionAsync(int productId, [FromBody] CreateProductOptionDto createDto)
        {
            try
            {
                // 1. Validate DTO
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ."));
                }

                // 2. Map từ DTO (Create) sang Entity (ProductOption)
                // (Sử dụng Mapster như bạn đang dùng)
                var productOption = createDto.Adapt<ProductOption>();

                // 3. Gán ID sản phẩm cha từ URL
                productOption.ProductId = productId;

                // 4. Gọi repository
                var addedOption = await _productRepository.AddProductOptionAsync(productOption);

                // 5. Xử lý kết quả từ repository
                // (Dựa trên logic chúng ta đã thảo luận, repo sẽ trả về null nếu
                // sản phẩm cha không tồn tại hoặc nếu tên thuộc tính bị trùng)
                if (addedOption == null)
                {
                    _logger.LogWarning("Thêm thuộc tính thất bại cho ProductId {ProductId}. Có thể do trùng tên hoặc sản phẩm không tồn tại.", productId);
                    return BadRequest(ApiResponse<string>.BadRequest("Thêm thuộc tính thất bại. Sản phẩm có thể không tồn tại hoặc tên thuộc tính đã được sử dụng."));
                }

                // 6. Map kết quả (đã có Id) sang DTO (Response)
                var optionDto = addedOption.Adapt<ProductOptionDto>();

                // 7. Trả về kết quả thành công
                _logger.LogInformation("Đã thêm thuộc tính {OptionId} cho sản phẩm {ProductId}", addedOption.Id, productId);
                return Ok(ApiResponse<ProductOptionDto>.Success("Thêm thuộc tính sản phẩm thành công", optionDto));
            }
            catch (Exception ex)
            {
                // Log lỗi chung
                _logger.LogError(ex, "Lỗi khi thêm thuộc tính cho sản phẩm ID: {ProductId}", productId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Thêm một giá trị mới (ví dụ: "Đỏ", "128GB") cho một thuộc tính.
        /// </summary>
        /// <param name="optionId">ID của thuộc tính cha (ví dụ: ID của "Màu sắc")</param>
        /// <param name="createDto">Thông tin giá trị mới (chỉ chứa "Value")</param>
        [HttpPost("options/{optionId:int}/values")] // -> Route: api/products/options/123/values
        public async Task<IActionResult> AddOptionValueAsync(int optionId, [FromBody] CreateOptionValueDto createDto)
        {
            try
            {
                // 1. Validate DTO
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ."));
                }

                // 2. Map từ DTO sang Entity
                var newOptionValue = createDto.Adapt<OptionValue>();

                // 3. Gán ID của thuộc tính cha (lấy từ URL)
                newOptionValue.OptionId = optionId;

                // 4. Gọi Repository để thêm
                // (Repository sẽ gọi DAO, và DAO nên kiểm tra xem OptionId có tồn tại không
                // và giá trị "Value" có bị trùng lặp cho cùng OptionId không)
                var addedValue = await _productRepository.AddOptionValueAsync(newOptionValue);

                // 5. Kiểm tra kết quả từ Repository/DAO
                if (addedValue == null)
                {
                    _logger.LogWarning("Thêm OptionValue thất bại cho OptionId {OptionId}. Có thể do Option không tồn tại hoặc giá trị bị trùng.", optionId);
                    return BadRequest(ApiResponse<string>.BadRequest("Thêm giá trị thất bại. Thuộc tính có thể không tồn tại hoặc giá trị này đã được thêm."));
                }

                // 6. Map sang DTO trả về
                var valueDto = addedValue.Adapt<OptionValueDto>();

                // 7. Trả về thành công
                _logger.LogInformation("Đã thêm OptionValue {ValueId} cho OptionId {OptionId}", addedValue.Id, optionId);
                return Ok(ApiResponse<OptionValueDto>.Success("Thêm giá trị thuộc tính thành công", valueDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm giá trị cho thuộc tính ID: {OptionId}", optionId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// [Admin] Cập nhật thông tin chi tiết của một biến thể (giá, tồn kho, SKU...).
        /// </summary>
        /// <param name="variantId">ID của biến thể cần cập nhật</param>
        /// <param name="updateDto">Dữ liệu cập nhật</param>
        [HttpPut("variants/{variantId:int}")] // -> Route: PUT api/products/variants/101
        public async Task<IActionResult> UpdateProductVariantAsync(int variantId, [FromBody] UpdateProductVariantDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ."));
            }

            try
            {
                // 1. Map DTO -> Entity
                var variantToUpdate = updateDto.Adapt<ProductVariant>();

                // 2. Gán ID từ URL
                variantToUpdate.Id = variantId;

                // 3. Gọi Repository
                // (DAO sẽ tự động fetch đối tượng gốc và cập nhật các trường)
                var updatedVariant = await _productRepository.UpdateProductVariantAsync(variantToUpdate);

                // 4. Kiểm tra
                if (updatedVariant == null)
                {
                    _logger.LogWarning("Cập nhật ProductVariant {VariantId} thất bại: Không tìm thấy hoặc lỗi.", variantId);
                    return NotFound(ApiResponse<string>.NotFound($"Không tìm thấy biến thể với ID: {variantId}"));
                }

                // 5. Trả về DTO
                var variantDto = updatedVariant.Adapt<ProductVariantDto>();
                return Ok(ApiResponse<ProductVariantDto>.Success("Cập nhật biến thể thành công", variantDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật biến thể ID: {VariantId}", variantId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// [Admin] Xóa mềm một biến thể (set IsActive = false).
        /// </summary>
        /// <param name="variantId">ID của biến thể cần xóa</param>
        [HttpDelete("variants/{variantId:int}")] // -> Route: DELETE api/products/variants/101
        public async Task<IActionResult> DeleteProductVariantAsync(int variantId)
        {
            try
            {
                var success = await _productRepository.DeleteProductVariantAsync(variantId);
                if (!success)
                {
                    _logger.LogWarning("Xóa mềm ProductVariant {VariantId} thất bại: Không tìm thấy.", variantId);
                    return NotFound(ApiResponse<string>.NotFound($"Không tìm thấy biến thể với ID: {variantId}"));
                }

                return Ok(ApiResponse<string>.Success("Xóa mềm biến thể thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mềm biến thể ID: {VariantId}", variantId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// [Admin] Cập nhật một thuộc tính (ví dụ: đổi tên "Màu sắc").
        /// </summary>
        /// <param name="optionId">ID của thuộc tính cần cập nhật</param>
        /// <param name="updateDto">Dữ liệu cập nhật</param>
        [HttpPut("options/{optionId:int}")] // -> Route: PUT api/products/options/1
        public async Task<IActionResult> UpdateProductOptionAsync(int optionId, [FromBody] UpdateProductOptionDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ."));
            }

            try
            {
                var optionToUpdate = updateDto.Adapt<ProductOption>();
                optionToUpdate.Id = optionId;

                var updatedOption = await _productRepository.UpdateProductOptionAsync(optionToUpdate);

                if (updatedOption == null)
                {
                    _logger.LogWarning("Cập nhật ProductOption {OptionId} thất bại: Không tìm thấy hoặc tên trùng.", optionId);
                    return NotFound(ApiResponse<string>.NotFound($"Không tìm thấy thuộc tính ID: {optionId} hoặc tên bị trùng."));
                }

                var optionDto = updatedOption.Adapt<ProductOptionDto>();
                return Ok(ApiResponse<ProductOptionDto>.Success("Cập nhật thuộc tính thành công", optionDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thuộc tính ID: {OptionId}", optionId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// [Admin] Xóa một thuộc tính (và các giá trị con của nó).
        /// </summary>
        /// <param name="optionId">ID của thuộc tính cần xóa</param>
        [HttpDelete("options/{optionId:int}")] // -> Route: DELETE api/products/options/1
        public async Task<IActionResult> DeleteProductOptionAsync(int optionId)
        {
            try
            {
                var success = await _productRepository.DeleteProductOptionAsync(optionId);
                if (!success)
                {
                    _logger.LogWarning("Xóa ProductOption {OptionId} thất bại: Không tìm thấy hoặc đang được sử dụng.", optionId);
                    return BadRequest(ApiResponse<string>.BadRequest("Không thể xóa thuộc tính. Không tìm thấy hoặc thuộc tính đang được một biến thể sử dụng."));
                }

                return Ok(ApiResponse<string>.Success("Xóa thuộc tính thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa thuộc tính ID: {OptionId}", optionId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// [Admin] Cập nhật một giá trị (ví dụ: đổi "Đo" thành "Đỏ").
        /// </summary>
        /// <param name="valueId">ID của giá trị cần cập nhật</param>
        /// <param name="updateDto">Dữ liệu cập nhật</param>
        [HttpPut("values/{valueId:int}")] // -> Route: PUT api/products/values/50
        public async Task<IActionResult> UpdateOptionValueAsync(int valueId, [FromBody] UpdateOptionValueDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ."));
            }

            try
            {
                var valueToUpdate = updateDto.Adapt<OptionValue>();
                valueToUpdate.Id = valueId;

                var updatedValue = await _productRepository.UpdateOptionValueAsync(valueToUpdate);

                if (updatedValue == null)
                {
                    _logger.LogWarning("Cập nhật OptionValue {ValueId} thất bại: Không tìm thấy hoặc giá trị bị trùng.", valueId);
                    return NotFound(ApiResponse<string>.NotFound($"Không tìm thấy giá trị ID: {valueId} hoặc giá trị bị trùng."));
                }

                var valueDto = updatedValue.Adapt<OptionValueDto>();
                return Ok(ApiResponse<OptionValueDto>.Success("Cập nhật giá trị thành công", valueDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật giá trị ID: {ValueId}", valueId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// [Admin] Xóa một giá trị (ví dụ: xóa màu "Đỏ").
        /// </summary>
        /// <param name="valueId">ID của giá trị cần xóa</param>
        [HttpDelete("values/{valueId:int}")] // -> Route: DELETE api/products/values/50
        public async Task<IActionResult> DeleteOptionValueAsync(int valueId)
        {
            try
            {
                var success = await _productRepository.DeleteOptionValueAsync(valueId);
                if (!success)
                {
                    _logger.LogWarning("Xóa OptionValue {ValueId} thất bại: Không tìm thấy hoặc đang được sử dụng.", valueId);
                    return BadRequest(ApiResponse<string>.BadRequest("Không thể xóa giá trị. Không tìm thấy hoặc giá trị đang được một biến thể sử dụng."));
                }

                return Ok(ApiResponse<string>.Success("Xóa giá trị thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa giá trị ID: {ValueId}", valueId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// [Admin] Lấy TẤT CẢ các biến thể (bao gồm cả ẩn) của một sản phẩm.
        /// </summary>
        /// <param name="productId">ID của sản phẩm cha</param>
        [HttpGet("{productId:int}/variants")] // -> Route: GET api/products/123/variants
        public async Task<IActionResult> GetAllVariantsForProductAsync(int productId)
        {
            try
            {
                // 1. Gọi repository (dùng hàm bạn vừa yêu cầu)
                var variants = await _productRepository.GetVariantsByProductIdAsync(productId);

                // 2. Map sang DTO
                // (Nếu 'variants' rỗng, 'variantDtos' cũng sẽ là list rỗng, đây là kết quả hợp lệ)
                var variantDtos = variants.Adapt<List<ProductVariantDto>>();

                // 3. Trả về
                return Ok(ApiResponse<List<ProductVariantDto>>.Success(
                    "Lấy danh sách biến thể thành công",
                    variantDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách biến thể cho sản phẩm ID: {ProductId}", productId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }
    }
}
