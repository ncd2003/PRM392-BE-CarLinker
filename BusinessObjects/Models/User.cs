using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models.Type;

namespace BusinessObjects.Models
{
    public class User : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; } 

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; }

        [MaxLength(50)]
        public Role UserRole { get; set; } = Role.CUSTOMER;

        public UserStatus UserStatus { get; set; }

        [MaxLength(500)]
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        // 🔗 Quan hệ 1–1: Mỗi user chỉ có 1 cart
        public virtual Cart Cart { get; set; }
        // 🔗 Quan hệ 1–N: 1 User có nhiều Order
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
