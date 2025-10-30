using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product
{
    public class ListProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Image { get; set; }

        public int CategoryId { get; set; }

        public int BrandId { get; set; }
        public bool IsDefault { get; set; } = true;
    }
}
