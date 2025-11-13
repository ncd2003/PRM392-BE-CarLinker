using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Chat
{
    public class ChatRoomResponseDTO
    {
        public long Id { get; set; }
        public int GarageId { get; set; }
        public string? GarageName { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime LastMessageAt { get; set; }
        public MessageResponseDTO? LastMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
