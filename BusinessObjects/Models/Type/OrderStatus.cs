using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public enum OrderStatus
    {
        Pending = 0,      
        Confirmed = 1,    // Đã xác nhận
        Shipping = 2,      // Đang giao
        Delivered = 3,    // Đã giao
        Cancelled = 4,    // Đã hủy
        Failed = 5        // Giao thất bại
    }
}
