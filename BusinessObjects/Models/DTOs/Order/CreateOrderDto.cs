using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
        [MaxLength(100)]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email người nhận.")]
        [MaxLength(100)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        [StringLength(255)]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(50)]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } // Ví dụ: "COD", "VNPay"
    }
}
