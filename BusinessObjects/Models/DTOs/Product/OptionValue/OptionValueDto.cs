using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product.OptionValue
{
    public class OptionValueDto
    {
        public int Id { get; set; } // Id của OptionValue
        public int OptionId { get; set; }
        public string Value { get; set; } // Giá trị: "Đỏ"
    }
}
