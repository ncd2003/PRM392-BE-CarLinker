using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product.ProductVariant
{
    public class ProductVariantOptionDto
    {

        public int VariantId { get; set; }

        public int OptionValueId { get; set; }

    }
}
