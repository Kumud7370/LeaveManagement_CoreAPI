using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.Designation;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class DesignationRepository : BaseRepository<Designation>, IDesignationRepository
    {
        private readonly IMongoCollection<Employee> _employeeCollection;

        public DesignationRepository(IMongoDbContext context) : base(context)
        {
            _employeeCollection = context.GetCollection<Employee>("employees");
        }

        public async Task<Designation?> GetByDesignationCodeAsync(string designationCode)
        {
            return await _collection
                .Find(x => x.DesignationCode == designationCode && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsDesignationCodeExistsAsync(string designationCode, string? excludeId = null)
        {
            var filter = Builders<Designation>.Filter.And(
                Builders<Designation>.Filter.Eq(x => x.DesignationCode, designationCode),
                Builders<Designation>.Filter.Eq(x => x.IsDeleted, false)
            );

            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = Builders<Designation>.Filter.And(
                    filter,
                    Builders<Designation>.Filter.Ne(x => x.Id, excludeId)
                );
            }

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<(List<Designation> Items, int TotalCount)> GetFilteredDesignationsAsync(DesignationFilterDto filter)
        {
            var filterBuilder = Builders<Designation>.Filter;
            var filters = new List<FilterDefinition<Designation>>
            {
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(x => x.DesignationCode, new BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.DesignationName, new BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.Description, new BsonRegularExpression(filter.SearchTerm, "i"))
                );
                filters.Add(searchFilter);
            }

            if (filter.IsActive.HasValue)
                filters.Add(filterBuilder.Eq(x => x.IsActive, filter.IsActive.Value));

            if (filter.Level.HasValue)
                filters.Add(filterBuilder.Eq(x => x.Level, filter.Level.Value));

            if (filter.MinLevel.HasValue)
                filters.Add(filterBuilder.Gte(x => x.Level, filter.MinLevel.Value));

            if (filter.MaxLevel.HasValue)
                filters.Add(filterBuilder.Lte(x => x.Level, filter.MaxLevel.Value));

            var combinedFilter = filterBuilder.And(filters);
            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            var sortBuilder = Builders<Designation>.Sort;
            SortDefinition<Designation> sort = filter.SortBy.ToLower() switch
            {
                "designationname" => filter.SortDescending
                    ? sortBuilder.Descending(x => x.DesignationName)
                    : sortBuilder.Ascending(x => x.DesignationName),
                "level" => filter.SortDescending
                    ? sortBuilder.Descending(x => x.Level)
                    : sortBuilder.Ascending(x => x.Level),
                "createdat" => filter.SortDescending
                    ? sortBuilder.Descending(x => x.CreatedAt)
                    : sortBuilder.Ascending(x => x.CreatedAt),
                _ => filter.SortDescending
                    ? sortBuilder.Descending(x => x.DesignationCode)
                    : sortBuilder.Ascending(x => x.DesignationCode)
            };

            var items = await _collection
                .Find(combinedFilter)
                .Sort(sort)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        public async Task<List<Designation>> GetByDepartmentIdAsync(string departmentId)
        {
            return await _collection
                .Find(x => x.DepartmentId == departmentId && x.IsActive && !x.IsDeleted)
                .SortBy(x => x.Level)
                .ThenBy(x => x.DesignationName)
                .ToListAsync();
        }

        public async Task<List<Designation>> GetByLevelAsync(int level)
        {
            return await _collection
                .Find(x => x.Level == level && !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<Designation>> GetActiveDesignationsAsync()
        {
            return await _collection
                .Find(x => x.IsActive && !x.IsDeleted)
                .SortBy(x => x.Level)
                .ThenBy(x => x.DesignationName)
                .ToListAsync();
        }

        public async Task<int> GetEmployeeCountByDesignationAsync(string designationId)
        {
            if (string.IsNullOrEmpty(designationId))
                return 0;

            var filter = Builders<Employee>.Filter.And(
                Builders<Employee>.Filter.Eq(x => x.DesignationId, designationId),
                Builders<Employee>.Filter.Eq(x => x.IsDeleted, false)
            );

            return (int)await _employeeCollection.CountDocumentsAsync(filter);
        }
    }
}