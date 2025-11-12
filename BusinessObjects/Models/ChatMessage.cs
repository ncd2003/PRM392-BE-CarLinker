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
    public class ChatMessage : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey(nameof(ChatRoom))]
        public long RoomId { get; set; }

        [Required]
        public SenderType SenderType { get; set; } // CUSTOMER, STAFF, ADMIN

        [Required]
        public int SenderId { get; set; } // References the sender's ID

        [MaxLength(4000)]
        public string? Message { get; set; } // Message content for text messages

        [Required]
        public MessageType MessageType { get; set; } = MessageType.TEXT;

        [MaxLength(2000)]
        public string? FileUrl { get; set; } // URL for media files

        public FileType? FileType { get; set; } // image, video, file

        [Required]
        public MessageStatus Status { get; set; } = MessageStatus.ACTIVE;

        public bool IsRead { get; set; } = false;

        // Navigation Properties
        public virtual ChatRoom ChatRoom { get; set; } = default!;
    }
}
