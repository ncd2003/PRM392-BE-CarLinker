using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.User
{
    public class UserCreateDto
    {
        public string? FullName { get; set; }
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string? Password { get; set; }
        public RoleGarage GarageRole { get; set; }
    }
}
