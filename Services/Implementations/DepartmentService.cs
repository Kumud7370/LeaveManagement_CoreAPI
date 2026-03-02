using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Department;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public DepartmentService(
            IDepartmentRepository departmentRepository,
            IEmployeeRepository employeeRepository)
        {
            _departmentRepository = departmentRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<DepartmentResponseDto?> CreateDepartmentAsync(CreateDepartmentRequestDto dto, Guid createdBy)
        {
            if (await _departmentRepository.IsDepartmentCodeExistsAsync(dto.DepartmentCode))
                return null;

            var department = new Department
            {
                DepartmentId = Guid.NewGuid(),
                DepartmentCode = dto.DepartmentCode,
                DepartmentName = dto.DepartmentName,
                Description = dto.Description,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            var createdDepartment = await _departmentRepository.CreateAsync(department);
            return await MapToResponseDtoAsync(createdDepartment);
        }

        public async Task<DepartmentResponseDto?> GetDepartmentByIdAsync(Guid departmentId)
        {
            var department = await _departmentRepository.GetByDepartmentIdAsync(departmentId);
            return department != null ? await MapToResponseDtoAsync(department) : null;
        }

        public async Task<DepartmentResponseDto?> GetDepartmentByCodeAsync(string departmentCode)
        {
            var department = await _departmentRepository.GetByDepartmentCodeAsync(departmentCode);
            return department != null ? await MapToResponseDtoAsync(department) : null;
        }

        public async Task<DepartmentDetailResponseDto?> GetDepartmentDetailsAsync(Guid departmentId)
        {
            var department = await _departmentRepository.GetDepartmentWithDetailsAsync(departmentId);
            if (department == null)
                return null;

            var detailDto = new DepartmentDetailResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentCode = department.DepartmentCode,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                IsActive = department.IsActive,
                DisplayOrder = department.DisplayOrder,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt,
                EmployeeCount = await _departmentRepository.GetEmployeeCountByDepartmentAsync(departmentId),
                ChildDepartmentCount = 0
            };

            var employees = await _employeeRepository.GetEmployeesByDepartmentAsync(departmentId.ToString());
            detailDto.Employees = employees.Select(e => new EmployeeSummaryDto
            {
                EmployeeId = Guid.Parse(e.Id),
                EmployeeCode = e.EmployeeCode,
                FullName = e.GetFullName(),
                Email = e.Email,
                ProfileImageUrl = e.ProfileImageUrl
            }).ToList();

            detailDto.AuditInfo = new AuditInfoDto
            {
                CreatedAt = department.CreatedAt,
                CreatedBy = department.CreatedBy,
                UpdatedAt = department.UpdatedAt,
                UpdatedBy = department.UpdatedBy
            };

            return detailDto;
        }

        public async Task<PaginatedResponseDto<DepartmentResponseDto>> GetFilteredDepartmentsAsync(DepartmentFilterRequestDto filter)
        {
            var (items, totalCount) = await _departmentRepository.GetFilteredDepartmentsAsync(filter);

            var departmentDtos = new List<DepartmentResponseDto>();
            foreach (var item in items)
                departmentDtos.Add(await MapToResponseDtoAsync(item));

            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            return new PaginatedResponseDto<DepartmentResponseDto>
            {
                Items = departmentDtos,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        public async Task<List<DepartmentResponseDto>> GetChildDepartmentsAsync(Guid parentDepartmentId)
        {
            return new List<DepartmentResponseDto>();
        }

        public async Task<List<DepartmentResponseDto>> GetRootDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetRootDepartmentsAsync();
            var result = new List<DepartmentResponseDto>();
            foreach (var department in departments)
                result.Add(await MapToResponseDtoAsync(department));
            return result;
        }

        public async Task<List<DepartmentResponseDto>> GetActiveDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetActiveDepartmentsAsync();
            var result = new List<DepartmentResponseDto>();
            foreach (var department in departments)
                result.Add(await MapToResponseDtoAsync(department));
            return result;
        }

        public async Task<List<DepartmentHierarchyDto>> GetDepartmentHierarchyAsync()
        {
            var allDepartments = await _departmentRepository.GetDepartmentHierarchyAsync();
            var hierarchy = new List<DepartmentHierarchyDto>();

            foreach (var dept in allDepartments)
            {
                var employeeCount = await _departmentRepository.GetEmployeeCountByDepartmentAsync(dept.DepartmentId);
                hierarchy.Add(new DepartmentHierarchyDto
                {
                    DepartmentId = dept.DepartmentId,
                    DepartmentCode = dept.DepartmentCode,
                    DepartmentName = dept.DepartmentName,
                    IsActive = dept.IsActive,
                    EmployeeCount = employeeCount,
                    Children = null
                });
            }

            return hierarchy;
        }

        public async Task<DepartmentResponseDto?> UpdateDepartmentAsync(UpdateDepartmentRequestDto dto, Guid updatedBy)
        {
            var department = await _departmentRepository.GetByDepartmentIdAsync(dto.DepartmentId);
            if (department == null)
                return null;

            if (dto.DepartmentCode != department.DepartmentCode)
            {
                if (await _departmentRepository.IsDepartmentCodeExistsAsync(dto.DepartmentCode, dto.DepartmentId))
                    return null;
            }

            department.DepartmentCode = dto.DepartmentCode;
            department.DepartmentName = dto.DepartmentName;
            department.Description = dto.Description;
            department.DisplayOrder = dto.DisplayOrder;
            department.IsActive = dto.IsActive;
            department.UpdatedBy = updatedBy;
            department.UpdatedAt = DateTime.UtcNow;

            var updated = await _departmentRepository.UpdateAsync(department.Id, department);
            return updated ? await MapToResponseDtoAsync(department) : null;
        }

        public async Task<bool> DeleteDepartmentAsync(Guid departmentId, Guid deletedBy)
        {
            var department = await _departmentRepository.GetByDepartmentIdAsync(departmentId);
            if (department == null)
                return false;

            if (!await CanDeleteDepartmentAsync(departmentId))
                return false;

            department.IsDeleted = true;
            department.DeletedAt = DateTime.UtcNow;
            department.DeletedBy = deletedBy;
            department.UpdatedBy = deletedBy;
            department.UpdatedAt = DateTime.UtcNow;

            return await _departmentRepository.DeleteAsync(department.Id);
        }

        public async Task<bool> ToggleDepartmentStatusAsync(Guid departmentId, Guid updatedBy)
        {
            var department = await _departmentRepository.GetByDepartmentIdAsync(departmentId);
            if (department == null)
                return false;

            department.IsActive = !department.IsActive;
            department.UpdatedBy = updatedBy;
            department.UpdatedAt = DateTime.UtcNow;

            return await _departmentRepository.UpdateAsync(department.Id, department);
        }

        public async Task<Dictionary<string, int>> GetDepartmentStatisticsAsync()
        {
            var allDepartments = await _departmentRepository.GetDepartmentHierarchyAsync();

            var statistics = new Dictionary<string, int>
            {
                { "TotalDepartments",    allDepartments.Count },
                { "ActiveDepartments",   allDepartments.Count(d => d.IsActive) },
                { "InactiveDepartments", allDepartments.Count(d => !d.IsActive) }
            };

            int departmentsWithEmployees = 0;
            foreach (var dept in allDepartments)
            {
                if (await _departmentRepository.HasEmployeesAsync(dept.DepartmentId))
                    departmentsWithEmployees++;
            }
            statistics.Add("DepartmentsWithEmployees", departmentsWithEmployees);

            return statistics;
        }

        public async Task<bool> CanDeleteDepartmentAsync(Guid departmentId)
        {
            if (await _departmentRepository.HasEmployeesAsync(departmentId))
                return false;
            return true;
        }

        private async Task<DepartmentResponseDto> MapToResponseDtoAsync(Department department)
        {
            return new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentCode = department.DepartmentCode,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                IsActive = department.IsActive,
                DisplayOrder = department.DisplayOrder,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt,
                EmployeeCount = await _departmentRepository.GetEmployeeCountByDepartmentAsync(department.DepartmentId),
                ChildDepartmentCount = 0
            };
        }
    }
}