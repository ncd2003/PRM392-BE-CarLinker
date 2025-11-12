using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class GarageServiceItem
    {
        // 1. Các Khóa ngoại (tạo thành Khóa chính kép)
        public int GarageId { get; set; }
        public int ServiceItemId { get; set; }

        // 2. Thuộc tính riêng của mối quan hệ
        // Ví dụ: Giá mà Gara này đặt cho ServiceItem này
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;

        // 3. Navigation Properties
        public virtual Garage Garage { get; set; } = default!;
        public virtual ServiceItem ServiceItem { get; set; } = default!;
    }
}
