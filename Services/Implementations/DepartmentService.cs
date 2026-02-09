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

            if (dto.ParentDepartmentId.HasValue)
            {
                var parentDepartment = await _departmentRepository.GetByDepartmentIdAsync(dto.ParentDepartmentId.Value);
                if (parentDepartment == null)
                    return null;
            }

            if (dto.HeadOfDepartment.HasValue)
            {
                var headEmployee = await _employeeRepository.GetByIdAsync(dto.HeadOfDepartment.Value.ToString());
                if (headEmployee == null)
                    return null;
            }

            var department = new Department
            {
                DepartmentId = Guid.NewGuid(),
                DepartmentCode = dto.DepartmentCode,
                DepartmentName = dto.DepartmentName,
                Description = dto.Description,
                HeadOfDepartment = dto.HeadOfDepartment,
                ParentDepartmentId = dto.ParentDepartmentId,
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
                HeadOfDepartment = department.HeadOfDepartment,
                ParentDepartmentId = department.ParentDepartmentId,
                IsActive = department.IsActive,
                DisplayOrder = department.DisplayOrder,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt,
                EmployeeCount = await _departmentRepository.GetEmployeeCountByDepartmentAsync(departmentId),
                ChildDepartmentCount = (await _departmentRepository.GetChildDepartmentsAsync(departmentId)).Count
            };

            if (department.ParentDepartmentId.HasValue)
            {
                var parentDepartment = await _departmentRepository.GetByDepartmentIdAsync(department.ParentDepartmentId.Value);
                detailDto.ParentDepartmentName = parentDepartment?.DepartmentName;
            }

            if (department.HeadOfDepartment.HasValue)
            {
                var headEmployee = await _employeeRepository.GetByIdAsync(department.HeadOfDepartment.Value.ToString());
                if (headEmployee != null)
                {
                    detailDto.HeadOfDepartmentName = headEmployee.GetFullName();
                    detailDto.DepartmentHead = new EmployeeSummaryDto
                    {
                        EmployeeId = Guid.Parse(headEmployee.Id),
                        EmployeeCode = headEmployee.EmployeeCode,
                        FullName = headEmployee.GetFullName(),
                        Email = headEmployee.Email,
                        ProfileImageUrl = headEmployee.ProfileImageUrl
                    };
                }
            }

            var childDepartments = await _departmentRepository.GetChildDepartmentsAsync(departmentId);
            detailDto.ChildDepartments = new List<DepartmentResponseDto>();
            foreach (var child in childDepartments)
            {
                detailDto.ChildDepartments.Add(await MapToResponseDtoAsync(child));
            }

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
            {
                departmentDtos.Add(await MapToResponseDtoAsync(item));
            }

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
            var departments = await _departmentRepository.GetChildDepartmentsAsync(parentDepartmentId);
            var result = new List<DepartmentResponseDto>();

            foreach (var department in departments)
            {
                result.Add(await MapToResponseDtoAsync(department));
            }

            return result;
        }

        public async Task<List<DepartmentResponseDto>> GetRootDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetRootDepartmentsAsync();
            var result = new List<DepartmentResponseDto>();

            foreach (var department in departments)
            {
                result.Add(await MapToResponseDtoAsync(department));
            }

            return result;
        }

        public async Task<List<DepartmentResponseDto>> GetActiveDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetActiveDepartmentsAsync();
            var result = new List<DepartmentResponseDto>();

            foreach (var department in departments)
            {
                result.Add(await MapToResponseDtoAsync(department));
            }

            return result;
        }

        public async Task<List<DepartmentHierarchyDto>> GetDepartmentHierarchyAsync()
        {
            var allDepartments = await _departmentRepository.GetDepartmentHierarchyAsync();

            var rootDepartments = allDepartments.Where(d => d.ParentDepartmentId == null).ToList();
            var hierarchy = new List<DepartmentHierarchyDto>();

            foreach (var root in rootDepartments)
            {
                var employeeCount = await _departmentRepository.GetEmployeeCountByDepartmentAsync(root.DepartmentId);

                hierarchy.Add(new DepartmentHierarchyDto
                {
                    DepartmentId = root.DepartmentId,
                    DepartmentCode = root.DepartmentCode,
                    DepartmentName = root.DepartmentName,
                    IsActive = root.IsActive,
                    EmployeeCount = employeeCount,
                    Children = await BuildHierarchyChildren(root.DepartmentId, allDepartments)
                });
            }

            return hierarchy;
        }

        private async Task<List<DepartmentHierarchyDto>> BuildHierarchyChildren(Guid parentId, List<Department> allDepartments)
        {
            var children = allDepartments.Where(d => d.ParentDepartmentId == parentId).ToList();
            var result = new List<DepartmentHierarchyDto>();

            foreach (var child in children)
            {
                var employeeCount = await _departmentRepository.GetEmployeeCountByDepartmentAsync(child.DepartmentId);

                result.Add(new DepartmentHierarchyDto
                {
                    DepartmentId = child.DepartmentId,
                    DepartmentCode = child.DepartmentCode,
                    DepartmentName = child.DepartmentName,
                    IsActive = child.IsActive,
                    EmployeeCount = employeeCount,
                    Children = await BuildHierarchyChildren(child.DepartmentId, allDepartments)
                });
            }

            return result.Count > 0 ? result : null;
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

            if (dto.ParentDepartmentId.HasValue)
            {
         
                if (dto.ParentDepartmentId.Value == dto.DepartmentId)
                    return null;

                var parentDepartment = await _departmentRepository.GetByDepartmentIdAsync(dto.ParentDepartmentId.Value);
                if (parentDepartment == null)
                    return null;

                if (await WouldCreateCircularHierarchy(dto.DepartmentId, dto.ParentDepartmentId.Value))
                    return null;
            }

            if (dto.HeadOfDepartment.HasValue)
            {
                var headEmployee = await _employeeRepository.GetByIdAsync(dto.HeadOfDepartment.Value.ToString());
                if (headEmployee == null)
                    return null;
            }

            department.DepartmentCode = dto.DepartmentCode;
            department.DepartmentName = dto.DepartmentName;
            department.Description = dto.Description;
            department.HeadOfDepartment = dto.HeadOfDepartment;
            department.ParentDepartmentId = dto.ParentDepartmentId;
            department.DisplayOrder = dto.DisplayOrder;
            department.IsActive = dto.IsActive;
            department.UpdatedBy = updatedBy;
            department.UpdatedAt = DateTime.UtcNow;

            var updated = await _departmentRepository.UpdateAsync(department.Id, department);
            return updated ? await MapToResponseDtoAsync(department) : null;
        }

        private async Task<bool> WouldCreateCircularHierarchy(Guid departmentId, Guid newParentId)
        {
            var currentId = newParentId;
            var maxDepth = 100; 
            var depth = 0;

            while (currentId != Guid.Empty && depth < maxDepth)
            {
                if (currentId == departmentId)
                    return true;

                var parent = await _departmentRepository.GetByDepartmentIdAsync(currentId);
                if (parent?.ParentDepartmentId == null)
                    break;

                currentId = parent.ParentDepartmentId.Value;
                depth++;
            }

            return false;
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
                { "TotalDepartments", allDepartments.Count },
                { "ActiveDepartments", allDepartments.Count(d => d.IsActive) },
                { "InactiveDepartments", allDepartments.Count(d => !d.IsActive) },
                { "RootDepartments", allDepartments.Count(d => d.ParentDepartmentId == null) },
                { "ChildDepartments", allDepartments.Count(d => d.ParentDepartmentId != null) }
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

            if (await _departmentRepository.HasChildDepartmentsAsync(departmentId))
                return false;

            return true;
        }

        private async Task<DepartmentResponseDto> MapToResponseDtoAsync(Department department)
        {
            var dto = new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentCode = department.DepartmentCode,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                HeadOfDepartment = department.HeadOfDepartment,
                ParentDepartmentId = department.ParentDepartmentId,
                IsActive = department.IsActive,
                DisplayOrder = department.DisplayOrder,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt,
                EmployeeCount = await _departmentRepository.GetEmployeeCountByDepartmentAsync(department.DepartmentId),
                ChildDepartmentCount = (await _departmentRepository.GetChildDepartmentsAsync(department.DepartmentId)).Count
            };

            if (department.ParentDepartmentId.HasValue)
            {
                var parentDepartment = await _departmentRepository.GetByDepartmentIdAsync(department.ParentDepartmentId.Value);
                dto.ParentDepartmentName = parentDepartment?.DepartmentName;
            }

            if (department.HeadOfDepartment.HasValue)
            {
                var headEmployee = await _employeeRepository.GetByIdAsync(department.HeadOfDepartment.Value.ToString());
                dto.HeadOfDepartmentName = headEmployee?.GetFullName();
            }

            return dto;
        }
    }
}