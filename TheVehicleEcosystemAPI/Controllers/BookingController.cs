using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Booking;
using BusinessObjects.Models.DTOs.ServiceItem;
using BusinessObjects.Models.Type;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using TheVehicleEcosystemAPI.Response.DTOs;
using TheVehicleEcosystemAPI.Utils;

namespace TheVehicleEcosystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class BookingController : ControllerBase
    {
        private readonly IServiceRecordRepository _serviceRecordRepository;
        private readonly IGarageRepository _garageRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            IServiceRecordRepository serviceRecordRepository,
            IGarageRepository garageRepository,
            IVehicleRepository vehicleRepository,
            ILogger<BookingController> logger)
        {
            _serviceRecordRepository = serviceRecordRepository;
            _garageRepository = garageRepository;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        /// <summary>
        /// UC-01: Create new booking (service record)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<BookingResponseDto>>> CreateBooking(
            [FromBody] CreateBookingRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                var userId = UserContextHelper.GetUserId(User);
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("User không hợp lệ"));
                }

                // PRE-2: Check garage active + accepting bookings
                var garage = await _garageRepository.GetByIdAsync(request.GarageId);
                if (garage == null || !garage.IsActive)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Garage không tồn tại hoặc đã bị vô hiệu hóa"));
                }

                if (!garage.AcceptingBookings)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Garage hiện không nhận đặt lịch"));
                }

                // PRE-1: Verify vehicle ownership
                var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
                if (vehicle == null || vehicle.UserId != userId.Value)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Phương tiện không hợp lệ hoặc không thuộc về bạn"));
                }

                // PRE-3 & PRE-4: Validate service items and check slot availability
                var garageServiceItems = await _garageRepository.GetServiceItemsByGarageIdAsync(request.GarageId);
                var requestedServiceItems = garageServiceItems
                    .Where(si => request.ServiceItemIds.Contains(si.Id))
                    .ToList();

                if (requestedServiceItems.Count != request.ServiceItemIds.Count)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Một hoặc nhiều dịch vụ không khả dụng tại garage này"));
                }

                // BR5: Time validation - cannot book past times
                if (request.StartTime < DateTime.UtcNow)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Không thể đặt lịch cho thời gian trong quá khứ"));
                }

                // BR2: Check slot availability (simplified - check if same time already exists)
                var existingBookings = await _garageRepository.GetBookingsByGarageIdAndDateAsync(
                    request.GarageId,
                    request.StartTime);

                var conflictingBooking = existingBookings.FirstOrDefault(b => 
                    Math.Abs((b.StartTime - request.StartTime).TotalMinutes) < 30);

                if (conflictingBooking != null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Thời gian đã được đặt. Vui lòng chọn thời gian khác"));
                }

                // BR4: Calculate total cost
                decimal totalCost = requestedServiceItems.Sum(si => si.Price);

                // Create ServiceRecord with cloned ServiceItems
                var serviceRecord = new ServiceRecord
                {
                    UserId = userId.Value,
                    VehicleId = request.VehicleId,
                    GarageId = request.GarageId,
                    StartTime = request.StartTime,
                    ServiceRecordStatus = ServiceRecordStatus.PENDING,
                    TotalCost = totalCost,
                    ServiceItems = requestedServiceItems.Select(si => new ServiceItem
                    {
                        Name = si.Name,
                        Price = si.Price,
                        ServiceCategoryId = si.ServiceCategoryId,
                        IsActive = true
                    }).ToList()
                };

                await _serviceRecordRepository.AddAsync(serviceRecord);

                // POST-2: TODO - Send notifications (implement notification service later)
                _logger.LogInformation("Booking created successfully for User {UserId}, Garage {GarageId}", 
                    userId.Value, request.GarageId);

                var response = new BookingResponseDto
                {
                    Id = serviceRecord.Id,
                    Status = serviceRecord.ServiceRecordStatus,
                    TotalCost = totalCost,
                    StartTime = serviceRecord.StartTime,
                    UserId = userId.Value,
                    CreatedAt = serviceRecord.CreatedAt?.DateTime ?? DateTime.UtcNow,
                    UpdatedAt = serviceRecord.UpdatedAt?.DateTime ?? DateTime.UtcNow
                };

                return Created($"/api/booking/{serviceRecord.Id}", 
                    ApiResponse<BookingResponseDto>.Created("Đặt lịch thành công", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi tạo booking"));
            }
        }

        /// <summary>
        /// Get all service items for a specific garage
        /// </summary>
        [HttpGet("garages/{garageId}/service-items")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<ServiceItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<ServiceItemDto>>>> GetGarageServiceItems(int garageId)
        {
            try
            {
                var garage = await _garageRepository.GetByIdAsync(garageId);
                if (garage == null)
                {
                    return NotFound(ApiResponse<object>.NotFound("Garage không tồn tại"));
                }

                var serviceItems = await _garageRepository.GetServiceItemsByGarageIdAsync(garageId);
                var serviceItemDtos = serviceItems.Select(si => si.Adapt<ServiceItemDto>()).ToList();

                return Ok(ApiResponse<List<ServiceItemDto>>.Success(
                    "Lấy danh sách dịch vụ thành công", serviceItemDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service items for garage {GarageId}", garageId);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi lấy danh sách dịch vụ"));
            }
        }

        /// <summary>
        /// Get available time slots for a garage on a specific date
        /// </summary>
        [HttpGet("garages/{garageId}/slots")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<TimeSlotDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<TimeSlotDto>>>> GetAvailableSlots(
            int garageId,
            [FromQuery] DateTime date)
        {
            try
            {
                var garage = await _garageRepository.GetByIdAsync(garageId);
                if (garage == null)
                {
                    return NotFound(ApiResponse<object>.NotFound("Garage không tồn tại"));
                }

                // Get existing bookings for the date
                var existingBookings = await _garageRepository.GetBookingsByGarageIdAndDateAsync(garageId, date);

                // Generate time slots (8 AM to 6 PM, hourly)
                var slots = new List<TimeSlotDto>();
                var targetDate = date.Date;

                for (int hour = 8; hour < 18; hour++)
                {
                    var slotTime = targetDate.AddHours(hour);
                    
                    // Check if slot is available (no booking within 30 minutes)
                    var hasConflict = existingBookings.Any(b => 
                        Math.Abs((b.StartTime - slotTime).TotalMinutes) < 30);

                    // Check if time is in the past
                    var isPast = slotTime < DateTime.UtcNow;

                    slots.Add(new TimeSlotDto
                    {
                        Time = slotTime,
                        IsAvailable = !hasConflict && !isPast && garage.AcceptingBookings,
                        Reason = isPast ? "Đã qua" : 
                                hasConflict ? "Đã được đặt" :
                                !garage.AcceptingBookings ? "Garage không nhận đặt lịch" : 
                                string.Empty
                    });
                }

                return Ok(ApiResponse<List<TimeSlotDto>>.Success(
                    "Lấy danh sách khung giờ thành công", slots));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting time slots for garage {GarageId}", garageId);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi lấy khung giờ"));
            }
        }

        /// <summary>
        /// Get user's bookings
        /// </summary>
        [HttpGet("my-bookings")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedData<BookingResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedData<BookingResponseDto>>>> GetMyBookings(
            [FromQuery] int page = 1,
            [FromQuery] int size = 30,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isAsc = true)
        {
            try
            {
                var userId = UserContextHelper.GetUserId(User);
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("User không hợp lệ"));
                }

                var (items, total) = await _serviceRecordRepository.GetAllByUserIdAsync(
                    userId.Value, page, size, sortBy, isAsc);

                var bookingDtos = items.Select(sr => sr.Adapt<BookingResponseDto>());
                var paginatedData = new PaginatedData<BookingResponseDto>(bookingDtos, total, page, size);

                return Ok(ApiResponse<PaginatedData<BookingResponseDto>>.Success(
                    "Lấy danh sách booking thành công", paginatedData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookings");
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi lấy danh sách booking"));
            }
        }

        /// <summary>
        /// Staff accept booking and set estimated end time
        /// </summary>
        [HttpPatch("{id}/accept")]
        [Authorize(Roles = "STAFF,OWNER")]
        [ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<BookingResponseDto>>> AcceptBooking(
            int id,
            [FromBody] AcceptBookingDto acceptDto)
        {
            try
            {
                var booking = await _serviceRecordRepository.GetByIdAsync(id);
                if (booking == null)
                {
                    return NotFound(ApiResponse<object>.NotFound("Booking không tồn tại"));
                }

                if (booking.ServiceRecordStatus != ServiceRecordStatus.PENDING)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Chỉ có thể chấp nhận booking đang ở trạng thái PENDING"));
                }

                booking.ServiceRecordStatus = ServiceRecordStatus.IN_PROGRESS;
                booking.EndTime = acceptDto.EstimatedEndTime;

                await _serviceRecordRepository.UpdateAsync(booking);

                var response = booking.Adapt<BookingResponseDto>();
                return Ok(ApiResponse<BookingResponseDto>.Success("Chấp nhận booking thành công", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting booking {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi chấp nhận booking"));
            }
        }

        /// <summary>
        /// Cancel booking
        /// </summary>
        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<BookingResponseDto>>> CancelBooking(int id)
        {
            try
            {
                var userId = UserContextHelper.GetUserId(User);
                var booking = await _serviceRecordRepository.GetByIdAsync(id);
                
                if (booking == null)
                {
                    return NotFound(ApiResponse<object>.NotFound("Booking không tồn tại"));
                }

                if (booking.UserId != userId)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Bạn không có quyền hủy booking này"));
                }

                if (booking.ServiceRecordStatus == ServiceRecordStatus.COMPLETED || 
                    booking.ServiceRecordStatus == ServiceRecordStatus.CANCELLED)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Không thể hủy booking đã hoàn thành hoặc đã hủy"));
                }

                booking.ServiceRecordStatus = ServiceRecordStatus.CANCELLED;
                await _serviceRecordRepository.UpdateAsync(booking);

                var response = booking.Adapt<BookingResponseDto>();
                return Ok(ApiResponse<BookingResponseDto>.Success("Hủy booking thành công", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling booking {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi hủy booking"));
            }
        }
    }
}
