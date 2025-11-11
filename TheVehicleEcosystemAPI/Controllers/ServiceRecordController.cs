using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.ServiceRecord;
using BusinessObjects.Models.DTOs.ServiceRecord;
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
    public class ServiceRecordController : ControllerBase
    {
        private readonly IServiceRecordRepository _serviceRecordRepository;
        private readonly IServiceItemRepository _serviceItemRepository;  // ✅ THÊM
        private readonly ILogger<ServiceCategoryController> _logger;

        public ServiceRecordController(
            IServiceRecordRepository serviceRecordRepository,
            IServiceItemRepository serviceItemRepository,  // ✅ THÊM
            ILogger<ServiceCategoryController> logger)
        {
            _serviceRecordRepository = serviceRecordRepository;
            _serviceItemRepository = serviceItemRepository;  // ✅ THÊM
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedData<ServiceRecordDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedData<ServiceRecordDto>>>> GetAllServiceRecords(
           [FromQuery] int page = 1,
           [FromQuery] int size = 30,
           [FromQuery] string? sortBy = null,
           [FromQuery] bool isAsc = true)
        {
            try
            {
                var (items, total) = await _serviceRecordRepository.GetAllAsync(page, size, sortBy, isAsc);
                var serviceRecordDtos = items.Select(u => u.Adapt<ServiceRecordDto>());
                var paginatedData = new PaginatedData<ServiceRecordDto>(serviceRecordDtos, total, page, size);
                var response = ApiResponse<PaginatedData<ServiceRecordDto>>.Success(
                    "Lấy danh sách dịch vụ thành công",
                    paginatedData);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting serviceCategories");
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy danh sách dịch vụ");
                return StatusCode(500, response);
            }
        }

        [HttpGet("user")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedData<ServiceRecordDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedData<ServiceRecordDto>>>> GetAllServiceRecordsByUserId(
           [FromQuery] int page = 1,
           [FromQuery] int size = 30,
           [FromQuery] string? sortBy = null,
           [FromQuery] bool isAsc = true)
        {
            try
            {
                var (items, total) = await _serviceRecordRepository.GetAllByUserIdAsync(UserContextHelper.GetUserId(User).Value,page, size, sortBy, isAsc);
                var serviceRecordDtos = items.Select(u => u.Adapt<ServiceRecordDto>());

                var paginatedData = new PaginatedData<ServiceRecordDto>(serviceRecordDtos, total, page, size);
                var response = ApiResponse<PaginatedData<ServiceRecordDto>>.Success(
                    "Lấy danh sách dịch vụ thành công",
                    paginatedData);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting serviceCategories");
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy danh sách dịch vụ");
                return StatusCode(500, response);
            }
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "CUSTOMER, GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<ServiceRecordDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceRecordDto>>> GetServiceRecordById(int id)
        {
            try
            {
                var serviceRecord = await _serviceRecordRepository.GetByIdAsync(id);
                if (serviceRecord == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Không tìm thấy dịch vụ với ID {id}"));
                }
                var serviceRecordDto = serviceRecord.Adapt<ServiceRecordDto>();
                return Ok(ApiResponse<ServiceRecordDto>.Success("Lấy thông tin dịch vụ thành công", serviceRecordDto));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting serviceRecord by id {Id}", id);
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy thông tin  dịch vụ");
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<ServiceRecordCreateDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceRecordCreateDto>>> CreateServiceRecord([FromBody] ServiceRecordCreateDto serviceRecordCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                // ✅ Tạo ServiceRecord mới
                var serviceRecord = new ServiceRecord
                {
                    VehicleId = serviceRecordCreateDto.VehicleId,
                    UserId = UserContextHelper.GetUserId(User).Value,
                    ServiceRecordStatus = BusinessObjects.Models.Type.ServiceRecordStatus.PENDING,
                    StartTime = DateTime.UtcNow,
                    ServiceItems = new List<ServiceItem>()
                };

                // ✅ Load ServiceItems từ DB dựa trên IDs
                if (serviceRecordCreateDto.ServiceItems != null && serviceRecordCreateDto.ServiceItems.Any())
                {
                    foreach (var serviceItemId in serviceRecordCreateDto.ServiceItems)
                    {
                        var serviceItem = await _serviceItemRepository.GetByIdAsync(serviceItemId);
                        
                        if (serviceItem == null)
                        {
                            return BadRequest(ApiResponse<object>.BadRequest($"ServiceItem với ID {serviceItemId} không tồn tại"));
                        }

                        // ✅ Clone ServiceItem để link với ServiceRecord mới
                        // Không dùng ServiceItem gốc vì nó có thể đang được dùng ở chỗ khác
                        var newServiceItem = new ServiceItem
                        {
                            Name = serviceItem.Name,
                            Price = serviceItem.Price,
                            ServiceCategoryId = serviceItem.ServiceCategoryId,
                            IsActive = true
                            // ServiceRecordId sẽ tự động được set khi save
                        };

                        serviceRecord.ServiceItems.Add(newServiceItem);
                    }
                    await _serviceRecordRepository.AddAsync(serviceRecord);

                    return Created("", ApiResponse<ServiceRecordCreateDto>.Created("Tạo dịch vụ thành công", serviceRecordCreateDto));
                }
                return BadRequest("Tạo dịch vụ không thành công");

            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating serviceRecord");
                var response = ApiResponse<object>.InternalError("Lỗi khi tạo dịch vụ");
                return StatusCode(500, response);
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(ApiResponse<ServiceRecordUpdateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceRecordUpdateDto>>> UpdateServiceRecord(int id, [FromBody] ServiceRecordUpdateDto serviceRecordUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }
                var serviceRecordDB = await _serviceRecordRepository.GetByIdAsync(id);
                if (serviceRecordDB == null)
                {
                    var notFoundResponse = ApiResponse<object>.NotFound($"Không tìm thấy dịch vụ với ID {id}");
                    return NotFound(notFoundResponse);
                }
                serviceRecordUpdateDto.Adapt(serviceRecordDB);
                await _serviceRecordRepository.UpdateAsync(serviceRecordDB);
                return Ok(ApiResponse<ServiceRecordUpdateDto>.Success("Cập nhật dịch vụ thành công", serviceRecordUpdateDto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.NotFound(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating serviceRecord with id {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi cập nhật dịch vụ"));
            }
        }
    }
}
