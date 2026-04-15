using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Employee;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;
using System.Diagnostics;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IDesignationRepository _designationRepository;
        private readonly IAssignmentHistoryRepository _assignmentHistoryRepository;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            IDepartmentRepository departmentRepository,
            IDesignationRepository designationRepository,
            IAssignmentHistoryRepository assignmentHistoryRepository)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _assignmentHistoryRepository = assignmentHistoryRepository;
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

                // Marathi (primary — required)
                FirstNameMr = dto.FirstNameMr,
                MiddleNameMr = dto.MiddleNameMr,
                LastNameMr = dto.LastNameMr,

                // English (optional)
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,

                // Hindi (optional)
                FirstNameHi = dto.FirstNameHi,
                MiddleNameHi = dto.MiddleNameHi,
                LastNameHi = dto.LastNameHi,

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

            var created = await _employeeRepository.CreateAsync(employee);
            return await MapToResponseDtoAsync(created);
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

            var departmentIds = items.Select(e => e.DepartmentId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            var designationIds = items.Select(e => e.DesignationId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

            var departmentMap = await GetDepartmentNamesAsync(departmentIds);
            var designationMap = await GetDesignationNamesAsync(designationIds);

            var dtos = new List<EmployeeResponseDto>();

            foreach (var e in items)
            {
                dtos.Add(await MapToResponseDtoAsync(e));
            }
            return new PagedResultDto<EmployeeResponseDto>(dtos, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<List<EmployeeResponseDto>> GetEmployeesByDepartmentAsync(string departmentId)
        {
            var employees = await _employeeRepository.GetEmployeesByDepartmentAsync(departmentId);
            var result = new List<EmployeeResponseDto>();
            foreach (var e in employees) result.Add(await MapToResponseDtoAsync(e));
            return result;
        }

        public async Task<List<EmployeeResponseDto>> GetEmployeesByManagerAsync(string managerId)
        {
            var employees = await _employeeRepository.GetEmployeesByManagerAsync(managerId);
            var result = new List<EmployeeResponseDto>();
            foreach (var e in employees) result.Add(await MapToResponseDtoAsync(e));
            return result;
        }

        public async Task<List<EmployeeResponseDto>> GetActiveEmployeesAsync()
        {
            var employees = await _employeeRepository.GetActiveEmployeesAsync();
            var result = new List<EmployeeResponseDto>();
            foreach (var e in employees) result.Add(await MapToResponseDtoAsync(e));
            return result;
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByUserIdAsync(string userId)
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            return employee != null ? await MapToResponseDtoAsync(employee) : null;
        }

        public async Task<EmployeeResponseDto?> UpdateEmployeeAsync(string id, UpdateEmployeeDto dto, string updatedBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return null;

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != employee.Email)
                if (await _employeeRepository.IsEmailExistsAsync(dto.Email, id))
                    return null;

            // Marathi names — only update if provided
            if (!string.IsNullOrWhiteSpace(dto.FirstNameMr)) employee.FirstNameMr = dto.FirstNameMr;
            if (dto.MiddleNameMr != null) employee.MiddleNameMr = dto.MiddleNameMr;
            if (!string.IsNullOrWhiteSpace(dto.LastNameMr)) employee.LastNameMr = dto.LastNameMr;

            // English names
            if (dto.FirstName != null) employee.FirstName = dto.FirstName;
            if (dto.MiddleName != null) employee.MiddleName = dto.MiddleName;
            if (dto.LastName != null) employee.LastName = dto.LastName;

            // Hindi names
            if (dto.FirstNameHi != null) employee.FirstNameHi = dto.FirstNameHi;
            if (dto.MiddleNameHi != null) employee.MiddleNameHi = dto.MiddleNameHi;
            if (dto.LastNameHi != null) employee.LastNameHi = dto.LastNameHi;

            if (!string.IsNullOrEmpty(dto.Email)) employee.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) employee.PhoneNumber = dto.PhoneNumber;
            if (dto.AlternatePhoneNumber != null) employee.AlternatePhoneNumber = dto.AlternatePhoneNumber;
            if (dto.DateOfBirth.HasValue) employee.DateOfBirth = dto.DateOfBirth.Value;
            if (dto.Gender.HasValue) employee.Gender = dto.Gender.Value;
            if (dto.Address != null) employee.Address = dto.Address;
            if (!string.IsNullOrEmpty(dto.DepartmentId)) employee.DepartmentId = dto.DepartmentId;
            if (!string.IsNullOrEmpty(dto.DesignationId)) employee.DesignationId = dto.DesignationId;
            if (dto.ManagerId != null) employee.ManagerId = dto.ManagerId;
            if (dto.DateOfJoining.HasValue) employee.DateOfJoining = dto.DateOfJoining.Value;
            if (dto.DateOfLeaving.HasValue) employee.DateOfLeaving = dto.DateOfLeaving.Value;
            if (dto.EmploymentType.HasValue) employee.EmploymentType = dto.EmploymentType.Value;
            if (dto.EmployeeStatus.HasValue) employee.EmployeeStatus = dto.EmployeeStatus.Value;
            if (dto.ProfileImageUrl != null) employee.ProfileImageUrl = dto.ProfileImageUrl;
            if (dto.BiometricId != null) employee.BiometricId = dto.BiometricId;

            employee.UpdatedBy = updatedBy;
            employee.UpdatedAt = DateTime.UtcNow;

            var updated = await _employeeRepository.UpdateAsync(id, employee);
            return updated ? await MapToResponseDtoAsync(employee) : null;
        }

        public async Task<bool> DeleteEmployeeAsync(string id, string deletedBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return false;
            employee.IsDeleted = true;
            employee.DeletedAt = DateTime.UtcNow;
            employee.UpdatedBy = deletedBy;
            employee.UpdatedAt = DateTime.UtcNow;
            return await _employeeRepository.DeleteAsync(id);
        }

        public async Task<bool> ChangeEmployeeStatusAsync(string id, EmployeeStatus status, string updatedBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return false;
            employee.EmployeeStatus = status;
            employee.UpdatedBy = updatedBy;
            employee.UpdatedAt = DateTime.UtcNow;
            return await _employeeRepository.UpdateAsync(id, employee);
        }

        public async Task<Dictionary<string, int>> GetEmployeeStatisticsByStatusAsync()
        {
            var employees = await _employeeRepository.GetActiveEmployeesAsync();
            return employees
                .GroupBy(e => e.EmployeeStatus.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<EmployeeResponseDto?> ReassignEmployeeAsync(string id, ReassignEmployeeDto dto, string changedBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return null;

            await _assignmentHistoryRepository.CreateAsync(new EmployeeAssignmentHistory
            {
                EmployeeId = employee.Id,
                FromDepartmentId = employee.DepartmentId,
                ToDepartmentId = dto.ToDepartmentId,
                FromDesignationId = employee.DesignationId,
                ToDesignationId = dto.ToDesignationId,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow,
                Reason = dto.Reason
            });

            employee.DepartmentId = dto.ToDepartmentId;
            employee.DesignationId = dto.ToDesignationId;
            employee.UpdatedBy = changedBy;
            employee.UpdatedAt = DateTime.UtcNow;

            var updated = await _employeeRepository.UpdateAsync(id, employee);
            return updated ? await MapToResponseDtoAsync(employee) : null;
        }

        public async Task<List<AssignmentHistoryResponseDto>> GetAssignmentHistoryAsync(string employeeId)
        {
            var history = await _assignmentHistoryRepository.GetByEmployeeIdAsync(employeeId);
            var result = new List<AssignmentHistoryResponseDto>();

            foreach (var h in history)
            {
                string? fromDeptName = null, toDeptName = null;
                string? fromDesigName = null, toDesigName = null;

                if (!string.IsNullOrEmpty(h.FromDepartmentId) && Guid.TryParse(h.FromDepartmentId, out var fromDeptGuid))
                {
                    var dept = await _departmentRepository.GetByDepartmentIdAsync(fromDeptGuid);
                    fromDeptName = dept?.DepartmentName;
                }
                if (Guid.TryParse(h.ToDepartmentId, out var toDeptGuid))
                {
                    var dept = await _departmentRepository.GetByDepartmentIdAsync(toDeptGuid);
                    toDeptName = dept?.DepartmentName;
                }
                if (!string.IsNullOrEmpty(h.FromDesignationId))
                {
                    var desig = await _designationRepository.GetByIdAsync(h.FromDesignationId);
                    fromDesigName = desig?.DesignationName;
                }
                if (!string.IsNullOrEmpty(h.ToDesignationId))
                {
                    var desig = await _designationRepository.GetByIdAsync(h.ToDesignationId);
                    toDesigName = desig?.DesignationName;
                }

                result.Add(new AssignmentHistoryResponseDto
                {
                    Id = h.Id,
                    EmployeeId = h.EmployeeId,
                    FromDepartmentId = h.FromDepartmentId,
                    FromDepartmentName = fromDeptName,
                    ToDepartmentId = h.ToDepartmentId,
                    ToDepartmentName = toDeptName ?? string.Empty,
                    FromDesignationId = h.FromDesignationId,
                    FromDesignationName = fromDesigName,
                    ToDesignationId = h.ToDesignationId,
                    ToDesignationName = toDesigName ?? string.Empty,
                    ChangedBy = h.ChangedBy,
                    ChangedAt = h.ChangedAt,
                    Reason = h.Reason
                });
            }
            return result;
        }

        // ── Bulk Reassign ─────────────────────────────────────────────────────
        public async Task<BulkReassignResultDto> BulkReassignEmployeesAsync(BulkReassignEmployeeDto dto, string changedBy)
        {
            var result = new BulkReassignResultDto { TotalRequested = dto.EmployeeIds.Count };
            if (dto.EmployeeIds.Count == 0) return result;

            var employees = new List<Employee>();
            foreach (var empId in dto.EmployeeIds)
            {
                var emp = await _employeeRepository.GetByIdAsync(empId);
                if (emp == null || emp.IsDeleted) { result.FailedIds.Add(empId); result.Failed++; }
                else employees.Add(emp);
            }

            if (employees.Count == 0) return result;

            var historyTasks = employees.Select(emp =>
                _assignmentHistoryRepository.CreateAsync(new EmployeeAssignmentHistory
                {
                    EmployeeId = emp.Id,
                    FromDepartmentId = emp.DepartmentId,
                    ToDepartmentId = dto.ToDepartmentId,
                    FromDesignationId = emp.DesignationId,
                    ToDesignationId = dto.ToDesignationId,
                    ChangedBy = changedBy,
                    ChangedAt = DateTime.UtcNow,
                    Reason = dto.Reason
                }));
            await Task.WhenAll(historyTasks);

            var validIds = employees.Select(e => e.Id);
            var modifiedCount = await _employeeRepository.BulkReassignAsync(validIds, dto.ToDepartmentId, dto.ToDesignationId, changedBy);
            result.Succeeded = (int)modifiedCount;
            var notModified = employees.Count - result.Succeeded;
            if (notModified > 0) result.Failed += notModified;

            return result;
        }

        // ── Mapping helpers ───────────────────────────────────────────────────
        private async Task<EmployeeResponseDto> MapToResponseDtoAsync(Employee employee)
        {
            string? departmentName = null;
            string? departmentNameMr = null;
            string? departmentNameHi = null;
            string? departmentNameEn = null;

            string? designationName = null;
            string? designationNameMr = null;
            string? designationNameHi = null;
            string? designationNameEn = null;

            string? managerName = null;

            // Department
            if (!string.IsNullOrEmpty(employee.DepartmentId) && Guid.TryParse(employee.DepartmentId, out var deptGuid))
            {
                var dept = await _departmentRepository.GetByDepartmentIdAsync(deptGuid);

                if (dept != null)
                {
                    departmentName = dept.DepartmentName;
                    departmentNameMr = dept.DepartmentName;
                    departmentNameHi = dept.DepartmentName;
                    departmentNameEn = dept.DepartmentName;
                }
            }

            // Designation
            if (!string.IsNullOrEmpty(employee.DesignationId))
            {
                var desig = await _designationRepository.GetByIdAsync(employee.DesignationId);

                if (desig != null)
                {
                    designationName = desig.DesignationName;
                    designationNameMr = desig.DesignationNameMr;
                    designationNameHi = desig.DesignationNameHi;
                    designationNameEn = desig.DesignationNameEn;
                }
            }

            // Manager
            if (!string.IsNullOrEmpty(employee.ManagerId))
            {
                var manager = await _employeeRepository.GetByIdAsync(employee.ManagerId);
                managerName = manager?.GetFullName("mr");
            }

            return new EmployeeResponseDto
            {
                Id = employee.Id,
                EmployeeCode = employee.EmployeeCode,

                FirstNameMr = employee.FirstNameMr,
                MiddleNameMr = employee.MiddleNameMr,
                LastNameMr = employee.LastNameMr,

                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                LastName = employee.LastName,

                FirstNameHi = employee.FirstNameHi,
                MiddleNameHi = employee.MiddleNameHi,
                LastNameHi = employee.LastNameHi,

                FullName = employee.GetFullName("mr"),

                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,

                DepartmentId = employee.DepartmentId,
                DepartmentName = departmentName,
                DepartmentNameMr = departmentNameMr,
                DepartmentNameHi = departmentNameHi,
                DepartmentNameEn = departmentNameEn,

                DesignationId = employee.DesignationId,
                DesignationName = designationName,
                DesignationNameMr = designationNameMr,
                DesignationNameHi = designationNameHi,
                DesignationNameEn = designationNameEn,

                ManagerId = employee.ManagerId,
                ManagerName = managerName,

                DateOfJoining = employee.DateOfJoining,
                DateOfLeaving = employee.DateOfLeaving,

                EmploymentType = employee.EmploymentType,
                EmploymentTypeName = employee.EmploymentType.ToString(),

                EmployeeStatus = employee.EmployeeStatus,
                EmployeeStatusName = employee.EmployeeStatus.ToString(),

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
                    if (!Guid.TryParse(id, out var guid)) continue;
                    var dept = await _departmentRepository.GetByDepartmentIdAsync(guid);
                    if (dept != null) map[id] = dept.DepartmentName;
                }
                catch { }
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
                    if (desig != null) map[id] = desig.DesignationName;
                }
                catch { }
            }
            return map;
        }
    }
}