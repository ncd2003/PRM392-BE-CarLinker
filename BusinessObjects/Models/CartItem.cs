using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Index(nameof(CartId), nameof(ProductVariantId), IsUnique = true)]
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Liên kết tới giỏ hàng chứa mặt hàng này
        [Required]
        [ForeignKey(nameof(Cart))]
        public int CartId { get; set; }
        public virtual Cart Cart { get; set; } = null!;

        // Liên kết tới biến thể sản phẩm cụ thể
        [Required]
        [ForeignKey(nameof(ProductVariant))]
        public int ProductVariantId { get; set; }
        public virtual ProductVariant ProductVariant { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        // --- TRƯỜNG MỚI ĐƯỢC THÊM ---
        [Required]
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

    }
}