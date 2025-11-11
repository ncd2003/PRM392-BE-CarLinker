using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Vehicle
{
    public class VehicleUpdateDto
    {
        public string? LicensePlate { get; set; }
        public FuelType? FuelType { get; set; }
        public TransmissionType? TransmissionType { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
    }
}
