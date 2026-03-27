using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.Leave;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class LeaveRepository : BaseRepository<Leave>, ILeaveRepository
    {
        public LeaveRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<(List<Leave> Items, int TotalCount)> GetFilteredLeavesAsync(LeaveFilterDto filter)
        {
            var filterBuilder = Builders<Leave>.Filter;
            var filters = new List<FilterDefinition<Leave>>
            {
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            if (filter.EmployeeIds != null && filter.EmployeeIds.Any())
            {
                filters.Add(filterBuilder.In(x => x.EmployeeId, filter.EmployeeIds));
            }

            else if (!string.IsNullOrWhiteSpace(filter.EmployeeId))
            {
                filters.Add(filterBuilder.Eq(x => x.EmployeeId, filter.EmployeeId));
            }

            if (!string.IsNullOrWhiteSpace(filter.LeaveTypeId))
            {
                filters.Add(filterBuilder.Eq(x => x.LeaveTypeId, filter.LeaveTypeId));
            }

            if (filter.LeaveStatus.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.LeaveStatus, filter.LeaveStatus.Value));
            }

            if (filter.StartDateFrom.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.StartDate, filter.StartDateFrom.Value));
            }

            if (filter.StartDateTo.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.StartDate, filter.StartDateTo.Value));
            }

            if (filter.EndDateFrom.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.EndDate, filter.EndDateFrom.Value));
            }

            if (filter.EndDateTo.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.EndDate, filter.EndDateTo.Value));
            }

            if (filter.AppliedDateFrom.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.AppliedDate, filter.AppliedDateFrom.Value));
            }

            if (filter.AppliedDateTo.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.AppliedDate, filter.AppliedDateTo.Value));
            }

            if (filter.IsEmergencyLeave.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.IsEmergencyLeave, filter.IsEmergencyLeave.Value));
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchFilter = filterBuilder.Regex(x => x.Reason, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i"));
                filters.Add(searchFilter);
            }

            var combinedFilter = filterBuilder.And(filters);

            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            var sortBuilder = Builders<Leave>.Sort;
            SortDefinition<Leave> sort = filter.SortBy.ToLower() switch
            {
                "startdate" => filter.SortDescending ? sortBuilder.Descending(x => x.StartDate) : sortBuilder.Ascending(x => x.StartDate),
                "enddate" => filter.SortDescending ? sortBuilder.Descending(x => x.EndDate) : sortBuilder.Ascending(x => x.EndDate),
                "totaldays" => filter.SortDescending ? sortBuilder.Descending(x => x.TotalDays) : sortBuilder.Ascending(x => x.TotalDays),
                "leavestatus" => filter.SortDescending ? sortBuilder.Descending(x => x.LeaveStatus) : sortBuilder.Ascending(x => x.LeaveStatus),
                "createdat" => filter.SortDescending ? sortBuilder.Descending(x => x.CreatedAt) : sortBuilder.Ascending(x => x.CreatedAt),
                _ => filter.SortDescending ? sortBuilder.Descending(x => x.AppliedDate) : sortBuilder.Ascending(x => x.AppliedDate)
            };

            var items = await _collection
                .Find(combinedFilter)
                .Sort(sort)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Limit(filter.PageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        public async Task<List<Leave>> GetLeavesByEmployeeIdAsync(string employeeId)
        {
            return await _collection
                .Find(x => x.EmployeeId == employeeId && !x.IsDeleted)
                .SortByDescending(x => x.AppliedDate)
                .ToListAsync();
        }

        public async Task<List<Leave>> GetLeavesByLeaveTypeIdAsync(string leaveTypeId)
        {
            return await _collection
                .Find(x => x.LeaveTypeId == leaveTypeId && !x.IsDeleted)
                .SortByDescending(x => x.AppliedDate)
                .ToListAsync();
        }

        public async Task<List<Leave>> GetPendingLeavesAsync()
        {
            return await _collection
                .Find(x => x.LeaveStatus == LeaveStatus.Pending && !x.IsDeleted)
                .SortBy(x => x.AppliedDate)
                .ToListAsync();
        }

        public async Task<List<Leave>> GetLeavesByStatusAsync(LeaveStatus status)
        {
            return await _collection
                .Find(x => x.LeaveStatus == status && !x.IsDeleted)
                .SortByDescending(x => x.AppliedDate)
                .ToListAsync();
        }

        public async Task<List<Leave>> GetLeavesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var filter = Builders<Leave>.Filter.And(
                Builders<Leave>.Filter.Eq(x => x.IsDeleted, false),
                Builders<Leave>.Filter.Or(
                    Builders<Leave>.Filter.And(
                        Builders<Leave>.Filter.Gte(x => x.StartDate, startDate),
                        Builders<Leave>.Filter.Lte(x => x.StartDate, endDate)
                    ),
                    Builders<Leave>.Filter.And(
                        Builders<Leave>.Filter.Gte(x => x.EndDate, startDate),
                        Builders<Leave>.Filter.Lte(x => x.EndDate, endDate)
                    ),
                    Builders<Leave>.Filter.And(
                        Builders<Leave>.Filter.Lte(x => x.StartDate, startDate),
                        Builders<Leave>.Filter.Gte(x => x.EndDate, endDate)
                    )
                )
            );

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<Leave>> GetEmployeeLeavesInRangeAsync(string employeeId, DateTime startDate, DateTime endDate)
        {
            var filter = Builders<Leave>.Filter.And(
                Builders<Leave>.Filter.Eq(x => x.EmployeeId, employeeId),
                Builders<Leave>.Filter.Eq(x => x.IsDeleted, false),
                Builders<Leave>.Filter.Or(
                    Builders<Leave>.Filter.And(
                        Builders<Leave>.Filter.Gte(x => x.StartDate, startDate),
                        Builders<Leave>.Filter.Lte(x => x.StartDate, endDate)
                    ),
                    Builders<Leave>.Filter.And(
                        Builders<Leave>.Filter.Gte(x => x.EndDate, startDate),
                        Builders<Leave>.Filter.Lte(x => x.EndDate, endDate)
                    ),
                    Builders<Leave>.Filter.And(
                        Builders<Leave>.Filter.Lte(x => x.StartDate, startDate),
                        Builders<Leave>.Filter.Gte(x => x.EndDate, endDate)
                    )
                )
            );

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> HasOverlappingLeaveAsync(string employeeId, DateTime startDate, DateTime endDate, string? excludeLeaveId = null)
        {
            var filterBuilder = Builders<Leave>.Filter;
            var filters = new List<FilterDefinition<Leave>>
            {
                filterBuilder.Eq(x => x.EmployeeId, employeeId),
                filterBuilder.Eq(x => x.IsDeleted, false),
                filterBuilder.In(x => x.LeaveStatus, new[] { LeaveStatus.Pending, LeaveStatus.Pending, LeaveStatus.AdminApproved, LeaveStatus.NayabApproved,  LeaveStatus.FullyApproved}),
                filterBuilder.Or(
                    filterBuilder.And(
                        filterBuilder.Gte(x => x.StartDate, startDate),
                        filterBuilder.Lte(x => x.StartDate, endDate)
                    ),
                    filterBuilder.And(
                        filterBuilder.Gte(x => x.EndDate, startDate),
                        filterBuilder.Lte(x => x.EndDate, endDate)
                    ),
                    filterBuilder.And(
                        filterBuilder.Lte(x => x.StartDate, startDate),
                        filterBuilder.Gte(x => x.EndDate, endDate)
                    )
                )
            };

            if (!string.IsNullOrEmpty(excludeLeaveId))
            {
                filters.Add(filterBuilder.Ne(x => x.Id, excludeLeaveId));
            }

            var combinedFilter = filterBuilder.And(filters);
            return await _collection.Find(combinedFilter).AnyAsync();
        }

        public async Task<int> GetApprovedLeaveDaysForEmployeeInYearAsync(string employeeId, string leaveTypeId, int year)
        {
            var startOfYear = new DateTime(year, 1, 1);
            var endOfYear = new DateTime(year, 12, 31);

            var filter = Builders<Leave>.Filter.And(
                Builders<Leave>.Filter.Eq(x => x.EmployeeId, employeeId),
                Builders<Leave>.Filter.Eq(x => x.LeaveTypeId, leaveTypeId),
                Builders<Leave>.Filter.Eq(x => x.LeaveStatus, LeaveStatus.FullyApproved),
                Builders<Leave>.Filter.Eq(x => x.IsDeleted, false),
                Builders<Leave>.Filter.Or(
                    Builders<Leave>.Filter.And(
                        Builders<Leave>.Filter.Gte(x => x.StartDate, startOfYear),
                        Builders<Leave>.Filter.Lte(x => x.StartDate, endOfYear)
                    ),
                    Builders<Leave>.Filter.And(
                        Builders<Leave>.Filter.Gte(x => x.EndDate, startOfYear),
                        Builders<Leave>.Filter.Lte(x => x.EndDate, endOfYear)
                    )
                )
            );

            var leaves = await _collection.Find(filter).ToListAsync();
            return (int)leaves.Sum(x => x.TotalDays);
        }

        public async Task<List<Leave>> GetUpcomingLeavesAsync(int days = 7)
        {
            var today = DateTime.UtcNow.Date;
            var futureDate = today.AddDays(days);

            var filter = Builders<Leave>.Filter.And(
                Builders<Leave>.Filter.Eq(x => x.LeaveStatus, LeaveStatus.FullyApproved),
                Builders<Leave>.Filter.Eq(x => x.IsDeleted, false),
                Builders<Leave>.Filter.Gte(x => x.StartDate, today),
                Builders<Leave>.Filter.Lte(x => x.StartDate, futureDate)
            );

            return await _collection.Find(filter).SortBy(x => x.StartDate).ToListAsync();
        }
    }
}
