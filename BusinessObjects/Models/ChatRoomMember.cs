using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class ChatRoomMember : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey(nameof(ChatRoom))]
        public long RoomId { get; set; }

        [Required]
        public SenderType UserType { get; set; } // STAFF or ADMIN

        [Required]
        public int UserId { get; set; } // References GarageStaff.Id or Admin ID

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ChatRoom ChatRoom { get; set; } = default!;
    }
}
