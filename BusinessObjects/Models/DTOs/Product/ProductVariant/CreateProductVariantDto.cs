using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product.ProductVariant
{
    public class CreateProductVariantDto
    {
        // --- Các trường bạn đã có ---
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        public int StockQuantity { get; set; } = 0;
        public bool IsDefault { get; set; } = false;
        // ... (Thêm Weight, Dimensions... nếu cần) ...

        // --- TRƯỜNG MỚI QUAN TRỌNG ---

        /// <summary>
        /// Danh sách các ID của OptionValue được chọn.
        /// Ví dụ: [50, 60] (cho "128GB" và "Đen")
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất một giá trị thuộc tính.")]
        public List<int> SelectedOptionValueIds { get; set; }
    }
}
