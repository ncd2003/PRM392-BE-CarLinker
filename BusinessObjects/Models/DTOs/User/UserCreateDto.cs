using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.User
{
    public class UserCreateDto
    {
        public string FullName;
        public string Email;
        public string? PhoneNumber;
        public Role UserRole;
    }
}
