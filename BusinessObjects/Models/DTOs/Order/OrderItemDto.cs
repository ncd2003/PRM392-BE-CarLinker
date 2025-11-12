using BusinessObjects.Models.DTOs.Product;
using BusinessObjects.Models.DTOs.Product.ProductVariant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Order
{
    public class OrderItemDto
    {
        public virtual ProductVariantDto ProductVariant { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        

    }
}
