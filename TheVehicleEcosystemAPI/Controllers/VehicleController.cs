using Azure;
using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Auth;
using BusinessObjects.Models.DTOs.Vehicle;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using TheVehicleEcosystemAPI.Response.DTOs;
using TheVehicleEcosystemAPI.Utils;

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
        private readonly CloudflareR2Storage _r2Storage;

        public VehicleController(
            IVehicleRepository vehicleRepository, 
            ILogger<VehicleController> logger,
            CloudflareR2Storage r2Storage)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
            _r2Storage = r2Storage;
        }

        [HttpGet]
        //[Authorize(Roles = "CUSTOMER")]
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
        //[Authorize(Roles = "CUSTOMER")]
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
        public async Task<ActionResult<ApiResponse<VehicleCreateDto>>> CreateVehicle([FromForm] VehicleCreateDto vehicleCreateDto, IFormFile imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                // Upload image to Cloudflare R2 if provided
                string? imageUrl = null;
                if (imageFile != null)
                {
                    try
                    {
                        // Upload to "vehicles" folder in R2
                        imageUrl = await _r2Storage.UploadImageAsync(imageFile, "vehicles");
                        _logger.LogInformation("Image uploaded successfully to R2 for vehicle with license plate {LicensePlate}", vehicleCreateDto.LicensePlate);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload image to R2 for vehicle");
                        var response = ApiResponse<object>.BadRequest("Lỗi khi tải ảnh lên. Vui lòng thử lại.");
                        return BadRequest(response);
                    }
                }

                Vehicle vehicle = vehicleCreateDto.Adapt<Vehicle>();
                
                // Get UserId from claims, or use default for testing
                vehicle.UserId = UserContextHelper.GetUserId(User).Value;
                vehicle.Image = imageUrl ?? string.Empty;
                
                await _vehicleRepository.AddAsync(vehicle);
                
                return Created("", ApiResponse<VehicleCreateDto>.Created("Tạo xe thành công", vehicleCreateDto));
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

        [HttpPatch("{id}")]
        //[Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateVehicle(int id, [FromForm] VehicleUpdateDto vehicleUpdateDto, IFormFile? imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                // Get existing vehicle first
                var existingVehicle = await _vehicleRepository.GetByIdAsync(id);
                if (existingVehicle == null)
                {
                    var notFoundResponse = ApiResponse<object>.NotFound($"Không tìm thấy xe với ID {id}");
                    return NotFound(notFoundResponse);
                }

                // Upload new image to R2 if provided
                string? imageUrl = existingVehicle.Image; // Keep old image by default
                if (imageFile != null)
                {
                    try
                    {
                        // Delete old image from R2 if exists
                        if (!string.IsNullOrEmpty(existingVehicle.Image))
                        {
                            await _r2Storage.DeleteImageAsync(existingVehicle.Image);
                            _logger.LogInformation("Old image deleted from R2 for vehicle ID {VehicleId}", id);
                        }

                        // Upload new image
                        imageUrl = await _r2Storage.UploadImageAsync(imageFile, "vehicles");
                        _logger.LogInformation("Image uploaded successfully to R2 for vehicle ID {VehicleId}", id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload image to R2 for vehicle update");
                        var response = ApiResponse<object>.BadRequest("Lỗi khi tải ảnh lên. Vui lòng thử lại.");
                        return BadRequest(response);
                    }
                }

                // Map DTO to entity
                Vehicle vehicle = vehicleUpdateDto.Adapt<Vehicle>();
                vehicle.UserId = UserContextHelper.GetUserId(User).Value;
                vehicle.Image = imageUrl ?? string.Empty; // Use new image or keep old one
                
                await _vehicleRepository.UpdateAsync(id, vehicle);
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
        //[Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVehicle(int id)
        {
            try
            {
                // Get vehicle to delete associated image
                var vehicle = await _vehicleRepository.GetByIdAsync(id);
                if (vehicle != null && !string.IsNullOrEmpty(vehicle.Image))
                {
                    // Try to delete image from R2
                    await _r2Storage.DeleteImageAsync(vehicle.Image);
                    _logger.LogInformation("Image deleted from R2 for vehicle ID {VehicleId}", id);
                }

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
