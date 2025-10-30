namespace BusinessObjects.Models.DTOs.Vehicle
{
    /// <summary>
    /// Vehicle Response DTO
    /// </summary>
    public class VehicleDto
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string? FuelType { get; set; }
        public string? TransmissionType { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }

        public string Image { get; set; }
    }
}
