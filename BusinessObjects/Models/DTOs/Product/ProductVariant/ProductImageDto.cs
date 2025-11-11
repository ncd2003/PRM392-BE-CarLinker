using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product
{
    public class ProductImageDto
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; }
        public int ProductId { get; set; }

        public bool IsFeatured { get; set; } = false; 
    }
}
