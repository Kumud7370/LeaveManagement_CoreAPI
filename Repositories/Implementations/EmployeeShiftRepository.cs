using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.EmployeeShift;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class EmployeeShiftRepository : BaseRepository<EmployeeShift>, IEmployeeShiftRepository
    {
        public EmployeeShiftRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<(List<EmployeeShift> Items, int TotalCount)> GetFilteredEmployeeShiftsAsync(EmployeeShiftFilterDto filter)
        {
            var filterBuilder = Builders<EmployeeShift>.Filter;
            var filters = new List<FilterDefinition<EmployeeShift>>
            {
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            if (!string.IsNullOrWhiteSpace(filter.EmployeeId))
            {
                filters.Add(filterBuilder.Eq(x => x.EmployeeId, filter.EmployeeId));
            }

            if (!string.IsNullOrWhiteSpace(filter.ShiftId))
            {
                filters.Add(filterBuilder.Eq(x => x.ShiftId, filter.ShiftId));
            }

            if (filter.Status.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.Status, filter.Status.Value));
            }

            if (filter.EffectiveFromStart.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.EffectiveFrom, filter.EffectiveFromStart.Value));
            }

            if (filter.EffectiveFromEnd.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.EffectiveFrom, filter.EffectiveFromEnd.Value));
            }

            if (filter.IsActive.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.IsActive, filter.IsActive.Value));
            }

            if (filter.OnlyCurrentAssignments == true)
            {
                var today = DateTime.UtcNow.Date;
                filters.Add(filterBuilder.And(
                    filterBuilder.Eq(x => x.IsActive, true),
                    filterBuilder.Eq(x => x.Status, ShiftChangeStatus.Approved),
                    filterBuilder.Lte(x => x.EffectiveFrom, today),
                    filterBuilder.Or(
                        filterBuilder.Eq(x => x.EffectiveTo, null),
                        filterBuilder.Gte(x => x.EffectiveTo, today)
                    )
                ));
            }

            var combinedFilter = filterBuilder.And(filters);

            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            var sortBuilder = Builders<EmployeeShift>.Sort;
            SortDefinition<EmployeeShift> sort = filter.SortBy.ToLower() switch
            {
                "effectivefrom" => filter.SortDescending ? sortBuilder.Descending(x => x.EffectiveFrom) : sortBuilder.Ascending(x => x.EffectiveFrom),
                "effectiveto" => filter.SortDescending ? sortBuilder.Descending(x => x.EffectiveTo) : sortBuilder.Ascending(x => x.EffectiveTo),
                "status" => filter.SortDescending ? sortBuilder.Descending(x => x.Status) : sortBuilder.Ascending(x => x.Status),
                "createdat" => filter.SortDescending ? sortBuilder.Descending(x => x.CreatedAt) : sortBuilder.Ascending(x => x.CreatedAt),
                _ => filter.SortDescending ? sortBuilder.Descending(x => x.EffectiveFrom) : sortBuilder.Ascending(x => x.EffectiveFrom)
            };

            var items = await _collection
                .Find(combinedFilter)
                .Sort(sort)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        public async Task<List<EmployeeShift>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _collection
                .Find(x => x.EmployeeId == employeeId && !x.IsDeleted)
                .SortByDescending(x => x.EffectiveFrom)
                .ToListAsync();
        }

        public async Task<EmployeeShift?> GetCurrentShiftForEmployeeAsync(string employeeId)
        {
            var today = DateTime.UtcNow.Date;
            return await _collection
                .Find(x => x.EmployeeId == employeeId &&
                          x.IsActive &&
                          !x.IsDeleted &&
                          x.Status == ShiftChangeStatus.Approved &&
                          x.EffectiveFrom <= today &&
                          (!x.EffectiveTo.HasValue || x.EffectiveTo >= today))
                .SortByDescending(x => x.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        public async Task<List<EmployeeShift>> GetByShiftIdAsync(string shiftId)
        {
            return await _collection
                .Find(x => x.ShiftId == shiftId && !x.IsDeleted)
                .SortByDescending(x => x.EffectiveFrom)
                .ToListAsync();
        }

        public async Task<List<EmployeeShift>> GetPendingShiftChangesAsync()
        {
            return await _collection
                .Find(x => x.Status == ShiftChangeStatus.Pending && !x.IsDeleted)
                .SortBy(x => x.RequestedDate)
                .ToListAsync();
        }

        public async Task<List<EmployeeShift>> GetByStatusAsync(ShiftChangeStatus status)
        {
            return await _collection
                .Find(x => x.Status == status && !x.IsDeleted)
                .SortByDescending(x => x.RequestedDate)
                .ToListAsync();
        }

        public async Task<bool> HasActiveShiftAsync(string employeeId, DateTime effectiveDate, string? excludeId = null)
        {
            var filterBuilder = Builders<EmployeeShift>.Filter;
            var filters = new List<FilterDefinition<EmployeeShift>>
            {
                filterBuilder.Eq(x => x.EmployeeId, employeeId),
                filterBuilder.Eq(x => x.IsDeleted, false),
                filterBuilder.In(x => x.Status, new[] { ShiftChangeStatus.Pending, ShiftChangeStatus.Approved }),
                filterBuilder.Lte(x => x.EffectiveFrom, effectiveDate),
                filterBuilder.Or(
                    filterBuilder.Eq(x => x.EffectiveTo, null),
                    filterBuilder.Gte(x => x.EffectiveTo, effectiveDate)
                )
            };

            if (!string.IsNullOrEmpty(excludeId))
            {
                filters.Add(filterBuilder.Ne(x => x.Id, excludeId));
            }

            var combinedFilter = filterBuilder.And(filters);
            return await _collection.Find(combinedFilter).AnyAsync();
        }

        public async Task<bool> HasOverlappingShiftAssignmentAsync(string employeeId, DateTime effectiveFrom, DateTime? effectiveTo, string? excludeId = null)
        {
            var filterBuilder = Builders<EmployeeShift>.Filter;
            var filters = new List<FilterDefinition<EmployeeShift>>
            {
                filterBuilder.Eq(x => x.EmployeeId, employeeId),
                filterBuilder.Eq(x => x.IsDeleted, false),
                filterBuilder.In(x => x.Status, new[] { ShiftChangeStatus.Pending, ShiftChangeStatus.Approved })
            };

            if (effectiveTo.HasValue)
            {
                filters.Add(filterBuilder.Or(
                    filterBuilder.And(
                        filterBuilder.Lte(x => x.EffectiveFrom, effectiveFrom),
                        filterBuilder.Or(
                            filterBuilder.Eq(x => x.EffectiveTo, null),
                            filterBuilder.Gte(x => x.EffectiveTo, effectiveFrom)
                        )
                    ),
                   
                    filterBuilder.And(
                        filterBuilder.Lte(x => x.EffectiveFrom, effectiveTo),
                        filterBuilder.Or(
                            filterBuilder.Eq(x => x.EffectiveTo, null),
                            filterBuilder.Gte(x => x.EffectiveTo, effectiveFrom)
                        )
                    ),
                   
                    filterBuilder.And(
                        filterBuilder.Gte(x => x.EffectiveFrom, effectiveFrom),
                        filterBuilder.Lte(x => x.EffectiveFrom, effectiveTo)
                    )
                ));
            }
            else
            {
                filters.Add(filterBuilder.Or(
                    filterBuilder.Eq(x => x.EffectiveTo, null),
                    filterBuilder.Gte(x => x.EffectiveTo, effectiveFrom)
                ));
            }

            if (!string.IsNullOrEmpty(excludeId))
            {
                filters.Add(filterBuilder.Ne(x => x.Id, excludeId));
            }

            var combinedFilter = filterBuilder.And(filters);
            return await _collection.Find(combinedFilter).AnyAsync();
        }

        public async Task<int> GetPendingShiftChangesCountForEmployeeAsync(string employeeId)
        {
            var count = await _collection.CountDocumentsAsync(x =>
                x.EmployeeId == employeeId &&
                x.Status == ShiftChangeStatus.Pending &&
                !x.IsDeleted);

            return (int)count;
        }

        public async Task<List<EmployeeShift>> GetUpcomingShiftChangesAsync(int days = 7)
        {
            var today = DateTime.UtcNow.Date;
            var futureDate = today.AddDays(days);

            var filter = Builders<EmployeeShift>.Filter.And(
                Builders<EmployeeShift>.Filter.Eq(x => x.Status, ShiftChangeStatus.Approved),
                Builders<EmployeeShift>.Filter.Eq(x => x.IsDeleted, false),
                Builders<EmployeeShift>.Filter.Gte(x => x.EffectiveFrom, today),
                Builders<EmployeeShift>.Filter.Lte(x => x.EffectiveFrom, futureDate)
            );

            return await _collection.Find(filter).SortBy(x => x.EffectiveFrom).ToListAsync();
        }
    }
}