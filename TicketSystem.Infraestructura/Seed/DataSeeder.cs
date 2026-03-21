using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketSystem.Dominio.Entidades;
using TicketSystem.Dominio.Enumeraciones;
using TicketSystem.Infraestructura.Datos;

namespace TicketSystem.Infraestructura.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(
        TicketSystemDbContext context,
        IPasswordHasher<User> passwordHasher)
    {
        if (await context.Users.AnyAsync())
            return;

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = "Administrator",
            Email = "admin@ticketsystem.com",
            Role = UserRole.Administrator
        };

        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123");

        var @operator = new User
        {
            Id = Guid.NewGuid(),
            Name = "Demo Operator",
            Email = "operador@ticketsystem.com",
            Role = UserRole.Operator
        };

        @operator.PasswordHash = passwordHasher.HashPassword(@operator, "Operador123");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Demo User",
            Email = "usuario@ticketsystem.com",
            Role = UserRole.User
        };

        user.PasswordHash = passwordHasher.HashPassword(user, "Usuario123");

        await context.Users.AddRangeAsync(admin, @operator, user);
        await context.SaveChangesAsync();
    }
}
