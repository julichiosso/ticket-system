using Microsoft.AspNetCore.Identity;
using TicketSystem.Aplicacion.DTOs;
using TicketSystem.Aplicacion.Interfaces;
using TicketSystem.Dominio.Entidades;

namespace TicketSystem.Aplicacion.Servicios
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            IUserRepository repository,
            IPasswordHasher<User> passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Guid> CreateAsync(CreateUserDto dto)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role
            };

            await _repository.CreateAsync(user);
            return user.Id;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _repository.GetAllAsync();

            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            });
        }

        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new InvalidOperationException("Passwords do not match.");

            if (dto.NewPassword.Length < 6)
                throw new InvalidOperationException("Password must be at least 6 characters.");

            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var result = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, dto.CurrentPassword);

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Current password is incorrect.");

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            await _repository.UpdateAsync(user);
        }
    }
}
