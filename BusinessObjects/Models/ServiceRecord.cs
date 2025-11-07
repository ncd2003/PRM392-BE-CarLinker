using BusinessObjects.Models.Type;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class ServiceRecord : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public ServiceRecordStatus ServiceRecordStatus { get; set; } = ServiceRecordStatus.PENDING;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalCost { get; set; }

        [Required]
        public DateTime StartTime { get; set; } = DateTime.Now;

        public DateTime? EndTime { get; set; }  // Nullable vì có thể chưa hoàn thành

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public virtual User User { get; set; } = default!;

        [Required]
        [ForeignKey(nameof(Vehicle))]
        public int VehicleId { get; set; }
        public virtual Vehicle Vehicle { get; set; } = default!;
        public virtual ICollection<ServiceItem> ServiceItems { get; set; } = new List<ServiceItem>();
    }
}
