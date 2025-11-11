using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.ServiceCategory;
using BusinessObjects.Models.DTOs.ServiceItem;
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
    public class ServiceItemController : ControllerBase
    {
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly IServiceItemRepository _serviceItemRepository;
        private readonly ILogger<ServiceCategoryController> _logger;

        public ServiceItemController(
            IServiceCategoryRepository serviceCategoryRepository,
            IServiceItemRepository serviceItemRepository,
            ILogger<ServiceCategoryController> logger)
        {
            _serviceCategoryRepository = serviceCategoryRepository;
            _serviceItemRepository = serviceItemRepository;
            _logger = logger;
        }


        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PaginatedData<ServiceItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedData<ServiceItemDto>>>> GetAllServiceItems(
           [FromQuery] int page = 1,
           [FromQuery] int size = 30,
           [FromQuery] string? sortBy = null,
           [FromQuery] bool isAsc = true)
        {
            try
            {
                var (items, total) = await _serviceItemRepository.GetAllAsync(page, size, sortBy, isAsc);
                var serviceItemDtos = items.Select(u => u.Adapt<ServiceItemDto>());

                var paginatedData = new PaginatedData<ServiceItemDto>(serviceItemDtos, total, page, size);
                var response = ApiResponse<PaginatedData<ServiceItemDto>>.Success(
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
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(ApiResponse<ServiceItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceItemDto>>> GetServiceCategoryById(int id)
        {
            try
            {
                var serviceItem = await _serviceItemRepository.GetByIdAsync(id);
                if (serviceItem == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Không tìm thấy dịch vụ với ID {id}"));
                }

                var serviceCategoryDto = serviceItem.Adapt<ServiceItemDto>();
                return Ok(ApiResponse<ServiceItemDto>.Success("Lấy thông tin dịch vụ thành công", serviceCategoryDto));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting serviceItem by id {Id}", id);
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy thông tin  dịch vụ");
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(ApiResponse<ServiceItemCreateDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceItemCreateDto>>> CreateServiceItem([FromBody] ServiceItemCreateDto serviceItemCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                ServiceItem serviceItem = serviceItemCreateDto.Adapt<ServiceItem>();
                await _serviceItemRepository.AddAsync(serviceItem);

                return Created("", ApiResponse<ServiceItemCreateDto>.Created("Tạo  dịch vụ thành công", serviceItemCreateDto));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating serviceItem");
                var response = ApiResponse<object>.InternalError("Lỗi khi tạo dịch vụ");
                return StatusCode(500, response);
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(ApiResponse<ServiceItemUpdateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServiceItemUpdateDto>>> UpdateServiceItem(int id, [FromBody] ServiceItemUpdateDto serviceItemUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }
                var serviceItemDB = await _serviceItemRepository.GetByIdAsync(id);
                if (serviceItemDB == null)
                {
                    var notFoundResponse = ApiResponse<object>.NotFound($"Không tìm thấy dịch vụ với ID {id}");
                    return NotFound(notFoundResponse);
                }
                serviceItemUpdateDto.Adapt(serviceItemDB);
                await _serviceItemRepository.UpdateAsync(serviceItemDB);
                return Ok(ApiResponse<ServiceItemUpdateDto>.Success("Cập nhật dịch vụ thành công", serviceItemUpdateDto));
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
                _logger.LogError(ex, "Error updating serviceItem with id {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi cập nhật dịch vụ"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteServiceItem(int id)
        {
            try
            {
                await _serviceItemRepository.DeleteAsync(id);
                return Ok(ApiResponse<object>.Success("Xóa  dịch vụ thành công"));
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
                _logger.LogError(ex, "Error deleting serviceItem with id {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi xóa dịch vụ"));
            }
        }
    }
}
