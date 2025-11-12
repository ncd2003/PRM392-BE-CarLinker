namespace BusinessObjects.Models.DTOs.Booking
{
    public class TimeSlotDto
    {
        public DateTime Time { get; set; }
        public bool IsAvailable { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
