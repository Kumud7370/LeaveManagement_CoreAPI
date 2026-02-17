using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);

        Task<User?> GetByEmployeeIdAsync(string employeeId);
        Task<User> CreateAsync(User user);
        Task<bool> UpdateAsync(string id, User user);
        Task<bool> DeleteAsync(string id);
        Task<bool> UpdateRefreshTokenAsync(string userId, string refreshToken, DateTime expiryTime);
        Task<List<User>> GetAllAsync();
    }
}