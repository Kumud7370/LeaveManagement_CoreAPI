using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.Attendance;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class AttendanceRepository : BaseRepository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<Attendance?> GetByEmployeeAndDateAsync(string employeeId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _collection.Find(x =>
                x.EmployeeId == employeeId &&
                x.AttendanceDate >= startOfDay &&
                x.AttendanceDate < endOfDay &&
                !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var filterBuilder = Builders<Attendance>.Filter;
            var filters = new List<FilterDefinition<Attendance>>
            {
                filterBuilder.Eq(x => x.EmployeeId, employeeId),
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            if (startDate.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.AttendanceDate, startDate.Value.Date));
            }

            if (endDate.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.AttendanceDate, endDate.Value.Date.AddDays(1)));
            }

            var combinedFilter = filterBuilder.And(filters);

            return await _collection.Find(combinedFilter)
                .SortByDescending(x => x.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _collection.Find(x =>
                x.AttendanceDate >= startDate.Date &&
                x.AttendanceDate <= endDate.Date.AddDays(1) &&
                !x.IsDeleted)
                .SortByDescending(x => x.AttendanceDate)
                .ToListAsync();
        }

        public async Task<(List<Attendance> Items, int TotalCount)> GetFilteredAttendanceAsync(AttendanceFilterDto filter)
        {
            var filterBuilder = Builders<Attendance>.Filter;
            var filters = new List<FilterDefinition<Attendance>>
            {
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            // Employee filter
            if (!string.IsNullOrWhiteSpace(filter.EmployeeId))
            {
                filters.Add(filterBuilder.Eq(x => x.EmployeeId, filter.EmployeeId));
            }

            // Date range filter
            if (filter.StartDate.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.AttendanceDate, filter.StartDate.Value.Date));
            }

            if (filter.EndDate.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.AttendanceDate, filter.EndDate.Value.Date.AddDays(1)));
            }

            // Status filter
            if (filter.Status.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.Status, filter.Status.Value));
            }

            // Late filter
            if (filter.IsLate.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.IsLate, filter.IsLate.Value));
            }

            // Early leave filter
            if (filter.IsEarlyLeave.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.IsEarlyLeave, filter.IsEarlyLeave.Value));
            }

            // Check-in method filter
            if (filter.CheckInMethod.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.CheckInMethod, filter.CheckInMethod.Value));
            }

            var combinedFilter = filterBuilder.And(filters);

            // Get total count
            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            // Sorting
            var sortBuilder = Builders<Attendance>.Sort;
            SortDefinition<Attendance> sort = filter.SortBy.ToLower() switch
            {
                "employeeid" => filter.SortDescending ? sortBuilder.Descending(x => x.EmployeeId) : sortBuilder.Ascending(x => x.EmployeeId),
                "checkintime" => filter.SortDescending ? sortBuilder.Descending(x => x.CheckInTime) : sortBuilder.Ascending(x => x.CheckInTime),
                "checkouttime" => filter.SortDescending ? sortBuilder.Descending(x => x.CheckOutTime) : sortBuilder.Ascending(x => x.CheckOutTime),
                "workinghours" => filter.SortDescending ? sortBuilder.Descending(x => x.WorkingHours) : sortBuilder.Ascending(x => x.WorkingHours),
                "status" => filter.SortDescending ? sortBuilder.Descending(x => x.Status) : sortBuilder.Ascending(x => x.Status),
                _ => filter.SortDescending ? sortBuilder.Descending(x => x.AttendanceDate) : sortBuilder.Ascending(x => x.AttendanceDate)
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

        public async Task<bool> HasCheckedInTodayAsync(string employeeId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var attendance = await _collection.Find(x =>
                x.EmployeeId == employeeId &&
                x.AttendanceDate >= startOfDay &&
                x.AttendanceDate < endOfDay &&
                !x.IsDeleted)
                .FirstOrDefaultAsync();

            return attendance?.CheckInTime != null;
        }

        public async Task<bool> HasCheckedOutTodayAsync(string employeeId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var attendance = await _collection.Find(x =>
                x.EmployeeId == employeeId &&
                x.AttendanceDate >= startOfDay &&
                x.AttendanceDate < endOfDay &&
                !x.IsDeleted)
                .FirstOrDefaultAsync();

            return attendance?.CheckOutTime != null;
        }

        public async Task<List<Attendance>> GetLateCheckInsAsync(DateTime startDate, DateTime endDate)
        {
            return await _collection.Find(x =>
                x.AttendanceDate >= startDate.Date &&
                x.AttendanceDate <= endDate.Date.AddDays(1) &&
                x.IsLate &&
                !x.IsDeleted)
                .SortByDescending(x => x.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetEarlyLeavesAsync(DateTime startDate, DateTime endDate)
        {
            return await _collection.Find(x =>
                x.AttendanceDate >= startDate.Date &&
                x.AttendanceDate <= endDate.Date.AddDays(1) &&
                x.IsEarlyLeave &&
                !x.IsDeleted)
                .SortByDescending(x => x.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetByDepartmentAsync(string departmentId, DateTime startDate, DateTime endDate)
        {
            // Note: This requires joining with Employee collection
            // For now, returning empty list - will be implemented in service layer
            return new List<Attendance>();
        }

        public async Task<int> GetAttendanceCountByStatusAsync(AttendanceStatus status, DateTime startDate, DateTime endDate)
        {
            return (int)await _collection.CountDocumentsAsync(x =>
                x.Status == status &&
                x.AttendanceDate >= startDate.Date &&
                x.AttendanceDate <= endDate.Date.AddDays(1) &&
                !x.IsDeleted);
        }

        public async Task<Dictionary<AttendanceStatus, int>> GetAttendanceStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var statistics = new Dictionary<AttendanceStatus, int>();

            foreach (AttendanceStatus status in Enum.GetValues(typeof(AttendanceStatus)))
            {
                var count = await GetAttendanceCountByStatusAsync(status, startDate, endDate);
                statistics[status] = count;
            }

            return statistics;
        }

        public async Task<List<Attendance>> GetAbsentEmployeesAsync(DateTime date, List<string> allEmployeeIds)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            // Get employees who have attendance records for the date
            var presentEmployeeIds = await _collection
                .Find(x => x.AttendanceDate >= startOfDay && x.AttendanceDate < endOfDay && !x.IsDeleted)
                .Project(x => x.EmployeeId)
                .ToListAsync();

            // Find employees without attendance records
            var absentEmployeeIds = allEmployeeIds.Except(presentEmployeeIds).ToList();

            // Create attendance records for absent employees
            var absentRecords = absentEmployeeIds.Select(employeeId => new Attendance
            {
                EmployeeId = employeeId,
                AttendanceDate = startOfDay,
                Status = AttendanceStatus.Absent,
                CreatedBy = "System"
            }).ToList();

            return absentRecords;
        }
    }
}