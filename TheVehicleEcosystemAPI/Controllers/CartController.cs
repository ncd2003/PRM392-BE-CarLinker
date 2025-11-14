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
using TheVehicleEcosystemAPI.Utils;

namespace TheVehicleEcosystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        [Authorize(Roles = "CUSTOMER")]
        [HttpGet("get-list-cart-items")]
        public async Task<IActionResult> GetListCartItems()
        {
            try
            {
                int UserId = UserContextHelper.GetUserId(User).Value;
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
        [Authorize(Roles = "CUSTOMER")]
        [HttpPost("Add-product-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddProductVariantDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ"));
            }

            try
            {
                int UserId = UserContextHelper.GetUserId(User).Value;

                var cartItem = await _cartRepository.AddProductToCart(productDto, UserId);

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
        [Authorize(Roles = "CUSTOMER")]
        [HttpPut("update-quantity-item")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ."));
            }

            try
            {
                int UserId = UserContextHelper.GetUserId(User).Value;

                var cartItem = await _cartRepository.UpdateCartItemQuantity(UserId, itemDto.ProductVariantId, itemDto.NewQuantity);
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
        [Authorize(Roles = "CUSTOMER")]
        [HttpDelete("remove-item/{productVariantId}")]
        public async Task<IActionResult> RemoveFromCart(int productVariantId)
        {
            try
            {
                int UserId = UserContextHelper.GetUserId(User).Value;
                await _cartRepository.RemoveItemFromCart(UserId, productVariantId);

                var cartId = await _cartRepository.GetCartIdByUserId(UserId);

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

    }
}



