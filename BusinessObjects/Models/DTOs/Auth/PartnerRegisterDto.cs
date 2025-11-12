using BusinessObjects.Models.Type;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models.DTOs.Auth
{
    /// <summary>
    /// DTO for partner (Garage Owner) registration
    /// </summary>
    public class PartnerRegisterRequestDto
    {
        // Personal Information
        [Required(ErrorMessage = "H? tên là b?t bu?c")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là b?t bu?c")]
        [EmailAddress(ErrorMessage = "Email không h?p l?")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "S? ?i?n tho?i là b?t bu?c")]
        [Phone(ErrorMessage = "S? ?i?n tho?i không h?p l?")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "M?t kh?u là b?t bu?c")]
        [MinLength(6, ErrorMessage = "M?t kh?u ph?i có ít nh?t 6 ký t?")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nh?n m?t kh?u là b?t bu?c")]
        [Compare(nameof(Password), ErrorMessage = "M?t kh?u xác nh?n không kh?p")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Garage Information
        [Required(ErrorMessage = "Tên gara là b?t bu?c")]
        [MaxLength(200)]
        public string GarageName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email gara là b?t bu?c")]
        [EmailAddress(ErrorMessage = "Email gara không h?p l?")]
        [MaxLength(100)]
        public string GarageEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "S? ?i?n tho?i gara là b?t bu?c")]
        [Phone(ErrorMessage = "S? ?i?n tho?i gara không h?p l?")]
        [MaxLength(20)]
        public string GaragePhoneNumber { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Gi? ho?t ??ng là b?t bu?c")]
        [MaxLength(100)]
        public string OperatingTime { get; set; } = "8:00 - 18:00";

        [MaxLength(50)]
        public string? Latitude { get; set; }

        [MaxLength(50)]
        public string? Longitude { get; set; }
    }

    /// <summary>
    /// Response DTO for partner registration
    /// Uses RoleUser for Garage Owner
    /// </summary>
    public class PartnerRegisterResponseDto
    {
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public RoleUser UserRole { get; set; } // Use RoleUser.GARAGE for owner
        
        public int GarageId { get; set; }
        public string GarageName { get; set; } = string.Empty;
        public string GarageEmail { get; set; } = string.Empty;
        
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
