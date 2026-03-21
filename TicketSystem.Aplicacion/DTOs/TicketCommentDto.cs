using System;
using System.Collections.Generic;

namespace TicketSystem.Aplicacion.DTOs
{
    public class TicketCommentDto
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Guid AuthorId { get; set; }
        public string Author { get; set; } = string.Empty;
        public int Role { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<AttachmentDto> Attachments { get; set; } = new();
    }
}
