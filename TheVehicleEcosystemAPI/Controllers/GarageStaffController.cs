using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.GarageStaff;
using BusinessObjects.Models.Type;
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
    public class GarageStaffController : ControllerBase
    {
        private readonly IGarageStaffRepository _garageStaffRepository;
        private readonly IGarageRepository _garageRepository;
        private readonly ILogger<GarageStaffController> _logger;
        private readonly CloudflareR2Storage _r2Storage;

        public GarageStaffController(
            IGarageStaffRepository garageStaffRepository,
            IGarageRepository garageRepository,
            ILogger<GarageStaffController> logger,
            CloudflareR2Storage r2Storage)
        {
            _garageStaffRepository = garageStaffRepository;
            _garageRepository = garageRepository;
            _logger = logger;
            _r2Storage = r2Storage;
        }

        /// <summary>
        /// Lấy danh sách tất cả nhân viên garage (Chỉ GARAGE)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedData<GarageStaffDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedData<GarageStaffDto>>>> GetAllStaff(
           [FromQuery] int page = 1,
           [FromQuery] int size = 30,
           [FromQuery] string? sortBy = null,
           [FromQuery] bool isAsc = true)
        {
            try
            {
                // Get current user's garageId
                var userId = UserContextHelper.GetUserId(User);
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Không tìm thấy thông tin người dùng"));
                }

                // Get garage of current user
                var garage = await _garageRepository.GetByUserIdAsync(userId.Value);
                if (garage == null)
                {
                    return NotFound(ApiResponse<object>.NotFound("Không tìm thấy thông tin garage"));
                }

                var (items, total) = await _garageStaffRepository.GetAllByGarageIdAsync(garage.Id, page, size, sortBy, isAsc);
                var garageStaffDtos = items.Select(gs => gs.Adapt<GarageStaffDto>());

                var paginatedData = new PaginatedData<GarageStaffDto>(garageStaffDtos, total, page, size);
                var response = ApiResponse<PaginatedData<GarageStaffDto>>.Success(
                    "Lấy danh sách nhân viên thành công",
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
                _logger.LogError(ex, "Error getting garage staff");
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy danh sách nhân viên");
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Lấy thông tin nhân viên theo ID (GARAGE)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<GarageStaffDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<GarageStaffDto>>> GetStaffById(int id)
        {
            try
            {
                var staff = await _garageStaffRepository.GetByIdAsync(id);
                if (staff == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Không tìm thấy nhân viên với ID {id}"));
                }

                // Verify the staff belongs to current user's garage
                var userId = UserContextHelper.GetUserId(User);
                if (userId.HasValue)
                {
                    var garage = await _garageRepository.GetByUserIdAsync(userId.Value);
                    if (garage != null && staff.GarageId != garage.Id)
                    {
                        return Forbid();
                    }
                }

                var garageStaffDto = staff.Adapt<GarageStaffDto>();
                return Ok(ApiResponse<GarageStaffDto>.Success("Lấy thông tin nhân viên thành công", garageStaffDto));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting garage staff by id {Id}", id);
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy thông tin nhân viên");
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Tạo nhân viên mới (Chỉ GARAGE)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<GarageStaffDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<GarageStaffDto>>> CreateStaff([FromBody] GarageStaffCreateDto staffCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(staffCreateDto.Email))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Email là bắt buộc"));
                }

                if (string.IsNullOrWhiteSpace(staffCreateDto.Password))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Mật khẩu là bắt buộc"));
                }

                if (string.IsNullOrWhiteSpace(staffCreateDto.FullName))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Họ tên là bắt buộc"));
                }

                // Get current user's garage
                var userId = UserContextHelper.GetUserId(User);
                if (!userId.HasValue)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Không tìm thấy thông tin người dùng"));
                }

                var garage = await _garageRepository.GetByUserIdAsync(userId.Value);
                if (garage == null)
                {
                    return NotFound(ApiResponse<object>.NotFound("Không tìm thấy thông tin garage"));
                }

                // Check if email already exists
                var existingStaff = await _garageStaffRepository.GetByEmailAsync(staffCreateDto.Email);
                if (existingStaff != null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Email đã tồn tại trong hệ thống"));
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(staffCreateDto.Password);

                // Create GarageStaff entity
                var staff = new GarageStaff
                {
                    FullName = staffCreateDto.FullName,
                    Email = staffCreateDto.Email,
                    PhoneNumber = staffCreateDto.PhoneNumber,
                    PasswordHash = passwordHash,
                    GarageRole = staffCreateDto.GarageRole,
                    UserStatus = UserStatus.ACTIVE,
                    IsActive = true,
                    GarageId = garage.Id
                };

                await _garageStaffRepository.AddAsync(staff);

                var garageStaffDto = staff.Adapt<GarageStaffDto>();
                return Created("", ApiResponse<GarageStaffDto>.Created("Tạo nhân viên thành công", garageStaffDto));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating garage staff");
                var response = ApiResponse<object>.InternalError("Lỗi khi tạo nhân viên");
                return StatusCode(500, response);
            }
        }


        /// <summary>
        /// Cập nhật thông tin nhân viên (GARAGE)
        /// </summary>
        [HttpPatch("{id}")]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<GarageStaffDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<GarageStaffDto>>> UpdateStaff(int id, [FromBody] GarageStaffUpdateDto staffUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                var staffDB = await _garageStaffRepository.GetByIdAsync(id);
                if (staffDB == null)
                {
                    var notFoundResponse = ApiResponse<object>.NotFound($"Không tìm thấy nhân viên với ID {id}");
                    return NotFound(notFoundResponse);
                }

                // Verify the staff belongs to current user's garage
                var userId = UserContextHelper.GetUserId(User);
                if (userId.HasValue)
                {
                    var garage = await _garageRepository.GetByUserIdAsync(userId.Value);
                    if (garage != null && staffDB.GarageId != garage.Id)
                    {
                        return Forbid();
                    }
                }

                // Update fields
                if (!string.IsNullOrWhiteSpace(staffUpdateDto.FullName))
                {
                    staffDB.FullName = staffUpdateDto.FullName;
                }

                if (staffUpdateDto.PhoneNumber != null)
                {
                    staffDB.PhoneNumber = staffUpdateDto.PhoneNumber;
                }

                if (staffUpdateDto.GarageRole.HasValue)
                {
                    staffDB.GarageRole = staffUpdateDto.GarageRole.Value;
                }

                if (staffUpdateDto.UserStatus.HasValue)
                {
                    staffDB.UserStatus = staffUpdateDto.UserStatus.Value;
                }

                await _garageStaffRepository.UpdateAsync(staffDB);

                var result = staffDB.Adapt<GarageStaffDto>();
                return Ok(ApiResponse<GarageStaffDto>.Success("Cập nhật nhân viên thành công", result));
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
                _logger.LogError(ex, "Error updating garage staff with id {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi cập nhật nhân viên"));
            }
        }


        /// <summary>
        /// Cập nhật ảnh nhân viên (GARAGE)
        /// </summary>
        [HttpPatch("image/{id}")]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<GarageStaffDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<GarageStaffDto>>> UpdateImage(int id, IFormFile imageFile)
        {
            try
            {
                if (imageFile == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("File ảnh là bắt buộc"));
                }

                var staffDB = await _garageStaffRepository.GetByIdAsync(id);
                if (staffDB == null)
                {
                    var notFoundResponse = ApiResponse<object>.NotFound($"Không tìm thấy nhân viên với ID {id}");
                    return NotFound(notFoundResponse);
                }

                // Verify the staff belongs to current user's garage
                var userId = UserContextHelper.GetUserId(User);
                if (userId.HasValue)
                {
                    var garage = await _garageRepository.GetByUserIdAsync(userId.Value);
                    if (garage != null && staffDB.GarageId != garage.Id)
                    {
                        return Forbid();
                    }
                }

                // Upload image to Cloudflare R2
                string? imageUrl = null;
                try
                {
                    imageUrl = await _r2Storage.UploadImageAsync(imageFile, "garage-staff");
                    _logger.LogInformation("Image uploaded successfully to R2 for garage staff with ID {Id}", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload image to R2 for garage staff");
                    var response = ApiResponse<object>.BadRequest("Lỗi khi tải ảnh lên. Vui lòng thử lại.");
                    return BadRequest(response);
                }

                staffDB.Image = imageUrl;
                await _garageStaffRepository.UpdateAsync(staffDB);

                var result = staffDB.Adapt<GarageStaffDto>();
                return Ok(ApiResponse<GarageStaffDto>.Success("Cập nhật ảnh nhân viên thành công", result));
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
                _logger.LogError(ex, "Error updating garage staff image with id {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi cập nhật ảnh nhân viên"));
            }
        }


        /// <summary>
        /// Xóa nhân viên (Chỉ GARAGE)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteStaff(int id)
        {
            try
            {
                var staffDB = await _garageStaffRepository.GetByIdAsync(id);
                if (staffDB == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Không tìm thấy nhân viên với ID {id}"));
                }

                // Verify the staff belongs to current user's garage
                var userId = UserContextHelper.GetUserId(User);
                if (userId.HasValue)
                {
                    var garage = await _garageRepository.GetByUserIdAsync(userId.Value);
                    if (garage != null && staffDB.GarageId != garage.Id)
                    {
                        return Forbid();
                    }
                }

                await _garageStaffRepository.DeleteAsync(id);
                return Ok(ApiResponse<object>.Success("Xóa nhân viên thành công"));
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
                _logger.LogError(ex, "Error deleting garage staff with id {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi xóa nhân viên"));
            }
        }
    }
}
