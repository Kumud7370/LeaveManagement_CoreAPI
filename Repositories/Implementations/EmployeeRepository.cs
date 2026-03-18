using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.Employee;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(IMongoDbContext context) : base(context) { }

        public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode)
        {
            return await _collection
                .Find(x => x.EmployeeCode == employeeCode && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _collection
                .Find(x => x.Email == email && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<Employee?> GetByUserIdAsync(string userId)
        {
            return await _collection
                .Find(x => x.UserId == userId && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsEmployeeCodeExistsAsync(string employeeCode, string? excludeId = null)
        {
            var filter = Builders<Employee>.Filter.And(
                Builders<Employee>.Filter.Eq(x => x.EmployeeCode, employeeCode),
                Builders<Employee>.Filter.Eq(x => x.IsDeleted, false)
            );
            if (!string.IsNullOrEmpty(excludeId))
                filter = Builders<Employee>.Filter.And(
                    filter,
                    Builders<Employee>.Filter.Ne(x => x.Id, excludeId));
            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, string? excludeId = null)
        {
            var filter = Builders<Employee>.Filter.And(
                Builders<Employee>.Filter.Eq(x => x.Email, email),
                Builders<Employee>.Filter.Eq(x => x.IsDeleted, false)
            );
            if (!string.IsNullOrEmpty(excludeId))
                filter = Builders<Employee>.Filter.And(
                    filter,
                    Builders<Employee>.Filter.Ne(x => x.Id, excludeId));
            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<(List<Employee> Items, int TotalCount)> GetFilteredEmployeesAsync(EmployeeFilterDto filter)
        {
            var fb = Builders<Employee>.Filter;
            var filters = new List<FilterDefinition<Employee>>
            {
                fb.Eq(x => x.IsDeleted, false)
            };

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var re = new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i");
                filters.Add(fb.Or(
                    fb.Regex(x => x.EmployeeCode, re),
                    fb.Regex(x => x.FirstName, re),
                    fb.Regex(x => x.LastName, re),
                    fb.Regex(x => x.Email, re)));
            }

            if (!string.IsNullOrWhiteSpace(filter.DepartmentId))
                filters.Add(fb.Eq(x => x.DepartmentId, filter.DepartmentId));
            if (!string.IsNullOrWhiteSpace(filter.DesignationId))
                filters.Add(fb.Eq(x => x.DesignationId, filter.DesignationId));
            if (!string.IsNullOrWhiteSpace(filter.ManagerId))
                filters.Add(fb.Eq(x => x.ManagerId, filter.ManagerId));
            if (filter.EmployeeStatus.HasValue)
                filters.Add(fb.Eq(x => x.EmployeeStatus, filter.EmployeeStatus.Value));
            if (filter.EmploymentType.HasValue)
                filters.Add(fb.Eq(x => x.EmploymentType, filter.EmploymentType.Value));
            if (filter.Gender.HasValue)
                filters.Add(fb.Eq(x => x.Gender, filter.Gender.Value));
            if (filter.JoiningDateFrom.HasValue)
                filters.Add(fb.Gte(x => x.DateOfJoining, filter.JoiningDateFrom.Value));
            if (filter.JoiningDateTo.HasValue)
                filters.Add(fb.Lte(x => x.DateOfJoining, filter.JoiningDateTo.Value));

            var combined = fb.And(filters);
            var totalCount = await _collection.CountDocumentsAsync(combined);

            var sb = Builders<Employee>.Sort;
            SortDefinition<Employee> sort = filter.SortBy.ToLower() switch
            {
                "firstname" => filter.SortDescending ? sb.Descending(x => x.FirstName) : sb.Ascending(x => x.FirstName),
                "lastname" => filter.SortDescending ? sb.Descending(x => x.LastName) : sb.Ascending(x => x.LastName),
                "email" => filter.SortDescending ? sb.Descending(x => x.Email) : sb.Ascending(x => x.Email),
                "dateofjoining" => filter.SortDescending ? sb.Descending(x => x.DateOfJoining) : sb.Ascending(x => x.DateOfJoining),
                "createdat" => filter.SortDescending ? sb.Descending(x => x.CreatedAt) : sb.Ascending(x => x.CreatedAt),
                _ => filter.SortDescending ? sb.Descending(x => x.EmployeeCode) : sb.Ascending(x => x.EmployeeCode)
            };

            var items = await _collection
                .Find(combined)
                .Sort(sort)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        public async Task<List<Employee>> GetEmployeesByDepartmentAsync(string departmentId) =>
            await _collection.Find(x => x.DepartmentId == departmentId && !x.IsDeleted).ToListAsync();

        public async Task<List<Employee>> GetEmployeesByManagerAsync(string managerId) =>
            await _collection.Find(x => x.ManagerId == managerId && !x.IsDeleted).ToListAsync();

        public async Task<List<Employee>> GetActiveEmployeesAsync() =>
            await _collection.Find(x => x.EmployeeStatus == EmployeeStatus.Active && !x.IsDeleted).ToListAsync();

        public async Task<int> GetEmployeeCountByStatusAsync(EmployeeStatus status) =>
            (int)await _collection.CountDocumentsAsync(x => x.EmployeeStatus == status && !x.IsDeleted);
        public async Task<long> BulkReassignAsync(
            IEnumerable<string> employeeIds,
            string toDepartmentId,
            string toDesignationId,
            string updatedBy)
        {
            var idList = employeeIds.ToList();
            if (idList.Count == 0) return 0;

            var filter = Builders<Employee>.Filter.And(
                Builders<Employee>.Filter.In(x => x.Id, idList),
                Builders<Employee>.Filter.Eq(x => x.IsDeleted, false)
            );

            var update = Builders<Employee>.Update
                .Set(x => x.DepartmentId, toDepartmentId)
                .Set(x => x.DesignationId, toDesignationId)
                .Set(x => x.UpdatedBy, updatedBy)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateManyAsync(filter, update);
            return result.ModifiedCount;
        }
    }
}