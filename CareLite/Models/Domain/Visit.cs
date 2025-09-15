using System;

namespace CareLite.Models.Domain
{
    public class Visit
    {
        public int VisitId { get; set; }
        public int AppointmentId { get; set; }
        public int ClinicianId { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsFinalized { get; set; }
    }
}
