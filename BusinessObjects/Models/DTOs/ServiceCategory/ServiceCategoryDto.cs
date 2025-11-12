using BusinessObjects.Models.DTOs.ServiceItem;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.ServiceCategory
{
    public class ServiceCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ServiceItemDto>? ServiceItems { get; set; }
    }
}
