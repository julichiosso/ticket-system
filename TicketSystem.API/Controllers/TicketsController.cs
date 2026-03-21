using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketSystem.Aplicacion.DTOs;
using TicketSystem.Aplicacion.Interfaces;
using TicketSystem.Dominio.Enumeraciones;

namespace TicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
    {
        var userId = GetUserIdFromToken();
        dto.UserId = userId;

        var id = await _ticketService.CreateAsync(dto);
        return Ok(new { ticketId = id });
    }

    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserIdFromToken();
        var pagedResult = await _ticketService.GetByUserAsync(userId, page, pageSize);
        return Ok(new
        {
            success = true,
            data = pagedResult
        });
    }

    [Authorize(Roles = "Administrator,Operator")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        if (!Enum.IsDefined(typeof(TicketStatus), dto.Status))
            throw new ArgumentException("Invalid status");

        var actorId = GetUserIdFromToken();
        await _ticketService.ChangeStatusAsync(id, (TicketStatus)dto.Status, actorId);
        return NoContent();
    }

    [Authorize(Roles = "Administrator,Operator")]
    [HttpGet]
    public async Task<IActionResult> GetFiltered([FromQuery] TicketFilterDto filter)
    {
        var result = await _ticketService.GetFilteredAsync(filter);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    [Authorize(Roles = "Administrator,Operator")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var actorId = GetUserIdFromToken();
        await _ticketService.DeleteAsync(id, actorId);
        return NoContent();
    }

    [HttpGet("{id}/comments")]
    public async Task<IActionResult> GetComments(Guid id)
    {
        var isAdminOrOperator = User.IsInRole("Administrator") || User.IsInRole("Operator");
        var comments = await _ticketService.GetCommentsAsync(id, includeInternal: isAdminOrOperator);

        return Ok(new
        {
            success = true,
            data = comments
        });
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] CreateTicketCommentDto dto)
    {
        var authorId = GetUserIdFromToken();
        var comment = await _ticketService.AddCommentAsync(id, authorId, dto);

        return Ok(new
        {
            success = true,
            data = comment
        });
    }

    [Authorize(Roles = "Operator,Administrator")]
    [HttpGet("operator/my-tickets")]
    public async Task<IActionResult> GetMyOperatorTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var operatorId = GetUserIdFromToken();
        var pagedResult = await _ticketService.GetByOperatorAsync(operatorId, page, pageSize);
        return Ok(new
        {
            success = true,
            data = pagedResult
        });
    }

    [Authorize(Roles = "Administrator")]
    [HttpPut("{id}/assign")]
    public async Task<IActionResult> AssignOperator(Guid id, [FromBody] AssignOperatorDto dto)
    {
        var actorId = GetUserIdFromToken();
        await _ticketService.AssignOperatorAsync(id, dto.OperatorId, actorId);
        return NoContent();
    }

    private Guid GetUserIdFromToken()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
            throw new UnauthorizedAccessException("User not authenticated");

        return Guid.Parse(claim.Value);
    }
}
