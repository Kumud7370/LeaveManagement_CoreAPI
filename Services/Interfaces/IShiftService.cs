using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Shift;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IShiftService
    {
        Task<ShiftResponseDto?> CreateShiftAsync(CreateShiftDto dto, string createdBy);
        Task<ShiftResponseDto?> GetShiftByIdAsync(string id);
        Task<ShiftResponseDto?> GetShiftByCodeAsync(string code);
        Task<PagedResultDto<ShiftResponseDto>> GetFilteredShiftsAsync(ShiftFilterDto filter);
        Task<List<ShiftResponseDto>> GetActiveShiftsAsync();
        Task<List<ShiftResponseDto>> GetNightShiftsAsync();
        Task<ShiftResponseDto?> UpdateShiftAsync(string id, UpdateShiftDto dto, string updatedBy);
        Task<bool> DeleteShiftAsync(string id, string deletedBy);
        Task<bool> ToggleShiftStatusAsync(string id, bool isActive, string updatedBy);
    }
}