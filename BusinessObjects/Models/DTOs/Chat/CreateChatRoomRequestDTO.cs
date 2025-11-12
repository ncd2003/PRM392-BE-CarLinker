using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Chat
{
    public class CreateChatRoomRequestDTO
    {
        [Required]
        public int GarageId { get; set; }

        [Required]
        public int CustomerId { get; set; }
    }
}
