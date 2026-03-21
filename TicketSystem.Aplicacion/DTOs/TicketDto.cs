using System;
using TicketSystem.Dominio.Enumeraciones;

namespace TicketSystem.Aplicacion.DTOs
{
    public class TicketDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public bool SlaComplied { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public Guid? AssignedOperatorId { get; set; }
        public string? AssignedOperatorName { get; set; }
    }
}
