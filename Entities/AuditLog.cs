using System;

namespace PatientsApi.Entities
{
    public class AuditLog
    {
        public long AuditLogId { get; set; }
        public string Entity { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Username { get; set; } = "System"; // valor seguro por defecto
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Changes { get; set; }
    }
}

