using BusinessObjects.Models.Type;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models.DTOs.GarageStaff
{
    public class GarageStaffDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Image { get; set; }
        public RoleGarage GarageRole { get; set; }
        public UserStatus UserStatus { get; set; }
        public bool IsActive { get; set; }
        public int GarageId { get; set; }
        public string? GarageName { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }

    public class GarageStaffCreateDto
    {
        [Required(ErrorMessage = "H? tên là b?t bu?c")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là b?t bu?c")]
        [EmailAddress(ErrorMessage = "Email không h?p l?")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "S? ?i?n tho?i không h?p l?")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "M?t kh?u là b?t bu?c")]
        [MinLength(6, ErrorMessage = "M?t kh?u ph?i có ít nh?t 6 ký t?")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role là b?t bu?c")]
        public RoleGarage GarageRole { get; set; } = RoleGarage.STAFF; // Default: STAFF
    }

    public class GarageStaffUpdateDto
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "S? ?i?n tho?i không h?p l?")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public RoleGarage? GarageRole { get; set; }
        
        public UserStatus? UserStatus { get; set; }
    }

    public class GarageStaffChangePasswordDto
    {
        [Required(ErrorMessage = "M?t kh?u c? là b?t bu?c")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "M?t kh?u m?i là b?t bu?c")]
        [MinLength(6, ErrorMessage = "M?t kh?u ph?i có ít nh?t 6 ký t?")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class GarageStaffLoginDto
    {
        [Required(ErrorMessage = "Email là b?t bu?c")]
        [EmailAddress(ErrorMessage = "Email không h?p l?")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "M?t kh?u là b?t bu?c")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO for staff login
    /// </summary>
    public class GarageStaffLoginResponseDto
    {
        public int StaffId { get; set; }
        public string Email { get; set; } = string.Empty;
        public RoleGarage Role { get; set; }
        public int GarageId { get; set; }
        public string GarageName { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
