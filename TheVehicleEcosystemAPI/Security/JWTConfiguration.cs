using BusinessObjects.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TheVehicleEcosystemAPI.Security
{
    public class JWTConfiguration
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;

        public JWTConfiguration(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _contextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Generate JWT Access Token - Chỉ chứa id, email và role
        /// </summary>
        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // ✨ Chỉ lưu id, email và role trong claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserRole.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiresInHours"] ?? "1")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generate JWT Access Token for GarageStaff - Chứa id, email, role và garageId
        /// </summary>
        public string GenerateJwtTokenForGarageStaff(GarageStaff staff)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // ✨ Lưu id, email, role (GarageRole) và garageId trong claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, staff.Id.ToString()),
                new Claim("userId", staff.Id.ToString()), // Alias for compatibility
                new Claim(ClaimTypes.Email, staff.Email),
                new Claim(ClaimTypes.Role, staff.GarageRole.ToString()),
                new Claim("garageId", staff.GarageId.ToString()),
                new Claim("userType", "GarageStaff") // Để phân biệt với User
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiresInHours"] ?? "1")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generate Refresh Token với expire time được lưu trong chính token
        /// </summary>
        public string GenerateRefreshToken(out DateTime expiryTime)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:RefreshTokenKey"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            expiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiresInDays"] ?? "7"));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("exp_time", expiryTime.ToString("O")),
                new Claim("token_type", "refresh")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiryTime,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validate Access Token
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Validate Refresh Token và lấy expire time từ trong token
        /// </summary>
        public (ClaimsPrincipal? principal, DateTime? expiryTime) ValidateRefreshToken(string refreshToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:RefreshTokenKey"]!);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);

                var expTimeClaim = principal.Claims.FirstOrDefault(c => c.Type == "exp_time");
                DateTime? expiryTime = null;

                if (expTimeClaim != null && DateTime.TryParse(expTimeClaim.Value, out DateTime parsedTime))
                {
                    expiryTime = parsedTime;
                }

                return (principal, expiryTime);
            }
            catch
            {
                return (null, null);
            }
        }

        /// <summary>
        /// Get User ID from expired token (for refresh token flow)
        /// </summary>
        public int? GetUserIdFromExpiredToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // ============================================================
        // ✨ HELPER METHODS - Lấy thông tin user từ HttpContext
        // ============================================================

        /// <summary>
        /// Lấy ClaimsPrincipal của user hiện tại
        /// </summary>
        public ClaimsPrincipal? CurrentUser => _contextAccessor.HttpContext?.User;

        /// <summary>
        /// Lấy User ID từ token hiện tại
        /// </summary>
        /// <returns>User ID hoặc null nếu không tìm thấy</returns>
        public int? GetCurrentUserId()
        {
            var user = CurrentUser;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return null;
        }

        /// <summary>
        /// Lấy Email từ token hiện tại
        /// </summary>
        /// <returns>Email hoặc null nếu không tìm thấy</returns>
        public string? GetCurrentUserEmail()
        {
            var user = CurrentUser;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Lấy Role từ token hiện tại
        /// </summary>
        /// <returns>Role (ADMIN, CUSTOMER, GARAGE) hoặc null nếu không tìm thấy</returns>
        public string? GetCurrentUserRole()
        {
            var user = CurrentUser;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Kiểm tra user hiện tại có role ADMIN không
        /// </summary>
        public bool IsAdmin()
        {
            return GetCurrentUserRole() == "ADMIN";
        }

        /// <summary>
        /// Kiểm tra user hiện tại có role CUSTOMER không
        /// </summary>
        public bool IsCustomer()
        {
            return GetCurrentUserRole() == "CUSTOMER";
        }

        /// <summary>
        /// Kiểm tra user hiện tại có role GARAGE không
        /// </summary>
        public bool IsGarage()
        {
            return GetCurrentUserRole() == "GARAGE";
        }

        /// <summary>
        /// Kiểm tra user hiện tại có quyền truy cập resource của user khác không
        /// </summary>
        /// <param name="targetUserId">ID của user cần truy cập</param>
        /// <returns>True nếu có quyền (ADMIN hoặc chính user đó)</returns>
        public bool CanAccessUserResource(int targetUserId)
        {
            if (IsAdmin())
                return true;

            var currentUserId = GetCurrentUserId();
            return currentUserId.HasValue && currentUserId.Value == targetUserId;
        }

        /// <summary>
        /// Lấy tất cả thông tin user từ token hiện tại
        /// </summary>
        /// <returns>Dictionary chứa id, email, role</returns>
        public Dictionary<string, string?> GetCurrentUserInfo()
        {
            return new Dictionary<string, string?>
            {
                ["Id"] = GetCurrentUserId()?.ToString(),
                ["Email"] = GetCurrentUserEmail(),
                ["Role"] = GetCurrentUserRole()
            };
        }

        /// <summary>
        /// Kiểm tra user đã authenticated chưa
        /// </summary>
        public bool IsAuthenticated()
        {
            return CurrentUser?.Identity?.IsAuthenticated == true;
        }
    }
}