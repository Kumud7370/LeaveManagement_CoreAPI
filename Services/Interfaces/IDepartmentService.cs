using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Department;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<DepartmentResponseDto?> CreateDepartmentAsync(CreateDepartmentRequestDto dto, Guid createdBy);
        Task<DepartmentResponseDto?> GetDepartmentByIdAsync(Guid departmentId);
        Task<DepartmentResponseDto?> GetDepartmentByCodeAsync(string departmentCode);
        Task<DepartmentDetailResponseDto?> GetDepartmentDetailsAsync(Guid departmentId);
        Task<PaginatedResponseDto<DepartmentResponseDto>> GetFilteredDepartmentsAsync(DepartmentFilterRequestDto filter);
        Task<List<DepartmentResponseDto>> GetChildDepartmentsAsync(Guid parentDepartmentId);
        Task<List<DepartmentResponseDto>> GetRootDepartmentsAsync();
        Task<List<DepartmentResponseDto>> GetActiveDepartmentsAsync();
        Task<List<DepartmentHierarchyDto>> GetDepartmentHierarchyAsync();
        Task<DepartmentResponseDto?> UpdateDepartmentAsync(UpdateDepartmentRequestDto dto, Guid updatedBy);
        Task<bool> DeleteDepartmentAsync(Guid departmentId, Guid deletedBy);
        Task<bool> ToggleDepartmentStatusAsync(Guid departmentId, Guid updatedBy);
        Task<Dictionary<string, int>> GetDepartmentStatisticsAsync();
        Task<bool> CanDeleteDepartmentAsync(Guid departmentId);
    }
}