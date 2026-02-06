using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        public RoleRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _collection.Find(x => x.Name == name && !x.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Role>> GetRolesByIdsAsync(List<string> roleIds)
        {
            return await _collection.Find(x => roleIds.Contains(x.Id) && !x.IsDeleted).ToListAsync();
        }
    }
}