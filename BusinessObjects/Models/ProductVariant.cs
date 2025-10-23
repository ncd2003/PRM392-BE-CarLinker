using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class ProductVariant : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [StringLength(255)]
        public string Name { get; set; }          // Tên biến thể (ví dụ: iPhone 14 - 128GB)

        [Required]
        [Column(TypeName = "decimal(18,2)")]  
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Weight { get; set; }       // Cân nặng (kg)

        [StringLength(50)]
        public string? SKU { get; set; }

        public int StockQuantity { get; set; } = 0;

        public int HoldQuantity { get; set; } = 0;

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? Dimensions { get; set; }    // Kích thước: DxRxC

        public bool IsDefault { get; set; } = false;  // Biến thể mặc định?
        public bool IsActive { get; set; } = true;    // Còn hoạt động?

        // Navigation property
        public virtual Product? Product { get; set; }
        public virtual ICollection<ProductVariantOption> ProductVariantOptions { get; set; }

        // --- 1 variant có thể nằm trong nhiều CartItem ---
        public virtual ICollection<CartItem>? CartItems { get; set; }

        // --- 1 variant có thể nằm trong nhiều OrderItem ---
        public virtual ICollection<OrderItem>? OrderItems { get; set; }



    }
}
