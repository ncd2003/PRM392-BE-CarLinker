using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Service
{
    public class ServiceCreateDto
    {
        public string Name { get; set; }
        public double PriceFrom { get; set; }
        public double PriceTo { get; set; }
    }
}
