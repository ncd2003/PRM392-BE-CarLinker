using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.User
{
    public class UserUpdateDto
    {
        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public string UserRole { get; set; }
    }
}
