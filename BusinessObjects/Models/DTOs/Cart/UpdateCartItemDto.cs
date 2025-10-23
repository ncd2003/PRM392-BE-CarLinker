using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Cart
{
    public class UpdateCartItemDto
    {
        [Required(ErrorMessage = "ProductVariantId là bắt buộc")]
        public int ProductVariantId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int NewQuantity { get; set; }
    }
}
