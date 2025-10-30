using BusinessObjects.Models.DTOs.Category;
using BusinessObjects.Models.DTOs.Product;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using TheVehicleEcosystemAPI.Response.DTOs;

namespace TheVehicleEcosystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryRepository categoryRepository, ILogger<CategoryController> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetListCategories()
        {
            try { 
                var categories = await _categoryRepository.GetAllCategoriesAsync();
                var categoryDtos = categories.Adapt<List<CategoryDto>>();
                return Ok(ApiResponse<List<CategoryDto>>.Success("Lấy danh sách danh mục thành công", categoryDtos));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy list sản phẩm");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

    }
}
