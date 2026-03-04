using AttendanceManagementSystem.Models.DTOs.Holiday;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IHolidayRepository : IBaseRepository<Holiday>
    {
        Task<Holiday?> GetByNameAndDateAsync(string holidayName, DateTime holidayDate);
        Task<bool> IsHolidayExistsAsync(string holidayName, DateTime holidayDate, string? excludeId = null);
        Task<(List<Holiday> Items, int TotalCount)> GetFilteredHolidaysAsync(HolidayFilterDto filter);
        Task<List<Holiday>> GetHolidaysByDepartmentAsync(string departmentId);
        Task<List<Holiday>> GetHolidaysByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Holiday>> GetUpcomingHolidaysAsync(int count = 10);
        Task<List<Holiday>> GetHolidaysByYearAsync(int year);
        Task<List<Holiday>> GetHolidaysByMonthAsync(int year, int month);
        Task<List<Holiday>> GetHolidaysByTypeAsync(HolidayType holidayType);
        Task<int> GetHolidayCountByTypeAsync(HolidayType holidayType);
        Task<bool> IsHolidayOnDateAsync(DateTime date);
        Task<bool> SoftDeleteAsync(string id, string deletedBy);
        Task<bool> UpdateHolidayFieldsAsync(string id, Holiday holiday);
    }
}