using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.GarageStaff
{
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
}
