using System;

namespace CareLite.Models.DTO
{
    public class CreateAppointmentRequest
    {
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
    }
}
