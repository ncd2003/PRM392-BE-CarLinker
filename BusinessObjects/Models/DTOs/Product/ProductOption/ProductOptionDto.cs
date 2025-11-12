using BusinessObjects.Models.DTOs.Product.OptionValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product.ProductOption
{
    public class ProductOptionDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Unit { get; set; }
        public bool IsRequired { get; set; }
        public List<OptionValueDto> OptionValues { get; set; }
    }
}
