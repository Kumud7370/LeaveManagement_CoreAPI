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
        public EmployeeRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode)
        {
            return await _collection.Find(x => x.EmployeeCode == employeeCode && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _collection.Find(x => x.Email == email && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsEmployeeCodeExistsAsync(string employeeCode, string? excludeId = null)
        {
            var filter = Builders<Employee>.Filter.And(
                Builders<Employee>.Filter.Eq(x => x.EmployeeCode, employeeCode),
                Builders<Employee>.Filter.Eq(x => x.IsDeleted, false)
            );

            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = Builders<Employee>.Filter.And(
                    filter,
                    Builders<Employee>.Filter.Ne(x => x.Id, excludeId)
                );
            }

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, string? excludeId = null)
        {
            var filter = Builders<Employee>.Filter.And(
                Builders<Employee>.Filter.Eq(x => x.Email, email),
                Builders<Employee>.Filter.Eq(x => x.IsDeleted, false)
            );

            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = Builders<Employee>.Filter.And(
                    filter,
                    Builders<Employee>.Filter.Ne(x => x.Id, excludeId)
                );
            }

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<(List<Employee> Items, int TotalCount)> GetFilteredEmployeesAsync(EmployeeFilterDto filter)
        {
            var filterBuilder = Builders<Employee>.Filter;
            var filters = new List<FilterDefinition<Employee>>
            {
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(x => x.EmployeeCode, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.FirstName, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.LastName, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.Email, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i"))
                );
                filters.Add(searchFilter);
            }

            if (!string.IsNullOrWhiteSpace(filter.DepartmentId))
            {
                filters.Add(filterBuilder.Eq(x => x.DepartmentId, filter.DepartmentId));
            }

            if (!string.IsNullOrWhiteSpace(filter.DesignationId))
            {
                filters.Add(filterBuilder.Eq(x => x.DesignationId, filter.DesignationId));
            }

            if (!string.IsNullOrWhiteSpace(filter.ManagerId))
            {
                filters.Add(filterBuilder.Eq(x => x.ManagerId, filter.ManagerId));
            }

            if (filter.EmployeeStatus.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.EmployeeStatus, filter.EmployeeStatus.Value));
            }

            if (filter.EmploymentType.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.EmploymentType, filter.EmploymentType.Value));
            }

            if (filter.Gender.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.Gender, filter.Gender.Value));
            }

            if (filter.JoiningDateFrom.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.DateOfJoining, filter.JoiningDateFrom.Value));
            }

            if (filter.JoiningDateTo.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.DateOfJoining, filter.JoiningDateTo.Value));
            }

            var combinedFilter = filterBuilder.And(filters);

            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            var sortBuilder = Builders<Employee>.Sort;
            SortDefinition<Employee> sort = filter.SortBy.ToLower() switch
            {
                "firstname" => filter.SortDescending ? sortBuilder.Descending(x => x.FirstName) : sortBuilder.Ascending(x => x.FirstName),
                "lastname" => filter.SortDescending ? sortBuilder.Descending(x => x.LastName) : sortBuilder.Ascending(x => x.LastName),
                "email" => filter.SortDescending ? sortBuilder.Descending(x => x.Email) : sortBuilder.Ascending(x => x.Email),
                "dateofjoining" => filter.SortDescending ? sortBuilder.Descending(x => x.DateOfJoining) : sortBuilder.Ascending(x => x.DateOfJoining),
                "createdat" => filter.SortDescending ? sortBuilder.Descending(x => x.CreatedAt) : sortBuilder.Ascending(x => x.CreatedAt),
                _ => filter.SortDescending ? sortBuilder.Descending(x => x.EmployeeCode) : sortBuilder.Ascending(x => x.EmployeeCode)
            };

            var items = await _collection
                .Find(combinedFilter)
                .Sort(sort)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        public async Task<List<Employee>> GetEmployeesByDepartmentAsync(string departmentId)
        {
            return await _collection
                .Find(x => x.DepartmentId == departmentId && !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<Employee>> GetEmployeesByManagerAsync(string managerId)
        {
            return await _collection
                .Find(x => x.ManagerId == managerId && !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<Employee>> GetActiveEmployeesAsync()
        {
            return await _collection
                .Find(x => x.EmployeeStatus == EmployeeStatus.Active && !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<int> GetEmployeeCountByStatusAsync(EmployeeStatus status)
        {
            return (int)await _collection
                .CountDocumentsAsync(x => x.EmployeeStatus == status && !x.IsDeleted);
        }
    }
}