using System;

namespace TicketSystem.Aplicacion.DTOs
{
    public class AssignOperatorDto
    {
        public Guid TicketId { get; set; }
        public Guid? OperatorId { get; set; }
    }
}
