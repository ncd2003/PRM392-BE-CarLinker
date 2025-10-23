using BusinessObjects.Models.DTOs;
using BusinessObjects.Models.DTOs.Order;
using BusinessObjects.Models.DTOs.Product;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using TheVehicleEcosystemAPI.Response.DTOs;

namespace TheVehicleEcosystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductRepository ProductRepository, ILogger<ProductController> logger)
        {
            _productRepository = ProductRepository;
            _logger = logger;
        }

        //Get products with filter params
        [HttpGet("product-list")]
        public async Task<IActionResult> GetListProduct([FromQuery] ProductFilterParams productFilterParams)
        {
            try
            {
                //var userId = GetUserId();
                var listProduct = await _productRepository.GetProductsAsync(productFilterParams);

                // Map sang DTO
                var listProductDto = listProduct.Adapt<List<ListProductVariantDto>>();

                return Ok(ApiResponse<List<ListProductVariantDto>>.Success("Lấy list sản phẩm thành công", listProductDto));
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

                // 3. Map từ entity sang DTO (Giả định bạn có lớp ProductDetailDto)
                var productDetailDto = product.Adapt<ProductDetailDto>();

                // 4. Trả về kết quả thành công
                return Ok(ApiResponse<ProductDetailDto>.Success("Lấy chi tiết sản phẩm thành công", productDetailDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy chi tiết sản phẩm ID: {productId}");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }
    }
}
