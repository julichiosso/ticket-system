using TicketSystem.Aplicacion.Common;
using TicketSystem.Aplicacion.DTOs;
using TicketSystem.Dominio.Enumeraciones;
using TicketSystem.Dominio.Entidades;

namespace TicketSystem.Aplicacion.Interfaces
{
    public interface ITicketService
    {
        Task<Guid> CreateAsync(CreateTicketDto dto);
        Task<PagedResult<TicketDto>> GetByUserAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<PagedResult<TicketDto>> GetByOperatorAsync(Guid operatorId, int page = 1, int pageSize = 10);
        Task ChangeStatusAsync(Guid ticketId, TicketStatus newStatus, Guid actorId);
        Task<PagedResult<TicketDto>> GetFilteredAsync(TicketFilterDto filter);
        Task DeleteAsync(Guid id, Guid actorId);
        Task AssignOperatorAsync(Guid ticketId, Guid? operatorId, Guid actorId);

        Task<IEnumerable<TicketCommentDto>> GetCommentsAsync(Guid ticketId, bool includeInternal);
        Task<TicketCommentDto> AddCommentAsync(Guid ticketId, Guid authorId, CreateTicketCommentDto dto);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync();
        Task<MetricsDto> GetMetricsAsync();
    }
}
