using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class AssignmentHistoryRepository : IAssignmentHistoryRepository
    {
        private readonly IMongoCollection<EmployeeAssignmentHistory> _collection;

        public AssignmentHistoryRepository(IMongoDbContext context)
        {
            _collection = context.GetCollection<EmployeeAssignmentHistory>("employeeassignmenthistories");
        }

        public async Task<EmployeeAssignmentHistory> CreateAsync(EmployeeAssignmentHistory entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<List<EmployeeAssignmentHistory>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _collection
                .Find(x => x.EmployeeId == employeeId && !x.IsDeleted)
                .SortByDescending(x => x.ChangedAt)
                .ToListAsync();
        }

        public async Task<EmployeeAssignmentHistory?> GetByIdAsync(string id)
        {
            return await _collection
                .Find(x => x.Id == id && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }
    }
}