using TicketSystem.Dominio.Enumeraciones;

namespace TicketSystem.Aplicacion.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public UserRole Role { get; set; }
    }
}
