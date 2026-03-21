using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.Aplicacion.Interfaces;

namespace TicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class AuditController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public AuditController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await _ticketService.GetAuditLogsAsync();
        return Ok(new { success = true, data = logs });
    }
}
