using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Index(nameof(UserId), IsUnique = true)] // Mỗi User chỉ có 1 Cart duy nhất
    public class Cart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Liên kết tới người dùng sở hữu giỏ hàng
        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;


        // Một giỏ hàng có nhiều mặt hàng
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    }
}
