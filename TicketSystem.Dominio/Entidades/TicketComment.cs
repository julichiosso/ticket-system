using System;
using System.Collections.Generic;

namespace TicketSystem.Dominio.Entidades
{
    public class TicketComment
    {
        public Guid Id { get; set; }

        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public Guid AuthorId { get; set; }
        public User Author { get; set; } = null!;

        public string Message { get; set; } = null!;
        public bool IsInternal { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
