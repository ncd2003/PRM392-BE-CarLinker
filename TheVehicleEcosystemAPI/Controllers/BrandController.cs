
using BusinessObjects.Models.DTOs.Brand;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using TheVehicleEcosystemAPI.Response.DTOs;

namespace TheVehicleEcosystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IBrandRepository _brandRepository;
        private readonly ILogger<BrandController> _logger;

        public BrandController(IBrandRepository brandRepository, ILogger<BrandController> logger)
        {
            _brandRepository = brandRepository;
            _logger = logger;
        }

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
    }
}
