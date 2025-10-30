using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public enum OrderStatus
    {
        PENDING = 0,      
        CONFIRMED = 1, 
        PACKED = 2,        // Đã xác nhận //dealer
        SHIPPING = 3,      // Đang giao
        DELIVERED = 4,    // Đã giao
        CANCELLED = 5,    // Đã hủy
        FAILED = 6        // Giao thất bại
    }
}
