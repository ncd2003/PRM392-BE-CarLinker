using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product.OptionValue
{
    public class UpdateOptionValueDto
    {
        [Required(ErrorMessage = "Giá trị là bắt buộc")]
        [StringLength(100)]
        public string Value { get; set; } = null!;
    }
}
