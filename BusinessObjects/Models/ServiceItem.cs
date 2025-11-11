using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class ServiceItem : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public int? ServiceCategoryId { get; set; }

        [ForeignKey(nameof(ServiceCategoryId))]
        public virtual ServiceCategory? ServiceCategory { get; set; } = default!;

        public int? ServiceRecordId { get; set; }

        //[ForeignKey(nameof(ServiceRecordId))]
        //public virtual ServiceRecord? ServiceRecord { get; set; } = default!;
    }
}
