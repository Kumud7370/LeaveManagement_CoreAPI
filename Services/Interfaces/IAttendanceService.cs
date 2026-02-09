using AttendanceManagementSystem.Models.DTOs.Attendance;
using AttendanceManagementSystem.Models.DTOs.Common;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IAttendanceService
    {
        // Check-in and Check-out
        Task<AttendanceResponseDto?> CheckInAsync(CheckInDto dto, string createdBy);
        Task<AttendanceResponseDto?> CheckOutAsync(CheckOutDto dto, string updatedBy);

        // Manual attendance management
        Task<AttendanceResponseDto?> MarkManualAttendanceAsync(ManualAttendanceDto dto, string createdBy);
        Task<AttendanceResponseDto?> UpdateAttendanceAsync(string id, ManualAttendanceDto dto, string updatedBy);

        // Retrieve attendance
        Task<AttendanceResponseDto?> GetAttendanceByIdAsync(string id);
        Task<AttendanceResponseDto?> GetTodayAttendanceAsync(string employeeId);
        Task<AttendanceResponseDto?> GetAttendanceByDateAsync(string employeeId, DateTime date);
        Task<PagedResultDto<AttendanceResponseDto>> GetFilteredAttendanceAsync(AttendanceFilterDto filter);
        Task<List<AttendanceResponseDto>> GetEmployeeAttendanceHistoryAsync(string employeeId, DateTime? startDate = null, DateTime? endDate = null);

        // Attendance summary and statistics
        Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(string employeeId, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetAttendanceStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<List<AttendanceResponseDto>> GetLateCheckInsAsync(DateTime startDate, DateTime endDate);
        Task<List<AttendanceResponseDto>> GetEarlyLeavesAsync(DateTime startDate, DateTime endDate);

        // Attendance operations
        Task<bool> DeleteAttendanceAsync(string id, string deletedBy);
        Task<bool> ApproveAttendanceAsync(string id, string approvedBy);

        // Automated tasks
        Task MarkAbsentEmployeesAsync(DateTime date);
    }
}