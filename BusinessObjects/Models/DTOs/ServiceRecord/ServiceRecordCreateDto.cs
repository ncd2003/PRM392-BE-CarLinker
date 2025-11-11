using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.ServiceRecord
{
    public class ServiceRecordCreateDto
    {
        public int VehicleId { get; set; }
        public List<int>? ServiceItems { get; set; }
    }
}
