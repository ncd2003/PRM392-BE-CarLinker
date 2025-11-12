using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.ServiceCategory;
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
    public class ServiceCategoryController : ControllerBase
    {
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly IServiceItemRepository _serviceItemRepository;
        private readonly IGarageRepository _garageRepository;
        private readonly ILogger<ServiceCategoryController> _logger;

        public ServiceCategoryController(
            IServiceCategoryRepository serviceCategoryRepository,
            IGarageRepository garageRepository,
            IServiceItemRepository serviceItemRepository,
            ILogger<ServiceCategoryController> logger)
        {
            _serviceCategoryRepository = serviceCategoryRepository;
            _garageRepository = garageRepository;
            _serviceItemRepository = serviceItemRepository;
            _logger = logger;
        }

        [HttpGet()]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedData<ServiceCategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceCategoryDto>>> GetAllServiceCategories(
           [FromQuery] int page = 1,
           [FromQuery] int size = 30,
           [FromQuery] string? sortBy = null,
           [FromQuery] bool isAsc = true)
        {
            try
            {
                var items = await _serviceCategoryRepository.GetAllAsync(page, size, sortBy, isAsc);
                var serviceCategoryDtos = items.Adapt<IEnumerable<ServiceCategoryDto>>();
                return Ok(ApiResponse<IEnumerable<ServiceCategoryDto>>.Success("Lấy danh sách gói dịch vụ thành công", serviceCategoryDtos));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting serviceCategories");
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy danh sách gói dịch vụ");
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN, GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<ServiceCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceCategoryDto>>> GetServiceCategoryById(int id)
        {
            try
            {
                var serviceCategory = await _serviceCategoryRepository.GetByIdAsync(id);
                if (serviceCategory == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Không tìm thấy gói dịch vụ với ID {id}"));
                }

                var serviceCategoryDto = serviceCategory.Adapt<ServiceCategoryDto>();
                return Ok(ApiResponse<ServiceCategoryDto>.Success("Lấy thông tin gói dịch vụ thành công", serviceCategoryDto));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting serviceCategory by id {Id}", id);
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy thông tin gói dịch vụ");
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(ApiResponse<ServiceCategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceCategoryDto>>> CreateServiceCategory([FromBody] ServiceCategoryCreateDto serviceCategoryCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                // Create ServiceCategory
                var serviceCategory = new ServiceCategory
                {
                    Name = serviceCategoryCreateDto.Name,
                    IsActive = true
                };

                await _serviceCategoryRepository.AddAsync(serviceCategory);

                // Update ServiceItems if provided
                if (serviceCategoryCreateDto.ServiceItems != null && serviceCategoryCreateDto.ServiceItems.Any())
                {
                    foreach (var serviceItemId in serviceCategoryCreateDto.ServiceItems)
                    {
                        var serviceItem = await _serviceItemRepository.GetByIdAsync(serviceItemId);
                        if (serviceItem != null)
                        {
                            serviceItem.ServiceCategoryId = serviceCategory.Id;
                            await _serviceItemRepository.UpdateAsync(serviceItem);
                        }
                    }
                }

                var result = serviceCategory.Adapt<ServiceCategoryDto>();
                return Created("", ApiResponse<ServiceCategoryDto>.Created("Tạo gói dịch vụ thành công", result));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating serviceCategory");
                var response = ApiResponse<object>.InternalError("Lỗi khi tạo gói dịch vụ");
                return StatusCode(500, response);
            }
        }


        [HttpPatch("{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(ApiResponse<ServiceCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceCategoryDto>>> UpdateServiceCategory(int id, [FromBody] ServiceCategoryUpdateDto serviceCategoryUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                var serviceCategoryDB = await _serviceCategoryRepository.GetByIdAsync(id);
                if (serviceCategoryDB == null)
                {
                    var notFoundResponse = ApiResponse<object>.NotFound($"Không tìm thấy gói dịch vụ với ID {id}");
                    return NotFound(notFoundResponse);
                }

                // Update basic properties
                if (!string.IsNullOrWhiteSpace(serviceCategoryUpdateDto.Name))
                {
                    serviceCategoryDB.Name = serviceCategoryUpdateDto.Name;
                }

                // Update ServiceItems if provided
                if (serviceCategoryUpdateDto.ServiceItems != null)
                {
                    // Get all current items in this category
                    var allItems = await _serviceItemRepository.GetAllAsync();
                    var currentItems = allItems.Where(si => si.ServiceCategoryId == id).ToList();

                    // Remove items that are no longer in the category
                    foreach (var item in currentItems)
                    {
                        if (!serviceCategoryUpdateDto.ServiceItems.Contains(item.Id))
                        {
                            item.ServiceCategoryId = null;
                            await _serviceItemRepository.UpdateAsync(item);
                        }
                    }

                    // Add new items to the category
                    foreach (var serviceItemId in serviceCategoryUpdateDto.ServiceItems)
                    {
                        var serviceItem = await _serviceItemRepository.GetByIdAsync(serviceItemId);
                        if (serviceItem != null && serviceItem.ServiceCategoryId != id)
                        {
                            serviceItem.ServiceCategoryId = id;
                            await _serviceItemRepository.UpdateAsync(serviceItem);
                        }
                    }
                }

                await _serviceCategoryRepository.UpdateAsync(serviceCategoryDB);

                var result = serviceCategoryDB.Adapt<ServiceCategoryDto>();
                return Ok(ApiResponse<ServiceCategoryDto>.Success("Cập nhật gói dịch vụ thành công", result));
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
                _logger.LogError(ex, "Error updating serviceCategory with id {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi cập nhật gói dịch vụ"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteServiceCategory(int id)
        {
            try
            {
                await _serviceCategoryRepository.DeleteAsync(id);
                return Ok(ApiResponse<object>.Success("Xóa gói dịch vụ thành công"));
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
                _logger.LogError(ex, "Error deleting serviceCategory with id {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi xóa gói dịch vụ"));
            }
        }
    }
}
