using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Order
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public OrderStatus NewStatus { get; set; }
    }
}
