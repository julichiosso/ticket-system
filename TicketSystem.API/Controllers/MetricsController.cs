using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.Aplicacion.Interfaces;

namespace TicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Operator")]
public class MetricsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public MetricsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await _ticketService.GetMetricsAsync();
        return Ok(new { success = true, data = metrics });
    }
}
