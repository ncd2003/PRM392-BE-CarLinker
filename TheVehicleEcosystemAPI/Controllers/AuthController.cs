using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Auth;
using BusinessObjects.Models.Type;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using System.Data;
using TheVehicleEcosystemAPI.Response.DTOs;
using TheVehicleEcosystemAPI.Security;

namespace TheVehicleEcosystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly JWTConfiguration _jwtConfiguration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserRepository userRepository,
            JWTConfiguration jwtConfiguration,
            ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _jwtConfiguration = jwtConfiguration;
            _logger = logger;
        }
        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                // Kiểm tra email đã tồn tại
                var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Email đã được sử dụng"));
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Tạo user mới với Mapster và set các giá trị cần thiết
                var newUser = request.Adapt<User>();
                newUser.PasswordHash = passwordHash;
                newUser.UserStatus = UserStatus.ACTIVE;
                newUser.IsActive = true;
                newUser.CreatedAt = DateTimeOffset.UtcNow;

                // Generate tokens
                var accessToken = _jwtConfiguration.GenerateJwtToken(newUser);
                var refreshToken = _jwtConfiguration.GenerateRefreshToken(out DateTime refreshTokenExpiry);

                // Lưu refresh token vào user
                newUser.RefreshToken = refreshToken;
                newUser.RefreshTokenExpiryTime = refreshTokenExpiry;

                await _userRepository.AddAsync(newUser);

                var response = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    //AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
                    User = new UserInfoDto
                    {
                        Id = newUser.Id,
                        Email = newUser.Email,
                        Role = newUser.UserRole.ToString(),
                    }
                };

                return Created("", ApiResponse<LoginResponseDto>.Created("Đăng ký thành công", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi đăng ký tài khoản"));
            }
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                // Tìm user theo email
                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Email hoặc mật khẩu không đúng"));
                }

                // Kiểm tra mật khẩu
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Email hoặc mật khẩu không đúng"));
                }

                // Kiểm tra tài khoản có active không
                if (!user.IsActive)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Tài khoản đã bị vô hiệu hóa"));
                }

                // Generate tokens
                var accessToken = _jwtConfiguration.GenerateJwtToken(user);
                var refreshToken = _jwtConfiguration.GenerateRefreshToken(out DateTime refreshTokenExpiry);

                // Lưu refresh token vào database
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = refreshTokenExpiry;
                await _userRepository.UpdateAsync(user);

                var response = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    //AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Role = user.UserRole.ToString(),
                    }
                };

                return Ok(ApiResponse<LoginResponseDto>.Success("Đăng nhập thành công", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi đăng nhập"));
            }
        }

        /// <summary>
        /// Refresh access token bằng refresh token
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                // Lấy user id từ expired access token
                var userId = _jwtConfiguration.GetUserIdFromExpiredToken(request.AccessToken);
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Access token không hợp lệ"));
                }

                // Lấy user từ database
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("User không tồn tại hoặc đã bị vô hiệu hóa"));
                }

                // Validate refresh token từ trong token
                var (principal, expiryTimeFromToken) = _jwtConfiguration.ValidateRefreshToken(request.RefreshToken);
                if (principal == null)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Refresh token không hợp lệ hoặc đã hết hạn"));
                }

                // Kiểm tra refresh token trong database
                if (user.RefreshToken != request.RefreshToken)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Refresh token không khớp"));
                }

                // Kiểm tra thời gian hết hạn (double check với database)
                if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Refresh token đã hết hạn"));
                }

                // Generate new tokens
                var newAccessToken = _jwtConfiguration.GenerateJwtToken(user);
                var newRefreshToken = _jwtConfiguration.GenerateRefreshToken(out DateTime newRefreshTokenExpiry);

                // Update refresh token in database
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = newRefreshTokenExpiry;
                await _userRepository.UpdateAsync(user);

                var response = new LoginResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    //AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Role = user.UserRole.ToString(),
                    }
                };

                return Ok(ApiResponse<LoginResponseDto>.Success("Làm mới token thành công", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi làm mới token"));
            }
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("User không hợp lệ"));
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    // Xóa refresh token
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;
                    await _userRepository.UpdateAsync(user);
                }

                return Ok(ApiResponse<object>.Success("Đăng xuất thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi đăng xuất"));
            }
        }

        /// <summary>
        /// Lấy thông tin user hiện tại
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserInfoDto>>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("User không hợp lệ"));
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.NotFound("User không tồn tại"));
                }

                var userInfo = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.UserRole.ToString(),
                };

                return Ok(ApiResponse<UserInfoDto>.Success("Lấy thông tin user thành công", userInfo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi lấy thông tin user"));
            }
        }
    }
}
