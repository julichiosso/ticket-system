using Microsoft.AspNetCore.Mvc;
using TicketSystem.Aplicacion.Interfaces;
using TicketSystem.Dominio.Entidades;
using TicketSystem.Dominio.Enumeraciones;

namespace TicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public SeedController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("create-operator")]
    public async Task<IActionResult> CreateOperator([FromBody] CreateOperatorRequest request, [FromServices] IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
            return NotFound();

        try
        {
            var @operator = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Role = UserRole.Operator,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _userRepository.CreateAsync(@operator);

            return Ok(new
            {
                success = true,
                message = $"Operator {@operator.Name} created successfully",
                id = @operator.Id
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}

public class CreateOperatorRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
