using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Chat
{
    public class SendMessageRequestDTO
    {
        [Required]
        public long RoomId { get; set; }

        [Required]
        public SenderType SenderType { get; set; } // CUSTOMER, STAFF, ADMIN

        [Required]
        public int SenderId { get; set; }

        public string? Message { get; set; } // For text messages

        public MessageType MessageType { get; set; } = MessageType.TEXT;

        public string? FileUrl { get; set; } // For media messages

        public FileType? FileType { get; set; } // image, video, file
    }
}
