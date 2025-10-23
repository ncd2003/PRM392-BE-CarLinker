using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Vehicle
{
    public class VehicleCreateDto
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string? FuelType { get; set; }
        public string? TransmissionType { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
    }
}
