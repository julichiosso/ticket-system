using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Security.Cryptography;
using TicketSystem.API.DTOs;
using TicketSystem.Aplicacion.Interfaces;
using TicketSystem.Dominio.Entidades;
using TicketSystem.Dominio.Enumeraciones;

namespace TicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordHasher<User> passwordHasher,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials");

        var token = _tokenService.GenerarToken(user);
        var refreshToken = _tokenService.GenerarRefreshToken();

        var refreshDays = _configuration.GetValue<int>("Jwt:RefreshExpiresInDays", 7);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(refreshDays);
        await _userRepository.UpdateAsync(user);

        return Ok(new
        {
            token,
            refreshToken,
            user = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role
            }
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);

        if (user == null || user.RefreshTokenExpires < DateTime.UtcNow)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        var newToken = _tokenService.GenerarToken(user);
        var newRefreshToken = _tokenService.GenerarRefreshToken();

        var refreshDays = _configuration.GetValue<int>("Jwt:RefreshExpiresInDays", 7);
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(refreshDays);
        await _userRepository.UpdateAsync(user);

        return Ok(new
        {
            token = newToken,
            refreshToken = newRefreshToken
        });
    }

    [HttpPost("register")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);

        if (existingUser != null)
            throw new ArgumentException("User already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Role = UserRole.User
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        await _userRepository.AddAsync(user);

        try
        {
            var frontendBase = _configuration["AppSettings:FrontendBaseUrl"] ?? "http://localhost:5173";
            var body = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <div style='background-color: #2563eb; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                        <h1 style='margin: 0; font-size: 24px;'>TicketSystem</h1>
                    </div>
                    <div style='padding: 20px; background-color: #ffffff;'>
                        <h2 style='color: #333;'>Welcome, {user.Name}!</h2>
                        <p style='color: #555;'>Your account has been created successfully.</p>
                        <p style='color: #555;'>You can now log in and create support tickets.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{frontendBase}' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                Go to System
                            </a>
                        </div>
                        <p style='color: #777; font-size: 12px;'>If you did not create this account, please ignore this message.</p>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(user.Email, "Welcome to TicketSystem!", body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SMTP ERROR] {ex.Message}");
        }

        return Ok(new { message = "User registered successfully" });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null) return Unauthorized();

        var user = await _userRepository.GetByIdAsync(Guid.Parse(id));
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpires = null;
            await _userRepository.UpdateAsync(user);
        }

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new { Id = id, Email = email, Name = name, Role = role });
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request)
    {
        var exists = await _userRepository.GetByEmailAsync(request.Email);
        if (exists != null)
            throw new ArgumentException("User already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Role = UserRole.Administrator
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        await _userRepository.AddAsync(user);

        return Ok(new { message = "Administrator created successfully" });
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return Ok(new { message = "If the email is registered, you will receive a recovery link." });

        var tokenBytes = new byte[16];
        RandomNumberGenerator.Fill(tokenBytes);
        var token = Convert.ToHexString(tokenBytes).ToUpper();

        user.PasswordResetToken = token;
        user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
        await _userRepository.UpdateAsync(user);

        Console.WriteLine($"[DEBUG] Reset Token for {request.Email}: {token}");

        var frontendBase = _configuration["AppSettings:FrontendBaseUrl"] ?? "http://localhost:5173";
        var resetUrl = $"{frontendBase}/reset-password?email={Uri.EscapeDataString(request.Email)}&token={token}";

        var body = $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                <div style='background-color: #2563eb; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                    <h1 style='margin: 0; font-size: 24px;'>TicketSystem</h1>
                </div>
                <div style='padding: 20px; background-color: #ffffff;'>
                    <h2 style='color: #333;'>Password Reset Request</h2>
                    <p style='color: #555;'>Your recovery code is:</p>
                    <div style='background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 20px; font-weight: bold; border-radius: 4px; margin: 20px 0; color: #2563eb; letter-spacing: 2px;'>
                        {token}
                    </div>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetUrl}' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Reset Password</a>
                    </div>
                    <p style='color: #777; font-size: 12px;'>The code will expire in 1 hour.</p>
                </div>
            </div>";

        try
        {
            await _emailService.SendEmailAsync(request.Email, "Password Recovery - TicketSystem", body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SMTP ERROR] {ex.Message}");
        }

        return Ok(new { message = "If the email is registered, you will receive a recovery link." });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { message = "Invalid or expired recovery link." });

        if (user.PasswordResetToken != request.Token)
            return BadRequest(new { message = "Invalid token." });

        if (user.PasswordResetTokenExpires < DateTime.UtcNow)
            return BadRequest(new { message = "The token has expired." });

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpires = null;

        await _userRepository.UpdateAsync(user);

        return Ok(new { message = "Password reset successfully." });
    }
}
