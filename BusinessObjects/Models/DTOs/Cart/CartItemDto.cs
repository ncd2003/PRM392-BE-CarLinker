using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Cart
{
    public class CartItemDto
    {
        [Required(ErrorMessage = "ProductVariantId là bắt buộc")]
        public int ProductVariantId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }
}
