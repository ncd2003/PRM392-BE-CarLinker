using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Product : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Required]
        [ForeignKey("Manufacturer")]
        public int ManufacturerId { get; set; }

        [Required]
        [ForeignKey("Brand")]
        public int BrandId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public string? Description { get; set; }


        [Required]
        public int WarrantyPeriod { get; set; } = 0;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsFeatured { get; set; } = false;


        // Navigation Properties
        public virtual Category Category { get; set; }
        public virtual Manufacturer Manufacturer { get; set; }
        public virtual Brand Brand { get; set; }

        // Collection navigation properties
        public virtual ICollection<ProductVariant> ProductVariants { get; set; }
        public virtual ICollection<ProductOption> ProductOptions { get; set; }

        public virtual ICollection<ProductImage> ProductImages { get; set; }
    }
}
