using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{

    public class ServiceCategory : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [ForeignKey(nameof(Garage))]
        public int GarageId { get; set; }
        public virtual Garage Garage { get; set; } = default!;
        public virtual ICollection<ServiceItem> ServiceItems { get; set; } = new List<ServiceItem>();
    }
}
