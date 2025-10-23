using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Vehicle;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using TheVehicleEcosystemAPI.Response.DTOs;

namespace TheVehicleEcosystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<VehicleController> _logger;

        public VehicleController(IVehicleRepository vehicleRepository, ILogger<VehicleController> logger)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedData<VehicleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedData<VehicleDto>>>> GetAllVehicles(
            [FromQuery] int page = 1,
            [FromQuery] int size = 30,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isAsc = true)
        {
            try
            {
                var (items, total) = await _vehicleRepository.GetAllAsync(page, size, sortBy, isAsc);
                var vehicleDtos = items.Select(v => v.Adapt<VehicleDto>());

                var paginatedData = new PaginatedData<VehicleDto>(vehicleDtos, total, page, size);
                var response = ApiResponse<PaginatedData<VehicleDto>>.Success(
                    "Lấy danh sách xe thành công",
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
                _logger.LogError(ex, "Error getting vehicles");
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy danh sách xe");
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VehicleDto>>> GetVehicleById(int id)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(id);
                if (vehicle == null)
                {
                    var notFoundResponse = ApiResponse<object>.NotFound($"Không tìm thấy xe với ID {id}");
                    return NotFound(notFoundResponse);
                }
                var vehicleDto = vehicle.Adapt<VehicleDto>();

                var response = ApiResponse<VehicleDto>.Success("Lấy thông tin xe thành công", vehicleDto);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle by id {Id}", id);
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy thông tin xe");
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<VehicleCreateDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VehicleCreateDto>>> CreateVehicle([FromBody] VehicleCreateDto vehicleCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                await _vehicleRepository.AddAsync(vehicleCreateDto.Adapt<Vehicle>());

                var successResponse = ApiResponse<VehicleCreateDto>.Created("Tạo xe thành công", vehicleCreateDto);
                return CreatedAtAction(nameof(GetVehicleById), new {vehicleCreateDto}, successResponse);
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                var response = ApiResponse<object>.InternalError("Lỗi khi tạo xe");
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateVehicle(int id, [FromBody] VehicleUpdateDto vehicle)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                await _vehicleRepository.UpdateAsync(vehicle.Adapt<Vehicle>());
                var successResponse = ApiResponse<object>.Success("Cập nhật xe thành công");
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
                _logger.LogError(ex, "Error updating vehicle with id {Id}", id);
                var response = ApiResponse<object>.InternalError("Lỗi khi cập nhật xe");
                return StatusCode(500, response);
            }
        }

        
        [HttpDelete("{id}")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVehicle(int id)
        {
            try
            {
                await _vehicleRepository.DeleteAsync(id);
                var response = ApiResponse<object>.Success("Xóa xe thành công");
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
                _logger.LogError(ex, "Error deleting vehicle with id {Id}", id);
                var response = ApiResponse<object>.InternalError("Lỗi khi xóa xe");
                return StatusCode(500, response);
            }
        }
    }
}
