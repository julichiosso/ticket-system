using System;
using System.Collections.Generic;
using TicketSystem.Dominio.Enumeraciones;

namespace TicketSystem.Dominio.Entidades
{
    public class Ticket
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Pending;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid? AssignedOperatorId { get; set; }
        public User? AssignedOperator { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public bool SlaComplied { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }
}
