using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product.ProductVariant
{
    public class UpdateProductVariantDto
    {
        [StringLength(255)]
        public string? Name { get; set; } // Tên (ví dụ: "128GB - Đen")

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        public int StockQuantity { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }
}
