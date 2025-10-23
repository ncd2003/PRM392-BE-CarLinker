using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product
{
    public class ListProductVariantDto
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Image { get; set; }
        public bool IsDefault { get; set; }
    }
}
