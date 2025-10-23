using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Category : BaseModel
    {

        [Key] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        
        [StringLength(255)]
        public string? Image { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Property
        public virtual ICollection<Product> Products { get; set; }
    }
}
