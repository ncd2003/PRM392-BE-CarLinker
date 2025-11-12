using BusinessObjects.Models.DTOs.Garage;
using BusinessObjects.Models.DTOs.ServiceItem;
using BusinessObjects.Models.DTOs.Vehicle;
using BusinessObjects.Models.Type;

namespace BusinessObjects.Models.DTOs.Booking
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public ServiceRecordStatus Status { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;

        public VehicleDto? Vehicle { get; set; }
        public GarageDto? Garage { get; set; }
        public List<ServiceItemDto> ServiceItems { get; set; } = new List<ServiceItemDto>();
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
