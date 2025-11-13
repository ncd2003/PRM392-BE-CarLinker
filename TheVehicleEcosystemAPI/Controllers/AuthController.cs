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
        private readonly IGarageRepository _garageRepository;
        private readonly IGarageStaffRepository _garageStaffRepository;
        private readonly JWTConfiguration _jwtConfiguration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserRepository userRepository,
            IGarageRepository garageRepository,
            IGarageStaffRepository garageStaffRepository,
            JWTConfiguration jwtConfiguration,
            ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _garageRepository = garageRepository;
            _garageStaffRepository = garageStaffRepository;
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
        /// Đăng ký tài khoản partner (Garage Owner)
        /// Tạo User với role GARAGE (Owner) và Garage entity
        /// </summary>
        [HttpPost("partner/register")]
        [ProducesResponseType(typeof(ApiResponse<PartnerRegisterResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PartnerRegisterResponseDto>>> PartnerRegister([FromBody] PartnerRegisterRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                // Kiểm tra email user đã tồn tại
                var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Email người dùng đã được sử dụng"));
                }

                // Kiểm tra email đã tồn tại trong GarageStaff
                var existingStaff = await _garageStaffRepository.GetByEmailAsync(request.Email);
                if (existingStaff != null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Email đã được sử dụng trong hệ thống garage"));
                }

                // Kiểm tra email garage đã tồn tại
                var existingGarageEmailInUser = await _userRepository.GetByEmailAsync(request.GarageEmail);
                if (existingGarageEmailInUser != null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Email gara đã được sử dụng"));
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Tạo User với role GARAGE (Owner)
                var newUser = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = passwordHash,
                    UserRole = RoleUser.GARAGE, // Owner role
                    UserStatus = UserStatus.ACTIVE,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                // Generate tokens for user
                var accessToken = _jwtConfiguration.GenerateJwtToken(newUser);
                var refreshToken = _jwtConfiguration.GenerateRefreshToken(out DateTime refreshTokenExpiry);

                // Lưu refresh token vào user
                newUser.RefreshToken = refreshToken;
                newUser.RefreshTokenExpiryTime = refreshTokenExpiry;

                // Lưu user vào database
                await _userRepository.AddAsync(newUser);

                // Tạo Garage liên kết với User
                var newGarage = new Garage
                {
                    Name = request.GarageName,
                    Email = request.GarageEmail,
                    PhoneNumber = request.GaragePhoneNumber,
                    Description = request.Description ?? string.Empty,
                    OperatingTime = request.OperatingTime,
                    Latitude = request.Latitude ?? string.Empty,
                    Longitude = request.Longitude ?? string.Empty,
                    Image = string.Empty, // Image will be uploaded later
                    IsActive = true,
                    UserId = newUser.Id, // Link to User (Owner)
                    CreatedAt = DateTimeOffset.UtcNow
                };

                // Lưu garage vào database
                await _garageRepository.AddAsync(newGarage);

                // Tạo response
                var response = new PartnerRegisterResponseDto
                {
                    UserId = newUser.Id,
                    UserEmail = newUser.Email,
                    UserRole = newUser.UserRole, // RoleUser.GARAGE
                    GarageId = newGarage.Id,
                    GarageName = newGarage.Name,
                    GarageEmail = newGarage.Email,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };

                _logger.LogInformation("Partner registered successfully: User {UserId} (Role: GARAGE), Garage {GarageId}", 
                    newUser.Id, newGarage.Id);

                return Created("", ApiResponse<PartnerRegisterResponseDto>.Created("Đăng ký partner thành công", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during partner registration");
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi đăng ký partner"));
            }
        }

        /// <summary>
        /// Đăng nhập cho GarageStaff
        /// </summary>
        [HttpPost("staff/login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> StaffLogin([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                // Tìm staff theo email trong bảng GarageStaff
                var staff = await _garageStaffRepository.GetByEmailAsync(request.Email);
                if (staff == null)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Email hoặc mật khẩu không đúng"));
                }

                // Kiểm tra mật khẩu
                if (!BCrypt.Net.BCrypt.Verify(request.Password, staff.PasswordHash))
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Email hoặc mật khẩu không đúng"));
                }

                // Kiểm tra tài khoản có active không
                if (!staff.IsActive)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Tài khoản đã bị vô hiệu hóa"));
                }

                // Kiểm tra trạng thái user
                if (staff.UserStatus != UserStatus.ACTIVE)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Tài khoản chưa được kích hoạt"));
                }

                // Generate tokens cho GarageStaff
                var accessToken = _jwtConfiguration.GenerateJwtTokenForGarageStaff(staff);
                var refreshToken = _jwtConfiguration.GenerateRefreshToken(out DateTime refreshTokenExpiry);

                // Lưu refresh token vào database
                staff.RefreshToken = refreshToken;
                staff.RefreshTokenExpiryTime = refreshTokenExpiry;
                await _garageStaffRepository.UpdateAsync(staff);

                var response = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = new UserInfoDto
                    {
                        Id = staff.Id,
                        Email = staff.Email,
                        Role = staff.GarageRole.ToString(), // DEALER, WAREHOUSE, STAFF
                    }
                };

                _logger.LogInformation("Staff logged in successfully: StaffId {StaffId}, GarageId {GarageId}, Role {Role}", 
                    staff.Id, staff.GarageId, staff.GarageRole);

                return Ok(ApiResponse<LoginResponseDto>.Success("Đăng nhập thành công", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during staff login");
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi đăng nhập"));
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

        /// <summary>
        /// Refresh access token cho GarageStaff
        /// </summary>
        [HttpPost("staff/refresh")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> StaffRefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Dữ liệu không hợp lệ"));
                }

                // Lấy staff id từ expired access token
                var staffId = _jwtConfiguration.GetUserIdFromExpiredToken(request.AccessToken);
                if (staffId == null)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Access token không hợp lệ"));
                }

                // Lấy staff từ database
                var staff = await _garageStaffRepository.GetByIdAsync(staffId.Value);
                if (staff == null || !staff.IsActive)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Staff không tồn tại hoặc đã bị vô hiệu hóa"));
                }

                // Validate refresh token
                var (principal, expiryTimeFromToken) = _jwtConfiguration.ValidateRefreshToken(request.RefreshToken);
                if (principal == null)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Refresh token không hợp lệ hoặc đã hết hạn"));
                }

                // Kiểm tra refresh token trong database
                if (staff.RefreshToken != request.RefreshToken)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Refresh token không khớp"));
                }

                // Kiểm tra thời gian hết hạn
                if (staff.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Refresh token đã hết hạn"));
                }

                // Generate new tokens
                var newAccessToken = _jwtConfiguration.GenerateJwtTokenForGarageStaff(staff);
                var newRefreshToken = _jwtConfiguration.GenerateRefreshToken(out DateTime newRefreshTokenExpiry);

                // Update refresh token in database
                staff.RefreshToken = newRefreshToken;
                staff.RefreshTokenExpiryTime = newRefreshTokenExpiry;
                await _garageStaffRepository.UpdateAsync(staff);

                var response = new LoginResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    User = new UserInfoDto
                    {
                        Id = staff.Id,
                        Email = staff.Email,
                        Role = staff.GarageRole.ToString(),
                    }
                };

                return Ok(ApiResponse<LoginResponseDto>.Success("Làm mới token thành công", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during staff token refresh");
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi làm mới token"));
            }
        }

        /// <summary>
        /// Đăng xuất cho GarageStaff
        /// </summary>
        [HttpPost("staff/logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> StaffLogout()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int staffId))
                {
                    return Unauthorized(ApiResponse<object>.BadRequest("Staff không hợp lệ"));
                }

                var staff = await _garageStaffRepository.GetByIdAsync(staffId);
                if (staff != null)
                {
                    // Xóa refresh token
                    staff.RefreshToken = null;
                    staff.RefreshTokenExpiryTime = null;
                    await _garageStaffRepository.UpdateAsync(staff);
                }

                return Ok(ApiResponse<object>.Success("Đăng xuất thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during staff logout");
                return StatusCode(500, ApiResponse<object>.InternalError("Lỗi khi đăng xuất"));
            }
        }
    }
}
