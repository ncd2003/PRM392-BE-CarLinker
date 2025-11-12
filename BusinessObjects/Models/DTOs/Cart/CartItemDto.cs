using BusinessObjects.Models.DTOs.Product;
using BusinessObjects.Models.DTOs.Product.ProductVariant;
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
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public ProductVariantDto? ProductVariant { get; set; }
    }
}
