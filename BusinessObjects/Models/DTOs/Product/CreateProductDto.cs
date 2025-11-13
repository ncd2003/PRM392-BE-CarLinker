using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "CategoryId là bắt buộc")]
        public int CategoryId { get; set; }


        [Required(ErrorMessage = "BrandId là bắt buộc")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Tên sản phẩm không được vượt quá 255 ký tự")]
        public string Name { get; set; }

        public string? Description { get; set; }

        //[StringLength(255)]
        //public string? Image { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thời gian bảo hành phải >= 0")]
        public int WarrantyPeriod { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;
    }
}
