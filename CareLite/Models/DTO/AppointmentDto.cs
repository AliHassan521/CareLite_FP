using System;

namespace CareLite.Models.DTO
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
