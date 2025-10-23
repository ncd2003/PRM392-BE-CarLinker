using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product
{
    public class ProductOptionDto
    {
        public int Id { get; set; } // Id của ProductOption
        public string OptionName { get; set; } // Tên tùy chọn: "Màu sắc"

        // Danh sách các giá trị có thể có: "Đỏ", "Xanh", "Vàng"
        public List<OptionValueDto> Values { get; set; }
    }
}
