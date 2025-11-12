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
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(ServiceCategoryId))]
        public int? ServiceCategoryId { get; set; }
        public virtual ServiceCategory? ServiceCategory { get; set; } = default!;

        public ICollection<GarageServiceItem> GarageServiceItems { get; set; } = new List<GarageServiceItem>();

        public int? ServiceRecordId { get; set; }

    }
}
