using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.Shift;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class ShiftRepository : BaseRepository<Shift>, IShiftRepository
    {
        public ShiftRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<Shift?> GetByCodeAsync(string code)
        {
            return await _collection.Find(x => x.ShiftCode == code && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsCodeExistsAsync(string code, string? excludeId = null)
        {
            var filter = Builders<Shift>.Filter.And(
                Builders<Shift>.Filter.Eq(x => x.ShiftCode, code),
                Builders<Shift>.Filter.Eq(x => x.IsDeleted, false)
            );

            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = Builders<Shift>.Filter.And(
                    filter,
                    Builders<Shift>.Filter.Ne(x => x.Id, excludeId)
                );
            }

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<(List<Shift> Items, int TotalCount)> GetFilteredShiftsAsync(ShiftFilterDto filter)
        {
            var filterBuilder = Builders<Shift>.Filter;
            var filters = new List<FilterDefinition<Shift>>
            {
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(x => x.ShiftName, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.ShiftCode, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i"))
                );
                filters.Add(searchFilter);
            }

            if (filter.IsActive.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.IsActive, filter.IsActive.Value));
            }

            if (filter.IsNightShift.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.IsNightShift, filter.IsNightShift.Value));
            }

            var combinedFilter = filterBuilder.And(filters);

            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            var sortBuilder = Builders<Shift>.Sort;
            SortDefinition<Shift> sort = filter.SortBy.ToLower() switch
            {
                "shiftname" => filter.SortDescending ? sortBuilder.Descending(x => x.ShiftName) : sortBuilder.Ascending(x => x.ShiftName),
                "shiftcode" => filter.SortDescending ? sortBuilder.Descending(x => x.ShiftCode) : sortBuilder.Ascending(x => x.ShiftCode),
                "starttime" => filter.SortDescending ? sortBuilder.Descending(x => x.StartTime) : sortBuilder.Ascending(x => x.StartTime),
                "createdat" => filter.SortDescending ? sortBuilder.Descending(x => x.CreatedAt) : sortBuilder.Ascending(x => x.CreatedAt),
                _ => filter.SortDescending ? sortBuilder.Descending(x => x.DisplayOrder) : sortBuilder.Ascending(x => x.DisplayOrder)
            };

            var items = await _collection
                .Find(combinedFilter)
                .Sort(sort)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        public async Task<List<Shift>> GetActiveShiftsAsync()
        {
            return await _collection
                .Find(x => x.IsActive && !x.IsDeleted)
                .SortBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<Shift>> GetNightShiftsAsync()
        {
            return await _collection
                .Find(x => x.IsNightShift && x.IsActive && !x.IsDeleted)
                .SortBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<int> GetMaxDisplayOrderAsync()
        {
            var maxOrderShift = await _collection
                .Find(x => !x.IsDeleted)
                .SortByDescending(x => x.DisplayOrder)
                .FirstOrDefaultAsync();

            return maxOrderShift?.DisplayOrder ?? 0;
        }

        public async Task<bool> HasOverlappingShiftTimesAsync(TimeOnly startTime, TimeOnly endTime, string? excludeId = null)
        {
            var filter = Builders<Shift>.Filter.And(
                Builders<Shift>.Filter.Eq(x => x.IsDeleted, false),
                Builders<Shift>.Filter.Eq(x => x.IsActive, true)
            );

            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = Builders<Shift>.Filter.And(
                    filter,
                    Builders<Shift>.Filter.Ne(x => x.Id, excludeId)
                );
            }

            var existingShifts = await _collection.Find(filter).ToListAsync();

            foreach (var shift in existingShifts)
            {
                if (TimesOverlap(startTime, endTime, shift.StartTime, shift.EndTime))
                    return true;
            }

            return false;
        }

        private bool TimesOverlap(TimeOnly start1, TimeOnly end1, TimeOnly start2, TimeOnly end2)
        {
            if (end1 < start1) 
            {
                if (end2 < start2) 
                    return true; 
                return start2 >= start1 || end2 <= end1;
            }

            if (end2 < start2) 
            {
                return start1 >= start2 || end1 <= end2;
            }

            return start1 < end2 && end1 > start2;
        }
    }
}