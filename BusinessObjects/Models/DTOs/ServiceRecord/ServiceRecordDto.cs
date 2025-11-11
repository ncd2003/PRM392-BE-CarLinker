using BusinessObjects.Models.DTOs.ServiceItem;
using BusinessObjects.Models.DTOs.User;
using BusinessObjects.Models.DTOs.Vehicle;
using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.ServiceRecord
{
    public class ServiceRecordDto
    {
        public int Id { get; set; }
        public ServiceRecordStatus ServiceRecordStatus { get; set; }
        public decimal? TotalCost { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        // Navigation properties - tên phải khớp với entity
        public UserDto? User { get; set; }
        public VehicleDto? Vehicle { get; set; }
        public List<ServiceItemDto>? ServiceItems { get; set; }
    }
}
