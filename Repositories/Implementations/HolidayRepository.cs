using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.Holiday;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class HolidayRepository : BaseRepository<Holiday>, IHolidayRepository
    {
        public HolidayRepository(IMongoDbContext context) : base(context)
        {
        }
        public async Task<bool> UpdateHolidayFieldsAsync(string id, Holiday holiday)
        {
            var filter = Builders<Holiday>.Filter.And(
                Builders<Holiday>.Filter.Eq(x => x.Id, id),
                Builders<Holiday>.Filter.Eq(x => x.IsDeleted, false)
            );

            var update = Builders<Holiday>.Update
                .Set(x => x.HolidayName, holiday.HolidayName)
                .Set(x => x.HolidayDate, holiday.HolidayDate)
                .Set(x => x.Description, holiday.Description)
                .Set(x => x.HolidayType, holiday.HolidayType)
                .Set(x => x.IsOptional, holiday.IsOptional)
                .Set(x => x.IsActive, holiday.IsActive)
                .Set(x => x.ApplicableDepartments, holiday.ApplicableDepartments)
                .Set(x => x.UpdatedBy, holiday.UpdatedBy)
                .Set(x => x.UpdatedAt, holiday.UpdatedAt);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<Holiday?> GetByNameAndDateAsync(string holidayName, DateTime holidayDate)
        {
            var startOfDay = holidayDate.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _collection.Find(x =>
                x.HolidayName == holidayName &&
                x.HolidayDate >= startOfDay &&
                x.HolidayDate < endOfDay &&
                !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsHolidayExistsAsync(string holidayName, DateTime holidayDate, string? excludeId = null)
        {
            var startOfDay = holidayDate.Date;
            var endOfDay = startOfDay.AddDays(1);

            var fb = Builders<Holiday>.Filter;
            var filter = fb.And(
                fb.Eq(x => x.HolidayName, holidayName),
                fb.Gte(x => x.HolidayDate, startOfDay),
                fb.Lt(x => x.HolidayDate, endOfDay),
                fb.Eq(x => x.IsDeleted, false)
            );

            if (!string.IsNullOrEmpty(excludeId))
                filter = fb.And(filter, fb.Ne(x => x.Id, excludeId));

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<(List<Holiday> Items, int TotalCount)> GetFilteredHolidaysAsync(HolidayFilterDto filter)
        {
            var fb = Builders<Holiday>.Filter;
            var filters = new List<FilterDefinition<Holiday>>
            {
                fb.Eq(x => x.IsDeleted, false)
            };

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                filters.Add(fb.Or(
                    fb.Regex(x => x.HolidayName, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    fb.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i"))
                ));
            }

            if (filter.HolidayType.HasValue)
                filters.Add(fb.Eq(x => x.HolidayType, filter.HolidayType.Value));

            if (filter.IsOptional.HasValue)
                filters.Add(fb.Eq(x => x.IsOptional, filter.IsOptional.Value));

            if (!string.IsNullOrWhiteSpace(filter.DepartmentId))
                filters.Add(fb.AnyEq(x => x.ApplicableDepartments, filter.DepartmentId));

            if (filter.IsActive.HasValue)
                filters.Add(fb.Eq(x => x.IsActive, filter.IsActive.Value));

            if (filter.Month.HasValue && filter.Year.HasValue)
            {
                var monthStart = new DateTime(filter.Year.Value, filter.Month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
                filters.Add(fb.Gte(x => x.HolidayDate, monthStart));
                filters.Add(fb.Lte(x => x.HolidayDate, monthEnd));
            }
            else if (filter.Year.HasValue)
            {
                var yearStart = new DateTime(filter.Year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var yearEnd = new DateTime(filter.Year.Value, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                filters.Add(fb.Gte(x => x.HolidayDate, yearStart));
                filters.Add(fb.Lte(x => x.HolidayDate, yearEnd));
            }
            else
            {
                if (filter.DateFrom.HasValue)
                    filters.Add(fb.Gte(x => x.HolidayDate, filter.DateFrom.Value));
                if (filter.DateTo.HasValue)
                    filters.Add(fb.Lte(x => x.HolidayDate, filter.DateTo.Value));
            }

            if (filter.IsUpcoming.HasValue && filter.IsUpcoming.Value)
                filters.Add(fb.Gte(x => x.HolidayDate, DateTime.UtcNow.Date));

            var combinedFilter = fb.And(filters);
            var totalCount = (int)await _collection.CountDocumentsAsync(combinedFilter);

            var sb = Builders<Holiday>.Sort;
            SortDefinition<Holiday> sort = filter.SortBy.ToLower() switch
            {
                "holidayname" => filter.SortDescending ? sb.Descending(x => x.HolidayName) : sb.Ascending(x => x.HolidayName),
                "holidaytype" => filter.SortDescending ? sb.Descending(x => x.HolidayType) : sb.Ascending(x => x.HolidayType),
                "createdat" => filter.SortDescending ? sb.Descending(x => x.CreatedAt) : sb.Ascending(x => x.CreatedAt),
                _ => filter.SortDescending ? sb.Descending(x => x.HolidayDate) : sb.Ascending(x => x.HolidayDate)
            };

            var items = await _collection
                .Find(combinedFilter)
                .Sort(sort)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<Holiday>> GetHolidaysByDepartmentAsync(string departmentId)
            => await _collection
                .Find(x => x.ApplicableDepartments.Contains(departmentId) && !x.IsDeleted)
                .SortBy(x => x.HolidayDate)
                .ToListAsync();

        public async Task<List<Holiday>> GetHolidaysByDateRangeAsync(DateTime startDate, DateTime endDate)
            => await _collection
                .Find(x => x.HolidayDate >= startDate && x.HolidayDate <= endDate && !x.IsDeleted)
                .SortBy(x => x.HolidayDate)
                .ToListAsync();

        public async Task<List<Holiday>> GetUpcomingHolidaysAsync(int count = 10)
        {
            var today = DateTime.UtcNow.Date;
            return await _collection
                .Find(x => x.HolidayDate >= today && !x.IsDeleted)
                .SortBy(x => x.HolidayDate)
                .Limit(count)
                .ToListAsync();
        }

        public async Task<List<Holiday>> GetHolidaysByYearAsync(int year)
        {
            var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var yearEnd = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
            return await _collection
                .Find(x => x.HolidayDate >= yearStart && x.HolidayDate <= yearEnd && !x.IsDeleted)
                .SortBy(x => x.HolidayDate)
                .ToListAsync();
        }

        public async Task<List<Holiday>> GetHolidaysByMonthAsync(int year, int month)
        {
            var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
            return await _collection
                .Find(x => x.HolidayDate >= monthStart && x.HolidayDate <= monthEnd && !x.IsDeleted)
                .SortBy(x => x.HolidayDate)
                .ToListAsync();
        }

        public async Task<List<Holiday>> GetHolidaysByTypeAsync(HolidayType holidayType)
            => await _collection
                .Find(x => x.HolidayType == holidayType && !x.IsDeleted)
                .SortBy(x => x.HolidayDate)
                .ToListAsync();

        public async Task<int> GetHolidayCountByTypeAsync(HolidayType holidayType)
            => (int)await _collection
                .CountDocumentsAsync(x => x.HolidayType == holidayType && !x.IsDeleted);

        public async Task<bool> IsHolidayOnDateAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);
            return await _collection
                .Find(x => x.HolidayDate >= startOfDay && x.HolidayDate < endOfDay && !x.IsDeleted)
                .AnyAsync();
        }

        public async Task<bool> SoftDeleteAsync(string id, string deletedBy)
        {
            var update = Builders<Holiday>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.DeletedAt, DateTime.UtcNow)
                .Set(x => x.UpdatedBy, deletedBy)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(
                x => x.Id == id && !x.IsDeleted,
                update
            );

            return result.ModifiedCount > 0;
        }
    }
}