using System;

namespace PatientsApi.Entities
{
    public class AuditLog
    {
        public long AuditLogId { get; set; }
        public string Entity { get; set; }
        public int EntityId { get; set; }
        public string Action { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Changes { get; set; }
    }
}
