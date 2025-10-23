using System.Security.Claims;

namespace TheVehicleEcosystemAPI.Middleware
{
    /// <summary>
    /// Middleware ?? log thông tin user và validate role
    /// </summary>
    public class RoleValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleValidationMiddleware> _logger;

        public RoleValidationMiddleware(RequestDelegate next, ILogger<RoleValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == ClaimTypes.NameIdentifier)?.Value;
                var userEmail = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var userRole = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var userName = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                // Log request v?i thông tin user
                _logger.LogInformation(
                    "Request: {Method} {Path} - User: {UserId} ({UserEmail}) - Role: {Role}",
                    context.Request.Method,
                    context.Request.Path,
                    userId,
                    userEmail,
                    userRole
                );

                // Thêm thông tin user vào HttpContext.Items ?? d? truy c?p
                context.Items["UserId"] = userId;
                context.Items["UserEmail"] = userEmail;
                context.Items["UserRole"] = userRole;
                context.Items["UserName"] = userName;
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extension method ?? ??ng ký middleware
    /// </summary>
    public static class RoleValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleValidationMiddleware>();
        }
    }
}
