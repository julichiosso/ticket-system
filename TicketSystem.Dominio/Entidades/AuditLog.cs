using System;

namespace TicketSystem.Dominio.Entidades
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid? TicketId { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; } = null!;
        public string Detail { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
