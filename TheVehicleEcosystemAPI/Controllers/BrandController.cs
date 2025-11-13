
using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Brand;
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

    public class BrandController : ControllerBase
    {
        private readonly IBrandRepository _brandRepository;
        private readonly ILogger<BrandController> _logger;

        public BrandController(IBrandRepository brandRepository, ILogger<BrandController> logger)
        {
            _brandRepository = brandRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy tất cả các hãng xe đang hoạt động
        /// </summary>
        [Authorize(Roles = "CUSTOMER,DEALER")]
        [HttpGet]
        public async Task<IActionResult> GetListBrands()
        {
            try
            {
                var brands = await _brandRepository.GetAllBrandsAsync();
                var brandDtos = brands.Adapt<List<BrandDto>>();
                return Ok(ApiResponse<List<BrandDto>>.Success("Lấy danh sách hãng xe thành công", brandDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy list hãng xe");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Lấy thông tin hãng xe theo ID
        /// </summary>
        [Authorize(Roles = "CUSTOMER,DEALER")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBrandById(int id)
        {
            try
            {
                var brand = await _brandRepository.GetBrandByIdAsync(id);
                if (brand == null)
                {
                    // Trả về 404 nếu không tìm thấy (và còn active)
                    return NotFound("Không tìm thấy hãng xe.");
                }

                var brandDto = brand.Adapt<BrandDto>();
                return Ok(ApiResponse<BrandDto>.Success("Lấy thông tin hãng xe thành công", brandDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy hãng xe với ID: {id}");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Tạo hãng xe mới
        /// </summary>
        [Authorize(Roles = "DEALER")]
        [HttpPost]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto brandDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Dữ liệu đầu vào không hợp lệ.");
            }

            try
            {
                var brand = brandDto.Adapt<Brand>();
                var createdBrand = await _brandRepository.CreateBrandAsync(brand);
                var createdBrandDto = createdBrand.Adapt<BrandDto>();

                return CreatedAtAction(nameof(GetBrandById), new { id = createdBrand.Id },
                    ApiResponse<BrandDto>.Success("Tạo hãng xe thành công", createdBrandDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hãng xe");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Cập nhật thông tin hãng xe
        /// </summary>
        [Authorize(Roles = "DEALER")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, [FromBody] UpdateBrandDto brandDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Dữ liệu đầu vào không hợp lệ.");
            }

            try
            {
                var existingBrand = await _brandRepository.GetBrandByIdAsync(id);
                if (existingBrand == null)
                {
                    return NotFound("Không tìm thấy hãng xe để cập nhật.");
                }

                brandDto.Adapt(existingBrand);

                await _brandRepository.UpdateBrandAsync(existingBrand);

                var updatedBrandDto = existingBrand.Adapt<BrandDto>();
                return Ok(ApiResponse<BrandDto>.Success("Cập nhật hãng xe thành công", updatedBrandDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật hãng xe với ID: {id}");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }

        /// <summary>
        /// Xóa (soft-delete) hãng xe
        /// </summary>
        [Authorize(Roles = "DEALER")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            try
            {
                // DAO sẽ tìm, set IsActive = false, và SaveChanges
                var result = await _brandRepository.DeleteBrandAsync(id);
                if (!result)
                {
                    // Trả về false nghĩa là không tìm thấy Brand (brand == null trong DAO)
                    return NotFound("Không tìm thấy hãng xe để xóa.");
                }

                return Ok(ApiResponse<string>.Success("Xóa (tạm) hãng xe thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa hãng xe với ID: {id}");
                return StatusCode(500, ApiResponse<string>.InternalError("Lỗi máy chủ."));
            }
        }
    }
}
