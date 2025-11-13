using BusinessObjects.Models.Type;

namespace BusinessObjects.Models.DTOs.Chat
{
    public class RoomMemberResponseDTO
    {
        public long Id { get; set; }
        public long RoomId { get; set; }
        public SenderType UserType { get; set; } // STAFF or ADMIN
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty; // Staff or Admin name
        public string? UserEmail { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
