using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(255)]
        public string? Image { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Property
        //public virtual ICollection<Product> Products { get; set; }
    }

}
