using BusinessObjects.Models.Type;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models.DTOs.Chat
{
    public class AddRoomMemberRequestDTO
    {
        [Required(ErrorMessage = "UserId is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "UserType is required.")]
        public SenderType UserType { get; set; } // STAFF (1) or ADMIN (2)
    }
}
