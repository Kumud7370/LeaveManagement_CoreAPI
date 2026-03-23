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

                // Marathi (primary)
                DepartmentNameMr = dto.DepartmentNameMr,
                DescriptionMr = dto.DescriptionMr,

                // English (optional)
                DepartmentName = dto.DepartmentName,
                Description = dto.Description,

                // Hindi (optional)
                DepartmentNameHi = dto.DepartmentNameHi,
                DescriptionHi = dto.DescriptionHi,

                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _departmentRepository.CreateAsync(department);
            return await MapToResponseDtoAsync(created);
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
            if (department == null) return null;

            var detailDto = new DepartmentDetailResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentCode = department.DepartmentCode,
                DepartmentNameMr = department.DepartmentNameMr,
                DepartmentName = department.DepartmentName,
                DepartmentNameHi = department.DepartmentNameHi,
                DescriptionMr = department.DescriptionMr,
                Description = department.Description,
                DescriptionHi = department.DescriptionHi,
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
                FullName = e.GetFullName("mr"),
                FullNameEn = !string.IsNullOrWhiteSpace(e.FirstName) ? e.GetFullName("en") : null,
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

            var dtos = new List<DepartmentResponseDto>();
            foreach (var item in items) dtos.Add(await MapToResponseDtoAsync(item));

            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            return new PaginatedResponseDto<DepartmentResponseDto>
            {
                Items = dtos,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        public async Task<List<DepartmentResponseDto>> GetChildDepartmentsAsync(Guid parentDepartmentId)
            => new List<DepartmentResponseDto>();

        public async Task<List<DepartmentResponseDto>> GetRootDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetRootDepartmentsAsync();
            var result = new List<DepartmentResponseDto>();
            foreach (var d in departments) result.Add(await MapToResponseDtoAsync(d));
            return result;
        }

        public async Task<List<DepartmentResponseDto>> GetActiveDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetActiveDepartmentsAsync();
            var result = new List<DepartmentResponseDto>();
            foreach (var d in departments) result.Add(await MapToResponseDtoAsync(d));
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
                    DepartmentNameMr = dept.DepartmentNameMr,
                    DepartmentName = dept.DepartmentName,
                    DepartmentNameHi = dept.DepartmentNameHi,
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
            if (department == null) return null;

            if (dto.DepartmentCode != department.DepartmentCode)
                if (await _departmentRepository.IsDepartmentCodeExistsAsync(dto.DepartmentCode, dto.DepartmentId))
                    return null;

            department.DepartmentCode = dto.DepartmentCode;

            // Marathi (primary)
            department.DepartmentNameMr = dto.DepartmentNameMr;
            department.DescriptionMr = dto.DescriptionMr;

            // English
            department.DepartmentName = dto.DepartmentName;
            department.Description = dto.Description;

            // Hindi
            department.DepartmentNameHi = dto.DepartmentNameHi;
            department.DescriptionHi = dto.DescriptionHi;

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
            if (department == null) return false;
            if (!await CanDeleteDepartmentAsync(departmentId)) return false;

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
            if (department == null) return false;
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

            int withEmployees = 0;
            foreach (var dept in allDepartments)
                if (await _departmentRepository.HasEmployeesAsync(dept.DepartmentId)) withEmployees++;

            statistics.Add("DepartmentsWithEmployees", withEmployees);
            return statistics;
        }

        public async Task<bool> CanDeleteDepartmentAsync(Guid departmentId)
            => !await _departmentRepository.HasEmployeesAsync(departmentId);

        private async Task<DepartmentResponseDto> MapToResponseDtoAsync(Department department)
        {
            return new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentCode = department.DepartmentCode,
                DepartmentNameMr = department.DepartmentNameMr,
                DepartmentName = department.DepartmentName,
                DepartmentNameHi = department.DepartmentNameHi,
                DescriptionMr = department.DescriptionMr,
                Description = department.Description,
                DescriptionHi = department.DescriptionHi,
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