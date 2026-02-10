using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.LeaveBalance;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class LeaveBalanceRepository : BaseRepository<LeaveBalance>, ILeaveBalanceRepository
    {
        public LeaveBalanceRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<LeaveBalance?> GetByEmployeeAndLeaveTypeAsync(string employeeId, string leaveTypeId, int year)
        {
            return await _collection
                .Find(x => x.EmployeeId == employeeId &&
                          x.LeaveTypeId == leaveTypeId &&
                          x.Year == year &&
                          !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<LeaveBalance>> GetByEmployeeIdAsync(string employeeId, int? year = null)
        {
            var filterBuilder = Builders<LeaveBalance>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(x => x.EmployeeId, employeeId),
                filterBuilder.Eq(x => x.IsDeleted, false)
            );

            if (year.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.Year, year.Value));
            }

            return await _collection
                .Find(filter)
                .SortByDescending(x => x.Year)
                .ThenBy(x => x.LeaveTypeId)
                .ToListAsync();
        }

        public async Task<List<LeaveBalance>> GetByLeaveTypeIdAsync(string leaveTypeId, int? year = null)
        {
            var filterBuilder = Builders<LeaveBalance>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(x => x.LeaveTypeId, leaveTypeId),
                filterBuilder.Eq(x => x.IsDeleted, false)
            );

            if (year.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.Year, year.Value));
            }

            return await _collection
                .Find(filter)
                .SortByDescending(x => x.Year)
                .ThenBy(x => x.EmployeeId)
                .ToListAsync();
        }

        public async Task<List<LeaveBalance>> GetByYearAsync(int year)
        {
            return await _collection
                .Find(x => x.Year == year && !x.IsDeleted)
                .SortBy(x => x.EmployeeId)
                .ThenBy(x => x.LeaveTypeId)
                .ToListAsync();
        }

        public async Task<(List<LeaveBalance> Items, int TotalCount)> GetFilteredLeaveBalancesAsync(LeaveBalanceFilterDto filter)
        {
            var filterBuilder = Builders<LeaveBalance>.Filter;
            var filters = new List<FilterDefinition<LeaveBalance>>
            {
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            // Employee filter
            if (!string.IsNullOrWhiteSpace(filter.EmployeeId))
            {
                filters.Add(filterBuilder.Eq(x => x.EmployeeId, filter.EmployeeId));
            }

            // Leave type filter
            if (!string.IsNullOrWhiteSpace(filter.LeaveTypeId))
            {
                filters.Add(filterBuilder.Eq(x => x.LeaveTypeId, filter.LeaveTypeId));
            }

            // Year filter
            if (filter.Year.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.Year, filter.Year.Value));
            }

            // Available balance range filter
            if (filter.MinAvailableBalance.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.Available, filter.MinAvailableBalance.Value));
            }

            if (filter.MaxAvailableBalance.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.Available, filter.MaxAvailableBalance.Value));
            }

            // Consumed balance range filter
            if (filter.MinConsumedBalance.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.Consumed, filter.MinConsumedBalance.Value));
            }

            if (filter.MaxConsumedBalance.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.Consumed, filter.MaxConsumedBalance.Value));
            }

            // Low balance filter
            if (filter.IsLowBalance.HasValue && filter.IsLowBalance.Value)
            {
                filters.Add(filterBuilder.Lte(x => x.Available, filter.LowBalanceThreshold ?? 2));
            }

            var combinedFilter = filterBuilder.And(filters);

            // Get total count
            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            // Sorting
            var sortBuilder = Builders<LeaveBalance>.Sort;
            SortDefinition<LeaveBalance> sort = filter.SortBy.ToLower() switch
            {
                "year" => filter.SortDescending ? sortBuilder.Descending(x => x.Year) : sortBuilder.Ascending(x => x.Year),
                "totalallocated" => filter.SortDescending ? sortBuilder.Descending(x => x.TotalAllocated) : sortBuilder.Ascending(x => x.TotalAllocated),
                "consumed" => filter.SortDescending ? sortBuilder.Descending(x => x.Consumed) : sortBuilder.Ascending(x => x.Consumed),
                "available" => filter.SortDescending ? sortBuilder.Descending(x => x.Available) : sortBuilder.Ascending(x => x.Available),
                "carriedforward" => filter.SortDescending ? sortBuilder.Descending(x => x.CarriedForward) : sortBuilder.Ascending(x => x.CarriedForward),
                "lastupdated" => filter.SortDescending ? sortBuilder.Descending(x => x.LastUpdated) : sortBuilder.Ascending(x => x.LastUpdated),
                _ => filter.SortDescending ? sortBuilder.Descending(x => x.Year).Descending(x => x.EmployeeId) : sortBuilder.Ascending(x => x.Year).Ascending(x => x.EmployeeId)
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

        public async Task<bool> ExistsAsync(string employeeId, string leaveTypeId, int year, string? excludeId = null)
        {
            var filterBuilder = Builders<LeaveBalance>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(x => x.EmployeeId, employeeId),
                filterBuilder.Eq(x => x.LeaveTypeId, leaveTypeId),
                filterBuilder.Eq(x => x.Year, year),
                filterBuilder.Eq(x => x.IsDeleted, false)
            );

            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = filterBuilder.And(filter, filterBuilder.Ne(x => x.Id, excludeId));
            }

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<List<LeaveBalance>> GetLowBalanceAlerts(decimal threshold = 2)
        {
            var currentYear = DateTime.UtcNow.Year;

            return await _collection
                .Find(x => x.Year == currentYear &&
                          x.Available <= threshold &&
                          x.Available > 0 &&
                          !x.IsDeleted)
                .SortBy(x => x.Available)
                .ToListAsync();
        }

        public async Task<List<LeaveBalance>> GetExpiringSoonAsync(int year, int daysThreshold = 30)
        {
            var endOfYear = new DateTime(year, 12, 31);
            var currentDate = DateTime.UtcNow.Date;
            var daysUntilExpiry = (endOfYear - currentDate).Days;

            if (daysUntilExpiry > daysThreshold)
                return new List<LeaveBalance>();

            return await _collection
                .Find(x => x.Year == year &&
                          x.Available > 0 &&
                          !x.IsDeleted)
                .SortByDescending(x => x.Available)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalConsumedByEmployeeAsync(string employeeId, int year)
        {
            var balances = await _collection
                .Find(x => x.EmployeeId == employeeId &&
                          x.Year == year &&
                          !x.IsDeleted)
                .ToListAsync();

            return balances.Sum(x => x.Consumed);
        }

        public async Task<Dictionary<string, decimal>> GetLeaveTypeConsumptionByEmployeeAsync(string employeeId, int year)
        {
            var balances = await _collection
                .Find(x => x.EmployeeId == employeeId &&
                          x.Year == year &&
                          !x.IsDeleted)
                .ToListAsync();

            return balances.ToDictionary(x => x.LeaveTypeId, x => x.Consumed);
        }
    }
}