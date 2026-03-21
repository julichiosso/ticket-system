using TicketSystem.Aplicacion.DTOs;
using TicketSystem.Dominio.Entidades;

namespace TicketSystem.Aplicacion.Interfaces
{
    public interface ITicketRepository
    {
        Task CreateAsync(Ticket ticket);
        Task<(IEnumerable<Ticket> Tickets, int Total)> GetByUserAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<(IEnumerable<Ticket> Tickets, int Total)> GetByOperatorAsync(Guid operatorId, int page = 1, int pageSize = 10);
        Task<Ticket?> GetByIdAsync(Guid id);
        IQueryable<Ticket> GetQueryable();
        Task<(IEnumerable<Ticket> Tickets, int Total)> GetFilteredAsync(TicketFilterDto filter);
        Task SaveChangesAsync();
        Task UpdateAsync(Ticket ticket);

        Task<IEnumerable<TicketComment>> GetCommentsByTicketAsync(Guid ticketId, bool includeInternal);
        Task AddCommentAsync(TicketComment comment);
        Task RecordAuditLogAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync();
        
    }
}
