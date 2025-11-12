using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Category;
using BusinessObjects.Models.DTOs.Product;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using TheVehicleEcosystemAPI.Response.DTOs;

namespace TheVehicleEcosystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryRepository categoryRepository, ILogger<CategoryController> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy tất cả các danh mục đang hoạt động
        /// </summary>
        [Authorize(Roles = "CUSTOMER,DEALER")]
        [HttpGet]
        public async Task<IActionResult> GetListCategories()
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync();
                var categoryDtos = categories.Adapt<List<CategoryDto>>();
                return Ok(ApiResponse<List<CategoryDto>>.Success("Lấy danh sách danh mục thành công", categoryDtos));
            }
            catch (Exception ex)
            {
                // Sửa lỗi chính tả log (từ "sản phẩm" -> "danh mục")
                _logger.LogError(ex, "Lỗi khi lấy list danh mục");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Lấy thông tin danh mục theo ID
        /// </summary>
        [Authorize(Roles = "CUSTOMER,DEALER")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound("Không tìm thấy danh mục.");
                }

                var categoryDto = category.Adapt<CategoryDto>();
                return Ok(ApiResponse<CategoryDto>.Success("Lấy thông tin danh mục thành công", categoryDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh mục với ID: {id}");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Tạo danh mục mới
        /// </summary>
        [Authorize(Roles = "DEALER")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Dữ liệu đầu vào không hợp lệ.");
            }

            try
            {
                var category = categoryDto.Adapt<Category>();
                var createdCategory = await _categoryRepository.CreateCategoryAsync(category);
                var createdCategoryDto = createdCategory.Adapt<CategoryDto>();

                return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id },
                    ApiResponse<CategoryDto>.Success("Tạo danh mục thành công", createdCategoryDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo danh mục");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Cập nhật thông tin danh mục
        /// </summary>
        [Authorize(Roles = "DEALER")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Dữ liệu đầu vào không hợp lệ.");
            }

            try
            {
                // 1. Fetch (Lấy và theo dõi entity)
                var existingCategory = await _categoryRepository.GetCategoryByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound("Không tìm thấy danh mục để cập nhật.");
                }

                // 2. Map (Gán DTO vào entity đã được theo dõi)
                // EF tự động phát hiện thay đổi
                categoryDto.Adapt(existingCategory);

                // 3. Save (DAO chỉ cần gọi SaveChangesAsync)
                await _categoryRepository.UpdateCategoryAsync(existingCategory);

                var updatedCategoryDto = existingCategory.Adapt<CategoryDto>();
                return Ok(ApiResponse<CategoryDto>.Success("Cập nhật danh mục thành công", updatedCategoryDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật danh mục với ID: {id}");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Xóa (soft-delete) danh mục
        /// </summary>
        [Authorize(Roles = "DEALER")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                // DAO sẽ tìm, set IsActive = false, và SaveChanges
                var result = await _categoryRepository.DeleteCategoryAsync(id);
                if (!result)
                {
                    // Trả về false nghĩa là không tìm thấy
                    return NotFound("Không tìm thấy danhMục để xóa.");
                }

                return Ok(ApiResponse<string>.Success("Xóa (tạm) danh mục thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa danh mục với ID: {id}");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

    }
}
