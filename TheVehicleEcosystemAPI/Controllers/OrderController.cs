using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Order;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using TheVehicleEcosystemAPI.Response.DTOs;

namespace TheVehicleEcosystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderRepository orderRepository, ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ"));
            }

            try
            {
                //var userId = GetUserId();

                // 1. Lấy model Order gốc từ DAO
                Order newOrder = await _orderRepository.CreateOrderFromCart(16, orderDto);

                // 2. DÙNG MAPSTER ĐỂ CHUYỂN ĐỔI
                // Tự động map Order -> OrderResponseDto và 
                // Order.OrderItems -> List<OrderItemDto>
                var orderResponse = newOrder.Adapt<OrderResponseDto>();

                // 3. Trả về DTO đã được map
                return Ok(ApiResponse<OrderResponseDto>.Success("Tạo đơn hàng thành công!", orderResponse));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi nghiệp vụ khi tạo đơn hàng cho UserID {UserId}: {Message}", 16, ex.Message);
                return BadRequest(ApiResponse<string>.BadRequest(ex.Message));
            }
        }

        /// <summary>
        /// Lấy lịch sử đơn hàng của người dùng đang đăng nhập.
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                //var userId = GetUserId();
                var orders = await _orderRepository.GetOrdersByUserId(16);

                // Map sang DTO
                var orderDtos = orders.Adapt<List<OrderResponseDto>>();

                return Ok(ApiResponse<List<OrderResponseDto>>.Success("Lấy đơn hàng thành công",orderDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy đơn hàng cho UserID {UserId}", 16);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Lấy chi tiết của một đơn hàng.
        /// Chỉ chủ đơn hàng hoặc Admin mới có quyền xem.
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetOrderDetail(orderId);

                if (order == null)
                {
                    return NotFound(ApiResponse<string>.NotFound("Không tìm thấy đơn hàng."));
                }

                // Kiểm tra quyền sở hữu
                //var userId = GetUserId();
                int userId = 16;
                if (order.UserId != userId && !User.IsInRole("Admin")) // Giả sử vai trò Admin là "Admin"
                {
                    return Forbid("Bạn không có quyền xem đơn hàng này.");
                }

                var orderDto = order.Adapt<OrderResponseDto>();
                return Ok(ApiResponse<OrderResponseDto>.Success("Lấy đơn hàng thành công", orderDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết đơn hàng {OrderId}", orderId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Khách hàng tự hủy đơn hàng (chỉ khi đơn ở trạng thái "Pending").
        /// </summary>
        [HttpPut("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                //var userId = GetUserId();
                int userId = 16;

                // Kiểm tra xem người gọi có phải Admin không
                int? userIdCheck = User.IsInRole("Admin") ? (int?)null : userId;

                await _orderRepository.CancelOrderAsync(orderId, userIdCheck);

                return Ok(ApiResponse<string>.Success("Hủy đơn hàng thành công. Hàng đã được hoàn lại kho."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex) // Bắt lỗi "Không thể hủy đơn hàng ở trạng thái này"
            {
                _logger.LogWarning(ex, "Lỗi nghiệp vụ khi hủy đơn hàng {OrderId}", orderId);
                return BadRequest(ApiResponse<string>.BadRequest(ex.Message));
            }
        }

        /// <summary>
        /// [Admin] Lấy tất cả đơn hàng trong hệ thống.
        /// </summary>
        [HttpGet("all-orders")]
        //[Authorize(Roles = "Admin")] // Chỉ Admin
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderRepository.GetAllOrders();
                var orderDtos = orders.Adapt<List<OrderResponseDto>>();
                return Ok(ApiResponse<List<OrderResponseDto>>.Success("Lấy đơn hàng thành công", orderDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Admin lấy tất cả đơn hàng");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// [Admin] Cập nhật trạng thái của một đơn hàng.
        /// </summary>
        [HttpPut("update-status")]
        //[Authorize(Roles = "Admin")] // Chỉ Admin
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateOrderStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ."));
            }

            try
            {
                await _orderRepository.UpdateOrderStatus(dto.OrderId, dto.NewStatus);
                return Ok(ApiResponse<string>.Success($"Cập nhật trạng thái đơn hàng {dto.OrderId} thành {dto.NewStatus} thành công."));
            }
            catch (Exception ex) // Bắt lỗi "Không tìm thấy đơn hàng"
            {
                _logger.LogError(ex, "Lỗi khi Admin cập nhật trạng thái đơn hàng {OrderId}", dto.OrderId);
                return BadRequest(ApiResponse<string>.BadRequest(ex.Message));
            }
        }
    }
}
