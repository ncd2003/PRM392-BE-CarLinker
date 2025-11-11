using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Garage;
using BusinessObjects.Models.DTOs.ServiceRecord;
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
    public class GarageController : ControllerBase
    {
        private readonly IGarageRepository _garageRepository;
        private readonly ILogger<VehicleController> _logger;
        private readonly CloudflareR2Storage _r2Storage;

        public GarageController(
            IGarageRepository garageRepository,
            ILogger<VehicleController> logger,
            CloudflareR2Storage r2Storage)
        {
            _garageRepository = garageRepository;
            _logger = logger;
            _r2Storage = r2Storage;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PaginatedData<GarageDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedData<Garage>>>> GetAllGarages(
           [FromQuery] int page = 1,
           [FromQuery] int size = 30,
           [FromQuery] string? sortBy = null,
           [FromQuery] bool isAsc = true)
        {
            try
            {
                var (items, total) = await _garageRepository.GetAllAsync(page, size, sortBy, isAsc);
                var garageDtos = items.Select(g => g.Adapt<GarageDto>());
                var paginatedData = new PaginatedData<GarageDto>(garageDtos, total, page, size);
                var response = ApiResponse<PaginatedData<GarageDto>>.Success("Lấy danh sách gara thành công",paginatedData);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting garage");
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy gara");
                return StatusCode(500, response);
            }
        }
        [HttpPost]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(ApiResponse<GarageCreateDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<GarageCreateDto>>> CreateGarage([FromForm] GarageCreateDto garageCreateDto, IFormFile imageFile)
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
                        imageUrl = await _r2Storage.UploadImageAsync(imageFile, "garage");
                        _logger.LogInformation("Image uploaded successfully to R2 for garage");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload image to R2 for garage");
                        var response = ApiResponse<object>.BadRequest("Lỗi khi tải ảnh lên. Vui lòng thử lại.");
                        return BadRequest(response);
                    }
                }
                Garage garage = garageCreateDto.Adapt<Garage>();
                garage.UserId = UserContextHelper.GetUserId(User).Value;
                garage.Image = imageUrl ?? string.Empty;
                await _garageRepository.AddAsync(garage);

                return Created("", ApiResponse<GarageCreateDto>.Created("Tạo gara thành công", garageCreateDto));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating garage");
                var response = ApiResponse<object>.InternalError("Lỗi khi tạo gara");
                return StatusCode(500, response);
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(ApiResponse<GarageUpdateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<GarageUpdateDto>>> UpdateGarage(int id, [FromForm] GarageUpdateDto garageUpdateDto, IFormFile? imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                // Get existing garage first
                var garageDB = await _garageRepository.GetByIdAsync(id);
                if (garageDB == null)
                {
                    var notFoundResponse = ApiResponse<object>.NotFound("Không tìm thấy gara}");
                    return NotFound(notFoundResponse);
                }

                // Upload new image to R2 if provided
                string? imageUrl = garageDB.Image; // Keep old image by default
                if (imageFile != null)
                {
                    try
                    {
                        // Delete old image from R2 if exists
                        if (!string.IsNullOrEmpty(garageDB.Image))
                        {
                            await _r2Storage.DeleteImageAsync(garageDB.Image);
                            _logger.LogInformation("Old image deleted from R2 for garage ID");
                        }

                        // Upload new image
                        imageUrl = await _r2Storage.UploadImageAsync(imageFile, "vehicles");
                        _logger.LogInformation("Image uploaded successfully to R2 for garage ID");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload image to R2 for garage update");
                        var response = ApiResponse<object>.BadRequest("Lỗi khi tải ảnh lên. Vui lòng thử lại.");
                        return BadRequest(response);
                    }
                }

                // Map DTO to entity
                garageUpdateDto.Adapt(garageDB);
                garageDB.UserId = UserContextHelper.GetUserId(User).Value;
                garageDB.Image = imageUrl ?? string.Empty;

                await _garageRepository.UpdateAsync(garageDB);
                return Ok(ApiResponse<GarageUpdateDto>.Success("Cập nhật gara thành công"));
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
                _logger.LogError(ex, "Error updating garage");
                var response = ApiResponse<object>.InternalError("Lỗi khi cập nhật gara");
                return StatusCode(500, response);
            }
        }
    }
}
