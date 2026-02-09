using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.LeaveType;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class LeaveTypeRepository : BaseRepository<LeaveType>, ILeaveTypeRepository
    {
        public LeaveTypeRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<LeaveType?> GetByCodeAsync(string code)
        {
            return await _collection.Find(x => x.Code == code && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsCodeExistsAsync(string code, string? excludeId = null)
        {
            var filter = Builders<LeaveType>.Filter.And(
                Builders<LeaveType>.Filter.Eq(x => x.Code, code),
                Builders<LeaveType>.Filter.Eq(x => x.IsDeleted, false)
            );

            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = Builders<LeaveType>.Filter.And(
                    filter,
                    Builders<LeaveType>.Filter.Ne(x => x.Id, excludeId)
                );
            }

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<(List<LeaveType> Items, int TotalCount)> GetFilteredLeaveTypesAsync(LeaveTypeFilterDto filter)
        {
            var filterBuilder = Builders<LeaveType>.Filter;
            var filters = new List<FilterDefinition<LeaveType>>
            {
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            // Search term filter
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.Code, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i"))
                );
                filters.Add(searchFilter);
            }

            // Active filter
            if (filter.IsActive.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.IsActive, filter.IsActive.Value));
            }

            // Requires approval filter
            if (filter.RequiresApproval.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.RequiresApproval, filter.RequiresApproval.Value));
            }

            // Requires document filter
            if (filter.RequiresDocument.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.RequiresDocument, filter.RequiresDocument.Value));
            }

            // Carry forward filter
            if (filter.IsCarryForward.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.IsCarryForward, filter.IsCarryForward.Value));
            }

            var combinedFilter = filterBuilder.And(filters);

            // Get total count
            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            // Sorting
            var sortBuilder = Builders<LeaveType>.Sort;
            SortDefinition<LeaveType> sort = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortDescending ? sortBuilder.Descending(x => x.Name) : sortBuilder.Ascending(x => x.Name),
                "code" => filter.SortDescending ? sortBuilder.Descending(x => x.Code) : sortBuilder.Ascending(x => x.Code),
                "maxdaysperyear" => filter.SortDescending ? sortBuilder.Descending(x => x.MaxDaysPerYear) : sortBuilder.Ascending(x => x.MaxDaysPerYear),
                "createdat" => filter.SortDescending ? sortBuilder.Descending(x => x.CreatedAt) : sortBuilder.Ascending(x => x.CreatedAt),
                _ => filter.SortDescending ? sortBuilder.Descending(x => x.DisplayOrder) : sortBuilder.Ascending(x => x.DisplayOrder)
            };

            // Get paginated items
            var items = await _collection
                .Find(combinedFilter)
                .Sort(sort)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        public async Task<List<LeaveType>> GetActiveLeaveTypesAsync()
        {
            return await _collection
                .Find(x => x.IsActive && !x.IsDeleted)
                .SortBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<LeaveType>> GetLeaveTypesRequiringApprovalAsync()
        {
            return await _collection
                .Find(x => x.RequiresApproval && x.IsActive && !x.IsDeleted)
                .SortBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<LeaveType>> GetLeaveTypesRequiringDocumentAsync()
        {
            return await _collection
                .Find(x => x.RequiresDocument && x.IsActive && !x.IsDeleted)
                .SortBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<int> GetMaxDisplayOrderAsync()
        {
            var maxOrderLeaveType = await _collection
                .Find(x => !x.IsDeleted)
                .SortByDescending(x => x.DisplayOrder)
                .FirstOrDefaultAsync();

            return maxOrderLeaveType?.DisplayOrder ?? 0;
        }
    }
}