using Microsoft.EntityFrameworkCore;
using TicketSystem.Aplicacion.DTOs;
using TicketSystem.Aplicacion.Interfaces;
using TicketSystem.Dominio.Entidades;
using TicketSystem.Infraestructura.Datos;

namespace TicketSystem.Infraestructura.Repositorios;

public class TicketRepository : ITicketRepository
{
    private readonly TicketSystemDbContext _context;

    public TicketRepository(TicketSystemDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Ticket> Tickets, int Total)> GetByUserAsync(Guid userId, int page = 1, int pageSize = 10)
    {
        var query = _context.Tickets
            .Include(t => t.User)
            .Include(t => t.AssignedOperator)
            .Where(t => t.UserId == userId && !t.IsDeleted);

        var total = await query.CountAsync();
        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (tickets, total);
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        return await _context.Tickets
            .Include(t => t.User)
            .Include(t => t.AssignedOperator)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Ticket> Tickets, int Total)> GetFilteredAsync(TicketFilterDto filter)
    {
        var query = _context.Tickets
            .Include(t => t.User)
            .Include(t => t.AssignedOperator)
            .Where(t => !t.IsDeleted)
            .AsQueryable();

        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority.Value);

        if (filter.UserId.HasValue)
            query = query.Where(t => t.UserId == filter.UserId.Value);

        if (filter.OperatorId.HasValue)
            query = query.Where(t => t.AssignedOperatorId == filter.OperatorId.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);

        if (!string.IsNullOrEmpty(filter.Title))
            query = query.Where(t => t.Title.Contains(filter.Title));

        var total = await query.CountAsync();

        query = filter.SortBy?.ToLower() switch
        {
            "title" => filter.Descending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "status" => filter.Descending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "priority" => filter.Descending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "createdat" => filter.Descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var tickets = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (tickets, total);
    }

    public IQueryable<Ticket> GetQueryable()
    {
        return _context.Tickets.Where(t => !t.IsDeleted).AsQueryable();
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Ticket> Tickets, int Total)> GetByOperatorAsync(Guid operatorId, int page = 1, int pageSize = 10)
    {
        var query = _context.Tickets
            .Include(t => t.User)
            .Include(t => t.AssignedOperator)
            .Where(t => t.AssignedOperatorId == operatorId && !t.IsDeleted);

        var total = await query.CountAsync();
        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (tickets, total);
    }

    public async Task<IEnumerable<TicketComment>> GetCommentsByTicketAsync(Guid ticketId, bool includeInternal)
    {
        var query = _context.TicketComments
            .Include(c => c.Author)
            .Include(c => c.Attachments)
            .Where(c => c.TicketId == ticketId);
        if (!includeInternal)
            query = query.Where(c => !c.IsInternal);

        return await query
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task AddCommentAsync(TicketComment comment)
    {
        await _context.TicketComments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    public async Task RecordAuditLogAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync()
    {
        return await _context.AuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(100)
            .ToListAsync();
    }
}
