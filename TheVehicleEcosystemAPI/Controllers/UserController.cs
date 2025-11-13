using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.ServiceCategory;
using BusinessObjects.Models.DTOs.ServiceItem;
using BusinessObjects.Models.DTOs.User;
using BusinessObjects.Models.DTOs.Vehicle;
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
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;
        private readonly CloudflareR2Storage _r2Storage;

        public UserController(IUserRepository userRepository, ILogger<UserController> logger, CloudflareR2Storage r2Storage)
        {
            _userRepository = userRepository;
            _logger = logger;
            _r2Storage = r2Storage;
        }

        /// <summary>
        /// Lấy danh sách tất cả user (Chỉ GARAGE)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedData<UserDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedData<UserDto>>>> GetAllUsers(
           [FromQuery] int page = 1,
           [FromQuery] int size = 30,
           [FromQuery] string? sortBy = null,
           [FromQuery] bool isAsc = true)
        {
            try
            {
                var (items, total) = await _userRepository.GetAllAsysnc(page, size, sortBy, isAsc);
                var userDtos = items.Select(u => u.Adapt<UserDto>());

                var paginatedData = new PaginatedData<UserDto>(userDtos, total, page, size);
                var response = ApiResponse<PaginatedData<UserDto>>.Success(
                    "Lấy danh sách người dùng thành công",
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
                _logger.LogError(ex, "Error getting users");
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy danh sách người dùng");
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Lấy thông tin user theo ID (GARAGE có thể xem tất cả, user khác chỉ xem của mình)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "CUSTOMER,GARAGE,CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Không tìm thấy user với ID {id}"));
                }

                var userDto = user.Adapt<UserDto>();
                return Ok(ApiResponse<UserDto>.Success("Lấy thông tin user thành công", userDto));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by id {Id}", id);
                var response = ApiResponse<object>.InternalError("Lỗi khi lấy thông tin user");
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Tạo user mới (Chỉ GARAGE)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] UserCreateDto userCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var response = ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ");
                    return BadRequest(response);
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(userCreateDto.Email))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Email là bắt buộc"));
                }

                if (string.IsNullOrWhiteSpace(userCreateDto.Password))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Mật khẩu là bắt buộc"));
                }

                if (string.IsNullOrWhiteSpace(userCreateDto.FullName))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Họ tên là bắt buộc"));
                }

                // Check if email already exists
                var existingUser = await _userRepository.GetByEmailAsync(userCreateDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Email đã tồn tại trong hệ thống"));
                }

                // Check if phone number already exists (if provided)
                if (!string.IsNullOrWhiteSpace(userCreateDto.PhoneNumber))
                {
                    var users = await _userRepository.GetAllAsysnc();
                    if (users.Any(u => u.PhoneNumber == userCreateDto.PhoneNumber))
                    {
                        return BadRequest(ApiResponse<object>.BadRequest("Số điện thoại đã tồn tại trong hệ thống"));
                    }
                }

                // Map DTO to User entity (Mapster will automatically hash password)
                var user = userCreateDto.Adapt<User>();

                await _userRepository.AddAsync(user);

                var userDto = user.Adapt<UserDto>();
                return Created("", ApiResponse<UserDto>.Created("Tạo người dùng thành công", userDto));
            }
            catch (ArgumentException ex)
            {
                var response = ApiResponse<object>.BadRequest(ex.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                var response = ApiResponse<object>.InternalError("Lỗi khi tạo người dùng");
                return StatusCode(500, response);
            }
        }


        /// <summary>
        /// Cập nhật thông tin user (GARAGE có thể cập nhật tất cả, user khác chỉ cập nhật của mình)
        /// </summary>
        [HttpPatch("{id}")]
        [Authorize(Roles = "GARAGE,CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<UserUpdateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserUpdateDto>>> UpdateUser(int id, [FromBody] UserUpdateDto userUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                var userDB = await _userRepository.GetByIdAsync(id);
                if (userDB == null)
                {
                    var notFoundResponse = ApiResponse<object>.NotFound($"Không tìm thấy người dùng với ID {id}");
                    return NotFound(notFoundResponse);
                }
                userUpdateDto.Adapt(userDB);
                await _userRepository.UpdateAsync(userDB);
                return Ok(ApiResponse<UserUpdateDto>.Success("Cập nhật user thành công"));
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
                _logger.LogError(ex, "Error updating user with id {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi cập nhật user"));
            }
        }


        /// <summary>
        /// Cập nhật thông tin user (GARAGE có thể cập nhật tất cả, user khác chỉ cập nhật của mình)
        /// </summary>
        [HttpPatch("image/{id}")]
        [Authorize(Roles = "GARAGE,CUSTOMER")]
        [ProducesResponseType(typeof(ApiResponse<UserUpdateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserUpdateDto>>> UpdateImage(int id, IFormFile imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                var userDB = await _userRepository.GetByIdAsync(id);
                if (userDB == null)
                {
                    var notFoundResponse = ApiResponse<object>.NotFound($"Không tìm thấy người dùng với ID {id}");
                    return NotFound(notFoundResponse);
                }

                // Upload image to Cloudflare R2 if provided
                string? imageUrl = null;
                if (imageFile != null)
                {
                    try
                    {
                        // Upload to "vehicles" folder in R2
                        imageUrl = await _r2Storage.UploadImageAsync(imageFile, "users");
                        _logger.LogInformation("Image uploaded successfully to R2 for user with ID {Id}", id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload image to R2 for vehicle");
                        var response = ApiResponse<object>.BadRequest("Lỗi khi tải ảnh lên. Vui lòng thử lại.");
                        return BadRequest(response);
                    }
                }
                userDB.Image = imageUrl ?? string.Empty;
                await _userRepository.UpdateAsync(userDB);
                return Ok(ApiResponse<UserUpdateDto>.Success("Cập nhật ảnh user thành công"));
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
                _logger.LogError(ex, "Error updating user with id {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi cập nhật ảnh user"));
            }
        }


        /// <summary>
        /// Xóa user (Chỉ GARAGE)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
        {
            try
            {
                await _userRepository.DeleteAsync(id);
                return Ok(ApiResponse<object>.Success("Xóa user thành công"));
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
                _logger.LogError(ex, "Error deleting user with id {Id}", id);
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi xóa user"));
            }
        }
    }
}
