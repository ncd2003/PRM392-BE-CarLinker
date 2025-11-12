using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Garage : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        public string Description { get; set; }

        [Required]
        public string OperatingTime { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public string Image { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(UserId))]
        public int UserId { get; set; }
        public virtual User User { get; set; } = default!;
        // Navigation Property cho mối quan hệ N-N
        public virtual ICollection<GarageServiceItem> GarageServiceItems { get; set; } = new List<GarageServiceItem>();
        public virtual ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
    }
}
