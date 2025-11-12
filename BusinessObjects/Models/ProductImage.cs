using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class ProductImage : BaseModel 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        // THUỘC TÍNH MỚI:
        [Required]
        public bool IsFeatured { get; set; } = false; // Mặc định là 'false'

        // Navigation property
        public virtual Product Product { get; set; }
    }
}
