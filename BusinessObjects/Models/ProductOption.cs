using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class ProductOption : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;



        [Required]
        [StringLength(20)]
        public string Type { get; set; } = null!;
        // Gồm: "text", "number", "select", "color"

        [StringLength(20)]
        public string? Unit { get; set; }   // Ví dụ: cm, kg, m2

        public bool IsRequired { get; set; } = false;

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public virtual Product Product { get; set; }

        public virtual ICollection<OptionValue> OptionValues { get; set; }
    }
}
