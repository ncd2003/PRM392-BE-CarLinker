using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class ChatRoom : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey(nameof(Garage))]
        public int GarageId { get; set; }

        [Required]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Garage Garage { get; set; } = default!;
        public virtual User Customer { get; set; } = default!;
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public virtual ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();
    }
}
