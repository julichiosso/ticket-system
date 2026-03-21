using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TicketSystem.Aplicacion.DTOs;
using TicketSystem.Aplicacion.Interfaces;

namespace TicketSystem.API.Hubs;

[Authorize]
public class TicketHub : Hub
{
    private readonly ITicketService _ticketService;

    public TicketHub(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public async Task JoinTicket(string ticketId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
    }

    public async Task LeaveTicket(string ticketId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
    }

    public async Task SendMessage(string ticketId, string message, bool isInternal)
    {
        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? Context.User?.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authorId))
            throw new HubException("Invalid token.");

        if (!Guid.TryParse(ticketId, out var ticketGuid))
            throw new HubException("Invalid ticket.");

        var dto = new CreateTicketCommentDto
        {
            Message = message,
            IsInternal = isInternal
        };

        var comment = await _ticketService.AddCommentAsync(ticketGuid, authorId, dto);

        await Clients.Group($"ticket-{ticketId}").SendAsync("NewMessage", comment);
    }

    public async Task NotifyTyping(string ticketId)
    {
        await Clients.Group($"ticket-{ticketId}").SendAsync("UserTyping");
    }
}
