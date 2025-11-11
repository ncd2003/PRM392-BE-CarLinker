using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Garage
{
    public class GarageDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? OperatingTime { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Image { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }
}
