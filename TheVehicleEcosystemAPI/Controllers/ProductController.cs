using BusinessObjects.Models.DTOs.Order;
using BusinessObjects.Models.DTOs.Product;
using DataAccess;
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
    }
}
