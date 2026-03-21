using TicketSystem.Aplicacion.DTOs;

namespace TicketSystem.Aplicacion.Interfaces
{
    public interface IUserService
    {
        Task<Guid> CreateAsync(CreateUserDto dto);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(Guid id);
        Task DeleteAsync(Guid id);
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    }
}
