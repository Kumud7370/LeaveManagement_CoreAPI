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

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
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
            return MapToResponseDto(createdEmployee);
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(string id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            return employee != null ? MapToResponseDto(employee) : null;
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByCodeAsync(string employeeCode)
        {
            var employee = await _employeeRepository.GetByEmployeeCodeAsync(employeeCode);
            return employee != null ? MapToResponseDto(employee) : null;
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByEmailAsync(string email)
        {
            var employee = await _employeeRepository.GetByEmailAsync(email);
            return employee != null ? MapToResponseDto(employee) : null;
        }

        public async Task<PagedResultDto<EmployeeResponseDto>> GetFilteredEmployeesAsync(EmployeeFilterDto filter)
        {
            var (items, totalCount) = await _employeeRepository.GetFilteredEmployeesAsync(filter);

            var employeeDtos = items.Select(MapToResponseDto).ToList();

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
            return employees.Select(MapToResponseDto).ToList();
        }

        public async Task<List<EmployeeResponseDto>> GetEmployeesByManagerAsync(string managerId)
        {
            var employees = await _employeeRepository.GetEmployeesByManagerAsync(managerId);
            return employees.Select(MapToResponseDto).ToList();
        }

        public async Task<List<EmployeeResponseDto>> GetActiveEmployeesAsync()
        {
            var employees = await _employeeRepository.GetActiveEmployeesAsync();
            return employees.Select(MapToResponseDto).ToList();
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
            return updated ? MapToResponseDto(employee) : null;
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

        private EmployeeResponseDto MapToResponseDto(Employee employee)
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
                DesignationId = employee.DesignationId,
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
    }
}