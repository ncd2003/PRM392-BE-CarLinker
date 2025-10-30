using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Cart;
using DataAccess;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using System.Security.Claims;
using TheVehicleEcosystemAPI.Response.DTOs;

namespace TheVehicleEcosystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartRepository cartRepository, ILogger<CartController> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        //Get list cart items
        [HttpGet("get-list-cart-items")]
        public async Task<IActionResult> GetListCartItems()
        {
            try
            {
                int UserId = 16;
                var cartId = await _cartRepository.GetCartIdByUserId(UserId);
                var listCartItem = await _cartRepository.GetListCartItemByCartId(cartId);

                // Map sang DTO
                var listCartItemDto = listCartItem.Adapt<List<CartItemDto>>();
                return Ok(ApiResponse<object>.Success("lấy sản phẩm trong giỏ hàng thành công", listCartItemDto));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm trong giỏ hàng");
                return StatusCode(500, ApiResponse<string>.InternalError(
                    "Đã xảy ra lỗi khi lấy sản phẩm trong giỏ hàng."
                ));
            }
        }

        //Add product to cart
        [HttpPost("Add-product-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddProductVariantDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ"));
            }

            try
            {
                int userId = 16; 

                var cartItem = await _cartRepository.AddProductToCart(productDto, userId);

                // Map sang DTO
                var cartItemDto = cartItem.Adapt<CartItemDto>();

                return Ok(ApiResponse<CartItemDto>.Success("Thêm sản phẩm vào giỏ hàng thành công", cartItemDto));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm vào giỏ hàng.");
                return StatusCode(500, ApiResponse<string>.InternalError(
                    "Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng."
                ));
            }
        }

        //Update cart item quantity
        [HttpPut("update-quantity-item")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto itemDto) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ."));
            }

            try
            {
                //var userId = GetUserId();
                int userId = 16;

                // Giả định: Repository có hàm này, gọi xuống DAO
                var cartItem = await _cartRepository.UpdateCartItemQuantity(userId, itemDto.ProductVariantId, itemDto.NewQuantity);
                // Map sang DTO
                var cartItemDto = cartItem.Adapt<CartItemDto>();
                //var cartId = await _cartRepository.GetCartIdByUserId(16);

                //var updatedCart = await _cartRepository.GetListCartItemByCartId(cartId);


                return Ok(ApiResponse<object>.Success("Cập nhật số lượng sản phẩm thành công", cartItemDto));
            }
            catch (Exception ex) // Bắt lỗi "Hết hàng" hoặc "Sản phẩm không tìm thấy"
            {
                _logger.LogWarning(ex, "Lỗi nghiệp vụ khi cập nhật giỏ hàng: {Message}", ex.Message);
                return BadRequest(ApiResponse<string>.BadRequest(ex.Message));
            }
        }

        // Remove item from cart
        [HttpDelete("remove-item/{productVariantId}")]
        public async Task<IActionResult> RemoveFromCart(int productVariantId)
        {
            try
            {
                //var userId = GetUserId();
                // Giả định repository có hàm này
                await _cartRepository.RemoveItemFromCart(16, productVariantId);

                var cartId = await _cartRepository.GetCartIdByUserId(16);

                // Trả về giỏ hàng mới nhất
                var updatedCart = await _cartRepository.GetListCartItemByCartId(cartId);
                var listCartItemDto = updatedCart.Adapt<List<CartItemDto>>();

                return Ok(ApiResponse<object>.Success("Cập nhật số lượng sản phẩm thành công", listCartItemDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm khỏi giỏ hàng.");
                return BadRequest(ApiResponse<string>.BadRequest(ex.Message)); // "Sản phẩm không tìm thấy"
            }
        }


        /// <summary>
        /// Helper: Lấy UserId từ Claims của token.
        /// </summary>
        private int GetUserId()
        {
            // Tìm Claim "NameIdentifier" (thường chứa UserId)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                // Lỗi này không nên xảy ra nếu [Authorize] được dùng đúng
                throw new UnauthorizedAccessException("Không thể xác định người dùng. Token không hợp lệ.");
            }

            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }

            // Lỗi này chỉ ra token bị cấu hình sai
            throw new BadHttpRequestException("Dữ liệu token người dùng (UserId) không hợp lệ.");
        }


    }
}
