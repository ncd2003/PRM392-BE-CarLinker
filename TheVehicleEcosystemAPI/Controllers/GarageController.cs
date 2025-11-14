using Azure;
using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Garage;
using BusinessObjects.Models.DTOs.ServiceCategory;
using BusinessObjects.Models.DTOs.ServiceItem;
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
        private readonly IServiceItemRepository _serviceItemRepository;
        private readonly ILogger<VehicleController> _logger;
        private readonly CloudflareR2Storage _r2Storage;

        public GarageController(
            IGarageRepository garageRepository,
            IServiceItemRepository serviceItemRepository,
            ILogger<VehicleController> logger,
            CloudflareR2Storage r2Storage)
        {
            _garageRepository = garageRepository;
            _serviceItemRepository = serviceItemRepository;
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
                var response = ApiResponse<PaginatedData<GarageDto>>.Success("Lấy danh sách gara thành công", paginatedData);
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

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<GarageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Garage>>> GetGarageById(int id)
        {
            try
            {
                var garageDB = await _garageRepository.GetByIdAsync(id);
                var garageDto = garageDB.Adapt<GarageDto>();
                var response = ApiResponse<GarageDto>.Success("Lấy gara thành công", garageDto);
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
        [Authorize(Roles = "GARAGE")]
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

        [HttpPatch]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<GarageUpdateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<GarageUpdateDto>>> UpdateGarage([FromForm] GarageUpdateDto garageUpdateDto, IFormFile? imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                // Get existing garage first
                var garageDB = await _garageRepository.GetByUserIdAsync(UserContextHelper.GetUserId(User).Value);
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

        [HttpPatch("service-items")]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<GarageUpdateServiceItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<GarageUpdateServiceItem>>> UpdateGarageServiceItems([FromBody] List<GarageUpdateServiceItem> garageUpdateServiceItem)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                // Get existing garage first
                Garage garageDB = await _garageRepository.GetByUserIdAsync(UserContextHelper.GetUserId(User).Value);

                if (garageDB == null)
                {
                    return NotFound(ApiResponse<object>.NotFound("Không tìm thấy gara"));
                }

                ICollection<GarageServiceItem> garageServiceItems = new List<GarageServiceItem>();
                foreach (var gsi in garageUpdateServiceItem)
                {
                    ServiceItem serviceItemDB = await _serviceItemRepository.GetByIdAsync(gsi.ServiceItemId);
                    if (serviceItemDB == null)
                    {
                        return NotFound(ApiResponse<object>.NotFound($"Service Item với ID {gsi.ServiceItemId} không tồn tại"));
                    }
                    GarageServiceItem garageServiceItem = new GarageServiceItem()
                    {
                        GarageId = garageDB.Id,
                        ServiceItemId = gsi.ServiceItemId,
                        Price = gsi.Price,
                    };
                    garageServiceItems.Add(garageServiceItem);
                }

                garageDB.GarageServiceItems.Clear();
                foreach (var item in garageServiceItems)
                {
                    garageDB.GarageServiceItems.Add(item);
                }

                await _garageRepository.UpdateAsync(garageDB);
                return Ok(ApiResponse<GarageUpdateServiceItem>.Success("Cập nhật dịch vụ gara thành công"));
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
                _logger.LogError(ex, "Error updating garage service items");
                var response = ApiResponse<object>.InternalError("Lỗi khi cập nhật dịch vụ gara");
                return StatusCode(500, response);
            }
        }

        [HttpGet("details/{garageId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<GarageDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<GarageDetailDto>>> GetGarageDetails(int garageId)
        {
            try
            {
                var garageDB = await _garageRepository.GetByIdAsync(garageId);
                if (garageDB == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Không tìm thấy gara với ID {garageId}"));
                }

                _logger.LogInformation("Garage {GarageId} has {Count} GarageServiceItems",
                    garageId, garageDB.GarageServiceItems.Count);

                _logger.LogInformation("Active GarageServiceItems: {Count}",
                    garageDB.GarageServiceItems.Count(gsi => gsi.IsActive));

                // Create GarageDetailDto
                var garageDetailDto = new GarageDetailDto
                {
                    GarageDto = garageDB.Adapt<GarageDto>(),
                    TotalServiceItems = garageDB.GarageServiceItems.Count(gsi => gsi.IsActive)
                };

                // Group service items by category
                var groupedByCategory = garageDB.GarageServiceItems
                    .Where(gsi => gsi.IsActive && gsi.ServiceItem != null)
                    .GroupBy(gsi => new
                    {
                        CategoryId = gsi.ServiceItem.ServiceCategoryId ?? 0,
                        CategoryName = gsi.ServiceItem.ServiceCategory?.Name ?? "Không phân loại"
                    });

                // Build ServiceCategoryDto list
                foreach (var group in groupedByCategory)
                {
                    var serviceCategoryDto = new ServiceCategoryDto
                    {
                        Id = group.Key.CategoryId,
                        Name = group.Key.CategoryName,
                        ServiceItems = group.Select(gsi => new ServiceItemDto
                        {
                            Id = gsi.ServiceItemId,
                            Name = gsi.ServiceItem.Name,
                            Price = gsi.Price  // Price from GarageServiceItem (specific to this garage)
                        }).ToList()
                    };

                    garageDetailDto.ServiceCategories.Add(serviceCategoryDto);
                }

                // Sort categories by name
                garageDetailDto.ServiceCategories = garageDetailDto.ServiceCategories
                    .OrderBy(cat => cat.Name)
                    .ToList();

                return Ok(ApiResponse<GarageDetailDto>.Success(
                    "Lấy thông tin chi tiết gara thành công",
                    garageDetailDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting garage details for garage {GarageId}", garageId);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi lấy thông tin chi tiết gara"));
            }
        }

        /// <summary>
        /// Lấy User ID của garage theo Garage ID
        /// </summary>
        [HttpGet("{garageId}/user-id")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<int>>> GetGarageUserId(int garageId)
        {
            try
            {
                var garageDB = await _garageRepository.GetByIdAsync(garageId);
                if (garageDB == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Không tìm thấy gara với ID {garageId}"));
                }

                _logger.LogInformation("Retrieved User ID {UserId} for Garage {GarageId}", 
                    garageDB.UserId, garageId);

                return Ok(ApiResponse<int>.Success(
                    "Lấy User ID của gara thành công",
                    garageDB.UserId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting User ID for garage {GarageId}", garageId);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi lấy User ID của gara"));
            }
        }
    }
}
