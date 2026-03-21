using TicketSystem.Dominio.Entidades;
using TicketSystem.Dominio.Enumeraciones;

namespace TicketSystem.Aplicacion.Interfaces;

public interface INotificationService
{
    Task NotifyTicketCreatedAsync(Ticket ticket, User user);

    Task NotifyStatusChangeAsync(Ticket ticket, User user, TicketStatus oldStatus, TicketStatus newStatus);

    Task NotifyOperatorAssignmentAsync(Ticket ticket, User @operator);

    Task NotifyNewCommentAsync(Ticket ticket, User recipient, TicketComment comment, User author);
}
