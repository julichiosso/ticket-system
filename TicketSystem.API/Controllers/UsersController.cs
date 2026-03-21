using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketSystem.Aplicacion.Interfaces;
using TicketSystem.Aplicacion.DTOs;
using TicketSystem.Dominio.Enumeraciones;

namespace TicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;

    public UsersController(
        IUserRepository userRepository,
        IUserService userService)
    {
        _userRepository = userRepository;
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userRepository.GetAllAsync();
        return Ok(new
        {
            success = true,
            data = users.Select(u => new { u.Id, u.Name, u.Email, u.Role })
        });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        return Ok(new
        {
            success = true,
            data = new { user.Id, user.Name, user.Email, user.Role }
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found");
    
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == id.ToString())
            return BadRequest(new { message = "You cannot delete yourself." });
    
        await _userRepository.DeleteAsync(user);
        return NoContent();
    }

    [HttpPut("roles")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateRoles([FromBody] List<UpdateRoleDto> changes)
    {
        if (changes == null || changes.Count == 0)
            return BadRequest(new { message = "No changes provided." });

        foreach (var change in changes)
        {
            var user = await _userRepository.GetByIdAsync(change.Id);
            if (user == null)
                return NotFound(new { message = $"User {change.Id} not found." });

            if (!Enum.IsDefined(typeof(UserRole), change.Role))
                return BadRequest(new { message = $"Invalid role for user {change.Id}." });

            user.Role = (UserRole)change.Role;
            await _userRepository.UpdateAsync(user);
        }

        return Ok(new { success = true, message = "Roles updated successfully." });
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid token." });

        try
        {
            await _userService.ChangePasswordAsync(userId, dto);
            return Ok(new { success = true, message = "Password updated successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
