using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Garage
{
    public class GarageUpdateServiceItem
    {
        [Required(ErrorMessage = "Service Item ID là bắt buộc.")]
        public int ServiceItemId { get; set; }

        [Required(ErrorMessage = "Giá là bắt buộc.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0.")]
        public decimal Price { get; set; }
    }
}
