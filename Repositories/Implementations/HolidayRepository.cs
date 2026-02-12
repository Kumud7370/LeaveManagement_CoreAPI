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

            var filterBuilder = Builders<Holiday>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(x => x.HolidayName, holidayName),
                filterBuilder.Gte(x => x.HolidayDate, startOfDay),
                filterBuilder.Lt(x => x.HolidayDate, endOfDay),
                filterBuilder.Eq(x => x.IsDeleted, false)
            );

            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = filterBuilder.And(
                    filter,
                    filterBuilder.Ne(x => x.Id, excludeId)
                );
            }

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<(List<Holiday> Items, int TotalCount)> GetFilteredHolidaysAsync(HolidayFilterDto filter)
        {
            var filterBuilder = Builders<Holiday>.Filter;
            var filters = new List<FilterDefinition<Holiday>>
            {
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(x => x.HolidayName, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i"))
                );
                filters.Add(searchFilter);
            }

            if (filter.HolidayType.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.HolidayType, filter.HolidayType.Value));
            }

            if (filter.IsOptional.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.IsOptional, filter.IsOptional.Value));
            }

            if (!string.IsNullOrWhiteSpace(filter.DepartmentId))
            {
                filters.Add(filterBuilder.AnyEq(x => x.ApplicableDepartments, filter.DepartmentId));
            }

            if (filter.DateFrom.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.HolidayDate, filter.DateFrom.Value));
            }

            if (filter.DateTo.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.HolidayDate, filter.DateTo.Value));
            }

            if (filter.IsUpcoming.HasValue && filter.IsUpcoming.Value)
            {
                filters.Add(filterBuilder.Gte(x => x.HolidayDate, DateTime.UtcNow.Date));
            }

            if (filter.Year.HasValue)
            {
                var yearStart = new DateTime(filter.Year.Value, 1, 1);
                var yearEnd = new DateTime(filter.Year.Value, 12, 31, 23, 59, 59);
                filters.Add(filterBuilder.Gte(x => x.HolidayDate, yearStart));
                filters.Add(filterBuilder.Lte(x => x.HolidayDate, yearEnd));
            }

            if (filter.Month.HasValue && filter.Year.HasValue)
            {
                var monthStart = new DateTime(filter.Year.Value, filter.Month.Value, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
                filters.Add(filterBuilder.Gte(x => x.HolidayDate, monthStart));
                filters.Add(filterBuilder.Lte(x => x.HolidayDate, monthEnd));
            }

            var combinedFilter = filterBuilder.And(filters);

            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            var sortBuilder = Builders<Holiday>.Sort;
            SortDefinition<Holiday> sort = filter.SortBy.ToLower() switch
            {
                "holidayname" => filter.SortDescending ? sortBuilder.Descending(x => x.HolidayName) : sortBuilder.Ascending(x => x.HolidayName),
                "holidaytype" => filter.SortDescending ? sortBuilder.Descending(x => x.HolidayType) : sortBuilder.Ascending(x => x.HolidayType),
                "createdat" => filter.SortDescending ? sortBuilder.Descending(x => x.CreatedAt) : sortBuilder.Ascending(x => x.CreatedAt),
                _ => filter.SortDescending ? sortBuilder.Descending(x => x.HolidayDate) : sortBuilder.Ascending(x => x.HolidayDate)
            };

            var items = await _collection
                .Find(combinedFilter)
                .Sort(sort)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        public async Task<List<Holiday>> GetHolidaysByDepartmentAsync(string departmentId)
        {
            return await _collection
                .Find(x => x.ApplicableDepartments.Contains(departmentId) && !x.IsDeleted)
                .SortBy(x => x.HolidayDate)
                .ToListAsync();
        }

        public async Task<List<Holiday>> GetHolidaysByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _collection
                .Find(x => x.HolidayDate >= startDate && x.HolidayDate <= endDate && !x.IsDeleted)
                .SortBy(x => x.HolidayDate)
                .ToListAsync();
        }

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
            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);
            return await _collection
                .Find(x => x.HolidayDate >= yearStart && x.HolidayDate <= yearEnd && !x.IsDeleted)
                .SortBy(x => x.HolidayDate)
                .ToListAsync();
        }

        public async Task<List<Holiday>> GetHolidaysByMonthAsync(int year, int month)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
            return await _collection
                .Find(x => x.HolidayDate >= monthStart && x.HolidayDate <= monthEnd && !x.IsDeleted)
                .SortBy(x => x.HolidayDate)
                .ToListAsync();
        }

        public async Task<List<Holiday>> GetHolidaysByTypeAsync(HolidayType holidayType)
        {
            return await _collection
                .Find(x => x.HolidayType == holidayType && !x.IsDeleted)
                .SortBy(x => x.HolidayDate)
                .ToListAsync();
        }

        public async Task<int> GetHolidayCountByTypeAsync(HolidayType holidayType)
        {
            return (int)await _collection
                .CountDocumentsAsync(x => x.HolidayType == holidayType && !x.IsDeleted);
        }

        public async Task<bool> IsHolidayOnDateAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _collection
                .Find(x => x.HolidayDate >= startOfDay && x.HolidayDate < endOfDay && !x.IsDeleted)
                .AnyAsync();
        }
    }
}