using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Holiday;
using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IHolidayService
    {
        Task<HolidayResponseDto?> CreateHolidayAsync(CreateHolidayDto dto, string createdBy);
        Task<HolidayResponseDto?> GetHolidayByIdAsync(string id);
        Task<PagedResultDto<HolidayResponseDto>> GetFilteredHolidaysAsync(HolidayFilterDto filter);
        Task<List<HolidayResponseDto>> GetHolidaysByDepartmentAsync(string departmentId);
        Task<List<HolidayResponseDto>> GetHolidaysByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<HolidayResponseDto>> GetUpcomingHolidaysAsync(int count = 10);
        Task<List<HolidayResponseDto>> GetHolidaysByYearAsync(int year);
        Task<List<HolidayResponseDto>> GetHolidaysByMonthAsync(int year, int month);
        Task<List<HolidayResponseDto>> GetHolidaysByTypeAsync(HolidayType holidayType);
        Task<HolidayResponseDto?> UpdateHolidayAsync(string id, UpdateHolidayDto dto, string updatedBy);
        Task<bool> DeleteHolidayAsync(string id, string deletedBy);
        Task<bool> IsHolidayOnDateAsync(DateTime date);
        Task<Dictionary<string, int>> GetHolidayStatisticsByTypeAsync();
    }
}