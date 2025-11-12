using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models.DTOs.Booking
{
    public class AcceptBookingDto
    {
        [Required(ErrorMessage = "Thời gian kết thúc dự kiến là bắt buộc")]
        public DateTime EstimatedEndTime { get; set; }
    }
}
