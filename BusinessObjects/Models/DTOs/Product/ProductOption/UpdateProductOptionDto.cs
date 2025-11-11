using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product.ProductOption
{
    public class UpdateProductOptionDto
    {
        [Required(ErrorMessage = "Tên thuộc tính là bắt buộc")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Loại thuộc tính là bắt buộc")]
        [StringLength(20)]
        public string Type { get; set; } = "select";

        [StringLength(20)]
        public string? Unit { get; set; }
        public bool IsRequired { get; set; }
    }
}
