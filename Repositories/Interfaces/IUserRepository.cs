using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> UpdateRefreshTokenAsync(string userId, string refreshToken, DateTime expiryTime);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
    }
}