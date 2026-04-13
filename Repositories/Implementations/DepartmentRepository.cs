using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.Department;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        private readonly IMongoCollection<Employee> _employeeCollection;

        public DepartmentRepository(IMongoDbContext context) : base(context)
        {
            _employeeCollection = context.GetCollection<Employee>("employees");
        }

        public async Task<Department?> GetByDepartmentCodeAsync(string departmentCode)
        {
            return await _collection.Find(x => x.DepartmentCode == departmentCode && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<Department?> GetByDepartmentIdAsync(Guid departmentId)
        {
            return await _collection.Find(x => x.DepartmentId == departmentId && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsDepartmentCodeExistsAsync(string departmentCode, Guid? excludeId = null)
        {
            var filter = Builders<Department>.Filter.And(
                Builders<Department>.Filter.Eq(x => x.DepartmentCode, departmentCode),
                Builders<Department>.Filter.Eq(x => x.IsDeleted, false)
            );

            if (excludeId.HasValue)
            {
                filter = Builders<Department>.Filter.And(
                    filter,
                    Builders<Department>.Filter.Ne(x => x.DepartmentId, excludeId.Value)
                );
            }

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<(List<Department> Items, int TotalCount)> GetFilteredDepartmentsAsync(DepartmentFilterRequestDto filter)
        {
            var filterBuilder = Builders<Department>.Filter;
            var filters = new List<FilterDefinition<Department>>();

            if (!filter.IncludeDeleted)
                filters.Add(filterBuilder.Eq(x => x.IsDeleted, false));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(x => x.DepartmentCode, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.DepartmentNameMr, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.DepartmentName, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.DepartmentNameHi, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm ?? "", "i"))
                );
                filters.Add(searchFilter);
            }

            if (filter.IsActive.HasValue)
                filters.Add(filterBuilder.Eq(x => x.IsActive, filter.IsActive.Value));

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            var sortBuilder = Builders<Department>.Sort;
            var sortDirection = filter.SortDirection?.ToLower() ?? "asc";

            SortDefinition<Department> sort = (filter.SortBy?.ToLower() ?? "departmentname") switch
            {
                "departmentcode" => sortDirection == "desc"
                    ? sortBuilder.Descending(x => x.DepartmentCode)
                    : sortBuilder.Ascending(x => x.DepartmentCode),
                "departmentname" => sortDirection == "desc"
                    ? sortBuilder.Descending(x => x.DepartmentName)
                    : sortBuilder.Ascending(x => x.DepartmentNameMr),
                "displayorder" => sortDirection == "desc"
                    ? sortBuilder.Descending(x => x.DisplayOrder)
                    : sortBuilder.Ascending(x => x.DisplayOrder),
                "createdat" => sortDirection == "desc"
                    ? sortBuilder.Descending(x => x.CreatedAt)
                    : sortBuilder.Ascending(x => x.CreatedAt),
                "updatedat" => sortDirection == "desc"
                    ? sortBuilder.Descending(x => x.UpdatedAt)
                    : sortBuilder.Ascending(x => x.UpdatedAt),
                _ => sortDirection == "desc"
                    ? sortBuilder.Descending(x => x.DepartmentName)
                    : sortBuilder.Ascending(x => x.DepartmentName)
            };

            var items = await _collection
                .Find(combinedFilter)
                .Sort(sort)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        public async Task<List<Department>> GetChildDepartmentsAsync(Guid parentDepartmentId)
        {
            return new List<Department>();
        }

        public async Task<List<Department>> GetRootDepartmentsAsync()
        {
            return await _collection
                .Find(x => !x.IsDeleted)
                .SortBy(x => x.DisplayOrder)
                .ThenBy(x => x.DepartmentName)
                .ToListAsync();
        }

        public async Task<List<Department>> GetActiveDepartmentsAsync()
        {
            return await _collection
                .Find(x => x.IsActive && !x.IsDeleted)
                .SortBy(x => x.DisplayOrder)
                .ThenBy(x => x.DepartmentName)
                .ToListAsync();
        }

        public async Task<Department?> GetDepartmentWithDetailsAsync(Guid departmentId)
        {
            return await _collection
                .Find(x => x.DepartmentId == departmentId && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetEmployeeCountByDepartmentAsync(Guid departmentId)
        {
            var departmentIdString = departmentId.ToString();
            return (int)await _employeeCollection
                .CountDocumentsAsync(x => x.DepartmentId == departmentIdString && !x.IsDeleted);
        }

        public async Task<bool> HasChildDepartmentsAsync(Guid departmentId)
        {
            return false;
        }

        public async Task<bool> HasEmployeesAsync(Guid departmentId)
        {
            var departmentIdString = departmentId.ToString();
            return await _employeeCollection
                .Find(x => x.DepartmentId == departmentIdString && !x.IsDeleted)
                .AnyAsync();
        }

        public async Task<List<Department>> GetDepartmentHierarchyAsync()
        {
            return await _collection
                .Find(x => !x.IsDeleted)
                .SortBy(x => x.DisplayOrder)
                .ThenBy(x => x.DepartmentName)
                .ToListAsync();
        }
    }
}