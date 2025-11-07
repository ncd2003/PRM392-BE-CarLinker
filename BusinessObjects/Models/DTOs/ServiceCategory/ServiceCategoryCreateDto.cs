using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.ServiceCategory
{
    public class ServiceCategoryCreateDto
    {
        public string? Name { get; set; }

        public List<int>? ServiceItems { get; set; }
    }
}
