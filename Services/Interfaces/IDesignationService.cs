using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Designation;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IDesignationService
    {
        Task<DesignationResponseDto?> CreateDesignationAsync(CreateDesignationDto dto, string createdBy);
        Task<DesignationResponseDto?> GetDesignationByIdAsync(string id);
        Task<DesignationResponseDto?> GetDesignationByCodeAsync(string designationCode);
        Task<PagedResultDto<DesignationResponseDto>> GetFilteredDesignationsAsync(DesignationFilterDto filter);
        Task<List<DesignationResponseDto>> GetActiveDesignationsAsync();
        Task<List<DesignationResponseDto>> GetDesignationsByLevelAsync(int level);
        Task<DesignationResponseDto?> UpdateDesignationAsync(string id, UpdateDesignationDto dto, string updatedBy);
        Task<bool> DeleteDesignationAsync(string id, string deletedBy);
        Task<bool> ToggleDesignationStatusAsync(string id, string updatedBy);
        Task<Dictionary<int, int>> GetDesignationStatisticsByLevelAsync();
    }
}