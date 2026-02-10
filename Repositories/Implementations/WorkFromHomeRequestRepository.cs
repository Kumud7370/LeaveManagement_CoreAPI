using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.DTOs.WorkFromHome;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class WorkFromHomeRequestRepository : BaseRepository<WorkFromHomeRequest>, IWorkFromHomeRequestRepository
    {
        public WorkFromHomeRequestRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<(List<WorkFromHomeRequest> Items, int TotalCount)> GetFilteredWfhRequestsAsync(WfhRequestFilterDto filter)
        {
            var filterBuilder = Builders<WorkFromHomeRequest>.Filter;
            var filters = new List<FilterDefinition<WorkFromHomeRequest>>
            {
                filterBuilder.Eq(x => x.IsDeleted, false)
            };

            // Employee filter
            if (!string.IsNullOrWhiteSpace(filter.EmployeeId))
            {
                filters.Add(filterBuilder.Eq(x => x.EmployeeId, filter.EmployeeId));
            }

            // Search term filter (can search by reason)
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchFilter = filterBuilder.Regex(x => x.Reason,
                    new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i"));
                filters.Add(searchFilter);
            }

            // Status filter
            if (filter.Status.HasValue)
            {
                filters.Add(filterBuilder.Eq(x => x.Status, filter.Status.Value));
            }

            // Start date range filter
            if (filter.StartDateFrom.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.StartDate, filter.StartDateFrom.Value));
            }

            if (filter.StartDateTo.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.StartDate, filter.StartDateTo.Value));
            }

            // End date range filter
            if (filter.EndDateFrom.HasValue)
            {
                filters.Add(filterBuilder.Gte(x => x.EndDate, filter.EndDateFrom.Value));
            }

            if (filter.EndDateTo.HasValue)
            {
                filters.Add(filterBuilder.Lte(x => x.EndDate, filter.EndDateTo.Value));
            }

            // Approved by filter
            if (!string.IsNullOrWhiteSpace(filter.ApprovedBy))
            {
                filters.Add(filterBuilder.Eq(x => x.ApprovedBy, filter.ApprovedBy));
            }

            var combinedFilter = filterBuilder.And(filters);

            // Get total count
            var totalCount = await _collection.CountDocumentsAsync(combinedFilter);

            // Sorting
            var sortBuilder = Builders<WorkFromHomeRequest>.Sort;
            SortDefinition<WorkFromHomeRequest> sort = filter.SortBy.ToLower() switch
            {
                "startdate" => filter.SortDescending ? sortBuilder.Descending(x => x.StartDate) : sortBuilder.Ascending(x => x.StartDate),
                "enddate" => filter.SortDescending ? sortBuilder.Descending(x => x.EndDate) : sortBuilder.Ascending(x => x.EndDate),
                "status" => filter.SortDescending ? sortBuilder.Descending(x => x.Status) : sortBuilder.Ascending(x => x.Status),
                "createdat" => filter.SortDescending ? sortBuilder.Descending(x => x.CreatedAt) : sortBuilder.Ascending(x => x.CreatedAt),
                _ => filter.SortDescending ? sortBuilder.Descending(x => x.StartDate) : sortBuilder.Ascending(x => x.StartDate)
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

        public async Task<List<WorkFromHomeRequest>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _collection
                .Find(x => x.EmployeeId == employeeId && !x.IsDeleted)
                .SortByDescending(x => x.StartDate)
                .ToListAsync();
        }

        public async Task<List<WorkFromHomeRequest>> GetPendingRequestsAsync()
        {
            return await _collection
                .Find(x => x.Status == ApprovalStatus.Pending && !x.IsDeleted)
                .SortBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<WorkFromHomeRequest>> GetApprovedRequestsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var filter = Builders<WorkFromHomeRequest>.Filter.And(
                Builders<WorkFromHomeRequest>.Filter.Eq(x => x.Status, ApprovalStatus.Approved),
                Builders<WorkFromHomeRequest>.Filter.Eq(x => x.IsDeleted, false),
                Builders<WorkFromHomeRequest>.Filter.Or(
                    Builders<WorkFromHomeRequest>.Filter.And(
                        Builders<WorkFromHomeRequest>.Filter.Lte(x => x.StartDate, endDate),
                        Builders<WorkFromHomeRequest>.Filter.Gte(x => x.EndDate, startDate)
                    )
                )
            );

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> HasOverlappingRequestAsync(string employeeId, DateTime startDate, DateTime endDate, string? excludeId = null)
        {
            var filterBuilder = Builders<WorkFromHomeRequest>.Filter;

            var filter = filterBuilder.And(
                filterBuilder.Eq(x => x.EmployeeId, employeeId),
                filterBuilder.Eq(x => x.IsDeleted, false),
                filterBuilder.In(x => x.Status, new[] { ApprovalStatus.Pending, ApprovalStatus.Approved }),
                filterBuilder.Or(
                    // New request starts during existing request
                    filterBuilder.And(
                        filterBuilder.Lte(x => x.StartDate, startDate),
                        filterBuilder.Gte(x => x.EndDate, startDate)
                    ),
                    // New request ends during existing request
                    filterBuilder.And(
                        filterBuilder.Lte(x => x.StartDate, endDate),
                        filterBuilder.Gte(x => x.EndDate, endDate)
                    ),
                    // New request encompasses existing request
                    filterBuilder.And(
                        filterBuilder.Gte(x => x.StartDate, startDate),
                        filterBuilder.Lte(x => x.EndDate, endDate)
                    )
                )
            );

            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = filterBuilder.And(filter, filterBuilder.Ne(x => x.Id, excludeId));
            }

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<List<WorkFromHomeRequest>> GetActiveWfhRequestsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var filter = Builders<WorkFromHomeRequest>.Filter.And(
                Builders<WorkFromHomeRequest>.Filter.Eq(x => x.Status, ApprovalStatus.Approved),
                Builders<WorkFromHomeRequest>.Filter.Eq(x => x.IsDeleted, false),
                Builders<WorkFromHomeRequest>.Filter.Lte(x => x.StartDate, today),
                Builders<WorkFromHomeRequest>.Filter.Gte(x => x.EndDate, today)
            );

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<int> GetWfhRequestCountByStatusAsync(ApprovalStatus status)
        {
            return (int)await _collection
                .CountDocumentsAsync(x => x.Status == status && !x.IsDeleted);
        }

        public async Task<List<WorkFromHomeRequest>> GetEmployeeWfhRequestsByDateRangeAsync(string employeeId, DateTime startDate, DateTime endDate)
        {
            var filter = Builders<WorkFromHomeRequest>.Filter.And(
                Builders<WorkFromHomeRequest>.Filter.Eq(x => x.EmployeeId, employeeId),
                Builders<WorkFromHomeRequest>.Filter.Eq(x => x.IsDeleted, false),
                Builders<WorkFromHomeRequest>.Filter.Or(
                    Builders<WorkFromHomeRequest>.Filter.And(
                        Builders<WorkFromHomeRequest>.Filter.Lte(x => x.StartDate, endDate),
                        Builders<WorkFromHomeRequest>.Filter.Gte(x => x.EndDate, startDate)
                    )
                )
            );

            return await _collection.Find(filter).SortBy(x => x.StartDate).ToListAsync();
        }
    }
}