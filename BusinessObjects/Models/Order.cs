using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }

        public string FullName { get; set; }

        public string Enail { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // --- THAY ĐỔI Ở ĐÂY ---
        [Required]
        public OrderStatus Status { get; set; } // Gán giá trị mặc định
        // --- KẾT THÚC THAY ĐỔI ---

        [StringLength(255)]
        public string? PaymentMethod { get; set; }  // "COD", "VNPay", "Momo", ...

        [StringLength(255)]
        public string? ShippingAddress { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        //public bool IsPaid { get; set; } = false;

        // 🔗 1 Order có nhiều OrderItem
        public virtual ICollection<OrderItem> OrderItems { get; set; }

        // (Không bắt buộc) - nếu bạn có bảng User
        public virtual User User { get; set; }
    }
}
