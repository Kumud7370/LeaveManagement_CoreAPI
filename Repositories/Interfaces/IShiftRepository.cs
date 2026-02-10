using AttendanceManagementSystem.Models.DTOs.Shift;
using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IShiftRepository : IBaseRepository<Shift>
    {
        Task<Shift?> GetByCodeAsync(string code);
        Task<bool> IsCodeExistsAsync(string code, string? excludeId = null);
        Task<(List<Shift> Items, int TotalCount)> GetFilteredShiftsAsync(ShiftFilterDto filter);
        Task<List<Shift>> GetActiveShiftsAsync();
        Task<List<Shift>> GetNightShiftsAsync();
        Task<int> GetMaxDisplayOrderAsync();
        Task<bool> HasOverlappingShiftTimesAsync(TimeOnly startTime, TimeOnly endTime, string? excludeId = null);
    }
}