using TicketSystem.Dominio.Enumeraciones;

namespace TicketSystem.Aplicacion.DTOs;

public class TicketFilterDto
{
    public TicketStatus? Status { get; set; }
    public string? Title { get; set; }
    public TicketPriority? Priority { get; set; }
    public Guid? OperatorId { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; }
    public bool Descending { get; set; } = false;
}
