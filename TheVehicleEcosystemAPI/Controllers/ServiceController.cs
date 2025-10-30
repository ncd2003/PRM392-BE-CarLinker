using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Service;
using BusinessObjects.Models.DTOs.Service;
using DataAccess;
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
    public class ServiceController : ControllerBase
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<VehicleController> _logger;

        public ServiceController(IServiceRepository serviceRepository, ILogger<VehicleController> logger)
        {
            _serviceRepository = serviceRepository;
            _logger = logger;
        }


        [HttpGet]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedData<ServiceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedData<ServiceDto>>>> GetAllVehicles(
            [FromQuery] int page = 1,
            [FromQuery] int size = 30,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isAsc = true)
        {
            try
            {
                var (items, total) = await _serviceRepository.GetAllAsync(page, size, sortBy, isAsc);
                var serviceDtos = items.Select(s => s.Adapt<ServiceDto>());

                var paginatedData = new PaginatedData<ServiceDto>(serviceDtos, total, page, size);
                var response = ApiResponse<PaginatedData<ServiceDto>>.Success(
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
                _logger.LogError(ex, "Error getting servies");
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy danh sách dịch vụ");
                return StatusCode(500, response);
            }
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<ServiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceDto>>> GetVehicleById(int id)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(id);
                if (service == null)
                {
                    var notFoundResponse = ApiResponse<object>.NotFound($"Không tìm thấy dịch vụ với ID {id}");
                    return NotFound(notFoundResponse);
                }
                var serviceDto = service.Adapt<ServiceDto>();

                var response = ApiResponse<ServiceDto>.Success("Lấy thông tin dịch vụ thành công", serviceDto);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service by id {Id}", id);
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy thông tin dịch vụ");
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<ServiceCreateDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceCreateDto>>> CreateVehicle([FromBody] ServiceCreateDto vehicleCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }
                Service service = vehicleCreateDto.Adapt<Service>();
                await _serviceRepository.AddAsync(service);
                return Created("", ApiResponse<ServiceCreateDto>.Created("Tạo dịch vụ thành công", vehicleCreateDto));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                var response = ApiResponse<object>.InternalError("Lỗi khi tạo dịch vụ");
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateVehicle(int id, [FromBody] ServiceUpdateDto serviceUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }
                Service service = serviceUpdateDto.Adapt<Service>();
                await _serviceRepository.UpdateAsync(id, service);
                var successResponse = ApiResponse<object>.Success("Cập nhật dịch vụ thành công");
                return Ok(successResponse);
            }
            catch (KeyNotFoundException ex)
            {
                var response = ApiResponse<object>.NotFound(ex.Message);
                return NotFound(response);
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service with id {Id}", id);
                var response = ApiResponse<object>.InternalError("Lỗi khi cập nhật dịch vụ");
                return StatusCode(500, response);
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteService(int id)
        {
            try
            {
                await _serviceRepository.DeleteAsync(id);
                var response = ApiResponse<object>.Success("Xóa dịch vụ thành công");
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                var response = ApiResponse<object>.NotFound(ex.Message);
                return NotFound(response);
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service with id {Id}", id);
                var response = ApiResponse<object>.InternalError("Lỗi khi xóa dịch vụ");
                return StatusCode(500, response);
            }
        }
    }
}
