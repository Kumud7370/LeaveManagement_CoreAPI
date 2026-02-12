using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.AttendanceRegularization;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class AttendanceRegularizationRepository : IAttendanceRegularizationRepository
    {
        private readonly IMongoCollection<AttendanceRegularization> _collection;

        public AttendanceRegularizationRepository(IMongoDbContext context)
        {
            _collection = context.GetCollection<AttendanceRegularization>("AttendanceRegularizations");
        }

        public async Task<AttendanceRegularization> CreateAsync(AttendanceRegularization regularization)
        {
            regularization.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            regularization.CreatedAt = DateTime.UtcNow;
            regularization.IsDeleted = false;
            await _collection.InsertOneAsync(regularization);
            return regularization;
        }

        public async Task<bool> UpdateAsync(string id, AttendanceRegularization regularization)
        {
            regularization.UpdatedAt = DateTime.UtcNow;
            var result = await _collection.ReplaceOneAsync(
                x => x.Id == id && !x.IsDeleted,
                regularization
            );
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var update = Builders<AttendanceRegularization>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(
                x => x.Id == id && !x.IsDeleted,
                update
            );
            return result.ModifiedCount > 0;
        }

        public async Task<AttendanceRegularization?> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<AttendanceRegularization>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _collection.Find(x => x.EmployeeId == employeeId && !x.IsDeleted)
                .SortByDescending(x => x.RequestedAt)
                .ToListAsync();
        }

        public async Task<(List<AttendanceRegularization> Items, int TotalCount)> GetFilteredAsync(RegularizationFilterDto filter)
        {
            var filterBuilder = Builders<AttendanceRegularization>.Filter;
            var filters = new List<FilterDefinition<AttendanceRegularization>>
            {
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            if (!string.IsNullOrWhiteSpace(filter.EmployeeId))
            {
                filters.Add(filterBuilder.Eq(x => x.EmployeeId, filter.EmployeeId));
            }

            if (filter.StartDate.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.AttendanceDate, filter.StartDate.Value.Date));
            }

            if (filter.EndDate.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.AttendanceDate, filter.EndDate.Value.Date.AddDays(1)));
            }

            if (filter.RegularizationType.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.RegularizationType, filter.RegularizationType.Value));
            }

            if (filter.Status.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.Status, filter.Status.Value));
            }

            var combinedFilter = filterBuilder.And(filters);

            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            var sortBuilder = Builders<AttendanceRegularization>.Sort;
            SortDefinition<AttendanceRegularization> sort = filter.SortBy.ToLower() switch
            {
                "employeeid" => filter.SortDescending ? sortBuilder.Descending(x => x.EmployeeId) : sortBuilder.Ascending(x => x.EmployeeId),
                "attendancedate" => filter.SortDescending ? sortBuilder.Descending(x => x.AttendanceDate) : sortBuilder.Ascending(x => x.AttendanceDate),
                "status" => filter.SortDescending ? sortBuilder.Descending(x => x.Status) : sortBuilder.Ascending(x => x.Status),
                _ => filter.SortDescending ? sortBuilder.Descending(x => x.RequestedAt) : sortBuilder.Ascending(x => x.RequestedAt)
            };

            var items = await _collection
                .Find(combinedFilter)
                .Sort(sort)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        public async Task<int> GetPendingCountByEmployeeAsync(string employeeId)
        {
            return (int)await _collection.CountDocumentsAsync(x =>
                x.EmployeeId == employeeId &&
                x.Status == RegularizationStatus.Pending &&
                !x.IsDeleted);
        }

        public async Task<AttendanceRegularization?> GetByEmployeeAndDateAsync(string employeeId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _collection.Find(x =>
                x.EmployeeId == employeeId &&
                x.AttendanceDate >= startOfDay &&
                x.AttendanceDate < endOfDay &&
                x.Status == RegularizationStatus.Pending &&
                !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<AttendanceRegularization>> GetPendingRegularizationsAsync()
        {
            return await _collection.Find(x =>
                x.Status == RegularizationStatus.Pending &&
                !x.IsDeleted)
                .SortBy(x => x.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<AttendanceRegularization>> GetByStatusAsync(RegularizationStatus status)
        {
            return await _collection.Find(x =>
                x.Status == status &&
                !x.IsDeleted)
                .SortByDescending(x => x.RequestedAt)
                .ToListAsync();
        }
    }
}