using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class OptionValue : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("ProductOption")]
        public int OptionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Value { get; set; } = null!;   // Ví dụ: Đỏ, Xanh, 195/65R15

        // Navigation Property
        public virtual ProductOption? ProductOption { get; set; }
        public virtual ICollection<ProductVariantOption> ProductVariantOptions { get; set; }  
    }
}
