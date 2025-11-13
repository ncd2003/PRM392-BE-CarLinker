using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Chat
{
    public class HideMessageRequestDTO
    {
        [Required]
        public int SenderId { get; set; }

        [Required]
        public int SenderType { get; set; }
    }
}
