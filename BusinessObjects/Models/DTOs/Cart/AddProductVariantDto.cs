using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Cart
{
    public class AddProductVariantDto
    {
        public int ProductVariantId { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; } = 0;


    }
}
