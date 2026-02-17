using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _collection.Find(x => x.Username == username && !x.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _collection.Find(x => x.Email == email && !x.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateRefreshTokenAsync(string userId, string refreshToken, DateTime expiryTime)
        {
            var update = Builders<User>.Update
                .Set(x => x.RefreshToken, refreshToken)
                .Set(x => x.RefreshTokenExpiryTime, expiryTime)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(x => x.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _collection.Find(x => x.RefreshToken == refreshToken && !x.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmployeeIdAsync(string employeeId)
        {
            return await _collection.Find(x => x.EmployeeId == employeeId && !x.IsDeleted).FirstOrDefaultAsync();
        }

        // Explicit implementation to match interface return type
        public new async Task<List<User>> GetAllAsync()
        {
            var users = await _collection.Find(x => !x.IsDeleted).ToListAsync();
            return users;
        }
    }
}