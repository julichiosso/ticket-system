using TicketSystem.Dominio.Enumeraciones;

namespace TicketSystem.Aplicacion.DTOs
{
    public class CreateTicketDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Guid UserId { get; set; }
        public TicketPriority Priority { get; set; }
    }
}
