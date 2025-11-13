using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Brand
{
    public class UpdateBrandDto
    {
        [Required(ErrorMessage = "Tên hãng xe không được để trống")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quốc gia không được để trống")]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Image { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
