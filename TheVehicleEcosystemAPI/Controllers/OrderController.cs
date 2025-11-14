using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Order;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using TheVehicleEcosystemAPI.Response.DTOs;
using TheVehicleEcosystemAPI.Utils;

namespace TheVehicleEcosystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderRepository orderRepository, ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [Authorize(Roles = "CUSTOMER")]
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.BadRequest("Dữ liệu không hợp lệ"));
            }
            int UserId = UserContextHelper.GetUserId(User).Value;
            try
            {
                // 1. Lấy model Order gốc từ DAO
                Order newOrder = await _orderRepository.CreateOrderFromCart(UserId, orderDto);

                var orderResponse = newOrder.Adapt<OrderResponseDto>();

                // 3. Trả về DTO đã được map
                return Ok(ApiResponse<OrderResponseDto>.Success("Tạo đơn hàng thành công!", orderResponse));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi nghiệp vụ khi tạo đơn hàng cho UserID {UserId}: {Message}", UserId, ex.Message);
                return BadRequest(ApiResponse<string>.BadRequest(ex.Message));
            }
        }

        /// <summary>
        /// Lấy lịch sử đơn hàng của người dùng đang đăng nhập.
        /// </summary>
        [Authorize(Roles = "CUSTOMER")]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            int UserId = UserContextHelper.GetUserId(User).Value;
            try
            {
                var orders = await _orderRepository.GetOrdersByUserId(UserId);

                // Map sang DTO
                var orderDtos = orders.Adapt<List<OrderResponseDto>>();

                return Ok(ApiResponse<List<OrderResponseDto>>.Success("Lấy đơn hàng thành công", orderDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy đơn hàng cho UserID {UserId}", UserId);
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Lấy chi tiết của một đơn hàng.
        /// Chỉ chủ đơn hàng hoặc Admin mới có quyền xem.
        /// </summary>
        [Authorize(Roles = "CUSTOMER,DEALER")]
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

                int UserId = UserContextHelper.GetUserId(User).Value;

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
        [Authorize(Roles = "CUSTOMER")]
        [HttpPut("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                int UserId = UserContextHelper.GetUserId(User).Value;

                await _orderRepository.CancelOrderAsync(orderId, UserId);

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
        [Authorize(Roles = "DEALER")]
        [HttpGet("all-orders")]
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
        [Authorize(Roles = "DEALER")]
        [HttpPut("update-status")]
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

        /// <summary>
        /// [Admin] Lấy tổng số đơn hàng trong hệ thống.
        /// </summary>
        [HttpGet("statistics/total-count")]
        [Authorize(Roles = "DEALER")]
        public async Task<IActionResult> GetTotalOrderCount()
        {
            try
            {
                int count = await _orderRepository.GetTotalOrderCountAsync();
                return Ok(ApiResponse<int>.Success("Lấy tổng số đơn hàng thành công.", count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tổng số đơn hàng.");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// [Admin] Lấy số đơn hàng đang chờ xử lý (Pending).
        /// </summary>
        [HttpGet("statistics/pending-count")]
        [Authorize(Roles = "DEALER")]
        public async Task<IActionResult> GetPendingOrderCount()
        {
            try
            {
                int count = await _orderRepository.GetPendingOrderCountAsync();
                return Ok(ApiResponse<int>.Success("Lấy số đơn hàng PENDING thành công.", count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số đơn hàng PENDING.");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }


        /// <summary>
        /// [Admin] Lấy tổng doanh thu (từ các đơn đã DELIVERED).
        /// </summary>
        [HttpGet("statistics/total-revenue")]
        [Authorize(Roles = "DEALER")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            try
            {
                decimal revenue = await _orderRepository.GetTotalRevenueAsync();
                return Ok(ApiResponse<decimal>.Success("Lấy tổng doanh thu thành công.", revenue));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tổng doanh thu.");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }
    }
}



