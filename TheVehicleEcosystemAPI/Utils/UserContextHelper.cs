using System.Security.Claims;

namespace TheVehicleEcosystemAPI.Utils
{
    /// <summary>
    /// Helper class ?? l?y thông tin user t? HttpContext (static methods)
    /// </summary>
    public static class UserContextHelper
    {
        /// <summary>
        /// L?y User ID t? claims
        /// </summary>
        public static int? GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// L?y User Email t? claims
        /// </summary>
        public static string? GetUserEmail(ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// L?y User Role t? claims
        /// </summary>
        public static string? GetUserRole(ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Ki?m tra xem user có role ADMIN không
        /// </summary>
        public static bool IsAdmin(ClaimsPrincipal user)
        {
            return GetUserRole(user) == "ADMIN";
        }

        /// <summary>
        /// Ki?m tra xem user có role CUSTOMER không
        /// </summary>
        public static bool IsCustomer(ClaimsPrincipal user)
        {
            return GetUserRole(user) == "CUSTOMER";
        }

        /// <summary>
        /// Ki?m tra xem user có role GARAGE không
        /// </summary>
        public static bool IsGarage(ClaimsPrincipal user)
        {
            return GetUserRole(user) == "GARAGE";
        }

        /// <summary>
        /// Ki?m tra xem user hi?n t?i có quy?n truy c?p resource c?a user khác không
        /// </summary>
        public static bool CanAccessUserResource(ClaimsPrincipal user, int targetUserId)
        {
            if (IsAdmin(user))
            {
                return true;
            }

            var currentUserId = GetUserId(user);
            return currentUserId.HasValue && currentUserId.Value == targetUserId;
        }

        /// <summary>
        /// L?y t?t c? thông tin user d??i d?ng dictionary
        /// </summary>
        public static Dictionary<string, string?> GetUserInfo(ClaimsPrincipal user)
        {
            return new Dictionary<string, string?>
            {
                ["Id"] = GetUserId(user)?.ToString(),
                ["Email"] = GetUserEmail(user),
                ["Role"] = GetUserRole(user)
            };
        }
    }
}
