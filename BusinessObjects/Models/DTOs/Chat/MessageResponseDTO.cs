using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Chat
{
    public class MessageResponseDTO
    {
        public long Id { get; set; }
        public long RoomId { get; set; }
        public SenderType SenderType { get; set; }
        public int SenderId { get; set; }
        public string? SenderName { get; set; } // Display name for UI
        public string? Message { get; set; }
        public MessageType MessageType { get; set; }
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
        public MessageStatus Status { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
