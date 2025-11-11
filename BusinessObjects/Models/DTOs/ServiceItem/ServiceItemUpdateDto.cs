using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.ServiceItem
{
    public class ServiceItemUpdateDto
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
    }
}
