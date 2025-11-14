using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.GarageStaff
{
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
}
