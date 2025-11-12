using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models.DTOs.Booking
{
    public class CreateBookingRequestDto
    {
        [Required(ErrorMessage = "GarageId là bắt buộc")]
        public int GarageId { get; set; }

        [Required(ErrorMessage = "VehicleId là bắt buộc")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Danh sách dịch vụ là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất một dịch vụ")]
        public List<int> ServiceItemIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        public DateTime StartTime { get; set; }
    }
}
