using System;

namespace CareLite.Models.Domain
{
    public class AppointmentStatusHistory
    {
        public int HistoryId { get; set; }
        public int AppointmentId { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public int? ChangedBy { get; set; }
    }
}
