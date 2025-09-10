namespace CareLite.Models.Domain
{
    public class AuditLog
    {
        public int AuditId { get; set; }
        public Guid CorrelationId { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
