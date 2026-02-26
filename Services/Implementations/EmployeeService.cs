using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Employee;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IDesignationRepository _designationRepository;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            IDepartmentRepository departmentRepository,
            IDesignationRepository designationRepository)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
        }

        public async Task<EmployeeResponseDto?> CreateEmployeeAsync(CreateEmployeeDto dto, string createdBy)
        {
            if (await _employeeRepository.IsEmployeeCodeExistsAsync(dto.EmployeeCode))
                return null;

            if (await _employeeRepository.IsEmailExistsAsync(dto.Email))
                return null;

            var employee = new Employee
            {
                EmployeeCode = dto.EmployeeCode,
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                AlternatePhoneNumber = dto.AlternatePhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Address = dto.Address,
                DepartmentId = dto.DepartmentId,
                DesignationId = dto.DesignationId,
                ManagerId = dto.ManagerId,
                DateOfJoining = dto.DateOfJoining,
                DateOfLeaving = dto.DateOfLeaving,
                EmploymentType = dto.EmploymentType,
                EmployeeStatus = dto.EmployeeStatus,
                ProfileImageUrl = dto.ProfileImageUrl,
                BiometricId = dto.BiometricId,
                CreatedBy = createdBy
            };

            var createdEmployee = await _employeeRepository.CreateAsync(employee);
            return await MapToResponseDtoAsync(createdEmployee);
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(string id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            return employee != null ? await MapToResponseDtoAsync(employee) : null;
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByCodeAsync(string employeeCode)
        {
            var employee = await _employeeRepository.GetByEmployeeCodeAsync(employeeCode);
            return employee != null ? await MapToResponseDtoAsync(employee) : null;
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByEmailAsync(string email)
        {
            var employee = await _employeeRepository.GetByEmailAsync(email);
            return employee != null ? await MapToResponseDtoAsync(employee) : null;
        }

        public async Task<PagedResultDto<EmployeeResponseDto>> GetFilteredEmployeesAsync(EmployeeFilterDto filter)
        {
            var (items, totalCount) = await _employeeRepository.GetFilteredEmployeesAsync(filter);

            // Collect unique department and designation IDs to batch-resolve names
            var departmentIds = items
                .Select(e => e.DepartmentId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            var designationIds = items
                .Select(e => e.DesignationId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            // Batch fetch departments and designations
            var departmentMap = await GetDepartmentNamesAsync(departmentIds);
            var designationMap = await GetDesignationNamesAsync(designationIds);

            var employeeDtos = items.Select(e => MapToResponseDto(e, departmentMap, designationMap)).ToList();

            return new PagedResultDto<EmployeeResponseDto>(
                employeeDtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        public async Task<List<EmployeeResponseDto>> GetEmployeesByDepartmentAsync(string departmentId)
        {
            var employees = await _employeeRepository.GetEmployeesByDepartmentAsync(departmentId);
            var result = new List<EmployeeResponseDto>();
            foreach (var e in employees)
                result.Add(await MapToResponseDtoAsync(e));
            return result;
        }

        public async Task<List<EmployeeResponseDto>> GetEmployeesByManagerAsync(string managerId)
        {
            var employees = await _employeeRepository.GetEmployeesByManagerAsync(managerId);
            var result = new List<EmployeeResponseDto>();
            foreach (var e in employees)
                result.Add(await MapToResponseDtoAsync(e));
            return result;
        }

        public async Task<List<EmployeeResponseDto>> GetActiveEmployeesAsync()
        {
            var employees = await _employeeRepository.GetActiveEmployeesAsync();
            var result = new List<EmployeeResponseDto>();
            foreach (var e in employees)
                result.Add(await MapToResponseDtoAsync(e));
            return result;
        }

        public async Task<EmployeeResponseDto?> UpdateEmployeeAsync(string id, UpdateEmployeeDto dto, string updatedBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
                return null;

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != employee.Email)
            {
                if (await _employeeRepository.IsEmailExistsAsync(dto.Email, id))
                    return null;
            }

            if (!string.IsNullOrEmpty(dto.FirstName))
                employee.FirstName = dto.FirstName;

            if (dto.MiddleName != null)
                employee.MiddleName = dto.MiddleName;

            if (!string.IsNullOrEmpty(dto.LastName))
                employee.LastName = dto.LastName;

            if (!string.IsNullOrEmpty(dto.Email))
                employee.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                employee.PhoneNumber = dto.PhoneNumber;

            if (dto.AlternatePhoneNumber != null)
                employee.AlternatePhoneNumber = dto.AlternatePhoneNumber;

            if (dto.DateOfBirth.HasValue)
                employee.DateOfBirth = dto.DateOfBirth.Value;

            if (dto.Gender.HasValue)
                employee.Gender = dto.Gender.Value;

            if (dto.Address is not null)
                employee.Address = dto.Address;

            if (!string.IsNullOrEmpty(dto.DepartmentId))
                employee.DepartmentId = dto.DepartmentId;

            if (!string.IsNullOrEmpty(dto.DesignationId))
                employee.DesignationId = dto.DesignationId;

            if (dto.ManagerId != null)
                employee.ManagerId = dto.ManagerId;

            if (dto.DateOfJoining.HasValue)
                employee.DateOfJoining = dto.DateOfJoining.Value;

            if (dto.DateOfLeaving.HasValue)
                employee.DateOfLeaving = dto.DateOfLeaving;

            if (dto.EmploymentType.HasValue)
                employee.EmploymentType = dto.EmploymentType.Value;

            if (dto.EmployeeStatus.HasValue)
                employee.EmployeeStatus = dto.EmployeeStatus.Value;

            if (dto.ProfileImageUrl != null)
                employee.ProfileImageUrl = dto.ProfileImageUrl;

            if (dto.BiometricId != null)
                employee.BiometricId = dto.BiometricId;

            employee.UpdatedBy = updatedBy;

            var updated = await _employeeRepository.UpdateAsync(id, employee);
            return updated ? await MapToResponseDtoAsync(employee) : null;
        }

        public async Task<bool> DeleteEmployeeAsync(string id, string deletedBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
                return false;

            employee.UpdatedBy = deletedBy;
            employee.DeletedAt = DateTime.UtcNow;

            return await _employeeRepository.DeleteAsync(id);
        }

        public async Task<bool> ChangeEmployeeStatusAsync(string id, EmployeeStatus status, string updatedBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
                return false;

            employee.EmployeeStatus = status;
            employee.UpdatedBy = updatedBy;

            return await _employeeRepository.UpdateAsync(id, employee);
        }

        public async Task<Dictionary<string, int>> GetEmployeeStatisticsByStatusAsync()
        {
            var statistics = new Dictionary<string, int>();

            foreach (EmployeeStatus status in Enum.GetValues(typeof(EmployeeStatus)))
            {
                var count = await _employeeRepository.GetEmployeeCountByStatusAsync(status);
                statistics[status.ToString()] = count;
            }

            return statistics;
        }

        private async Task<EmployeeResponseDto> MapToResponseDtoAsync(Employee employee)
        {
            string? departmentName = null;
            string? designationName = null;

            if (!string.IsNullOrEmpty(employee.DepartmentId) && Guid.TryParse(employee.DepartmentId, out var deptGuid))
            {
                try
                {
                    var dept = await _departmentRepository.GetByDepartmentIdAsync(deptGuid);
                    departmentName = dept?.DepartmentName;
                }
                catch { /* leave null if not found */ }
            }

            if (!string.IsNullOrEmpty(employee.DesignationId))
            {
                try
                {
                    var desig = await _designationRepository.GetByIdAsync(employee.DesignationId);
                    designationName = desig?.DesignationName;
                }
                catch { /* leave null if not found */ }
            }

            return BuildDto(employee, departmentName, designationName);
        }
        private EmployeeResponseDto MapToResponseDto(
            Employee employee,
            Dictionary<string, string> departmentMap,
            Dictionary<string, string> designationMap)
        {
            departmentMap.TryGetValue(employee.DepartmentId ?? "", out var departmentName);
            designationMap.TryGetValue(employee.DesignationId ?? "", out var designationName);
            return BuildDto(employee, departmentName, designationName);
        }

        private EmployeeResponseDto BuildDto(Employee employee, string? departmentName, string? designationName)
        {
            return new EmployeeResponseDto
            {
                Id = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                LastName = employee.LastName,
                FullName = employee.GetFullName(),
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                AlternatePhoneNumber = employee.AlternatePhoneNumber,
                DateOfBirth = employee.DateOfBirth,
                Age = employee.GetAge(),
                Gender = employee.Gender,
                GenderName = employee.Gender.ToString(),
                Address = employee.Address,
                DepartmentId = employee.DepartmentId,
                DepartmentName = departmentName,       
                DesignationId = employee.DesignationId,
                DesignationName = designationName,     
                ManagerId = employee.ManagerId,
                DateOfJoining = employee.DateOfJoining,
                DateOfLeaving = employee.DateOfLeaving,
                EmploymentType = employee.EmploymentType,
                EmploymentTypeName = employee.EmploymentType.ToString(),
                EmployeeStatus = employee.EmployeeStatus,
                EmployeeStatusName = employee.EmployeeStatus.ToString(),
                ProfileImageUrl = employee.ProfileImageUrl,
                BiometricId = employee.BiometricId,
                IsCurrentlyEmployed = employee.IsCurrentlyEmployed(),
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt
            };
        }

        private async Task<Dictionary<string, string>> GetDepartmentNamesAsync(List<string> ids)
        {
            var map = new Dictionary<string, string>();
            foreach (var id in ids)
            {
                try
                {
                    if (!Guid.TryParse(id, out var deptGuid))
                        continue;

                    var dept = await _departmentRepository.GetByDepartmentIdAsync(deptGuid);
                    if (dept != null)
                        map[id] = dept.DepartmentName;
                }
                catch { /* skip if not found */ }
            }
            return map;
        }
        private async Task<Dictionary<string, string>> GetDesignationNamesAsync(List<string> ids)
        {
            var map = new Dictionary<string, string>();
            foreach (var id in ids)
            {
                try
                {
                    var desig = await _designationRepository.GetByIdAsync(id);
                    if (desig != null)
                        map[id] = desig.DesignationName;
                }
                catch { /* skip if not found */ }
            }
            return map;
        }
    }
}