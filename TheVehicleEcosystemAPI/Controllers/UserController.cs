using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.User;
using BusinessObjects.Models.DTOs.Auth;
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
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả user (Chỉ ADMIN)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
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
        /// Lấy thông tin user theo ID (ADMIN có thể xem tất cả, user khác chỉ xem của mình)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN,CUSTOMER,GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(int id)
        {
            try
            {
                // Kiểm tra quyền truy cập
                if (!UserContextHelper.CanAccessUserResource(User, id))
                {
                    return StatusCode(403, ApiResponse<object>.BadRequest("Bạn không có quyền xem thông tin user này"));
                }

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
        /// Cập nhật thông tin user (ADMIN có thể cập nhật tất cả, user khác chỉ cập nhật của mình)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,CUSTOMER,GARAGE")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateUser(int id, [FromBody] UserUpdateDto user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                // Kiểm tra quyền truy cập
                if (!UserContextHelper.CanAccessUserResource(User, id))
                {
                    return StatusCode(403, ApiResponse<object>.BadRequest("Bạn không có quyền cập nhật thông tin user này"));
                }

                await _userRepository.UpdateAsync(user.Adapt<User>());
                return Ok(ApiResponse<object>.Success("Cập nhật user thành công"));
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
        /// Xóa user (Chỉ ADMIN)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
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
