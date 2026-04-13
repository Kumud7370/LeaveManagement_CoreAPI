using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Designation;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class DesignationService : IDesignationService
    {
        private readonly IDesignationRepository _designationRepository;
        private readonly IDepartmentRepository _departmentRepository;

        public DesignationService(
            IDesignationRepository designationRepository,
            IDepartmentRepository departmentRepository)
        {
            _designationRepository = designationRepository;
            _departmentRepository = departmentRepository;
        }

        public async Task<DesignationResponseDto?> CreateDesignationAsync(CreateDesignationDto dto, string createdBy)
        {
            if (await _designationRepository.IsDesignationCodeExistsAsync(dto.DesignationCode))
                return null;

            var designation = new Designation
            {
                DesignationCode = dto.DesignationCode,
                DesignationName = dto.DesignationName,
                DesignationNameMr = dto.DesignationNameMr,
                DesignationNameEn = dto.DesignationNameEn,
                DesignationNameHi = dto.DesignationNameHi,
                Description = dto.Description,
                Level = dto.Level,
                IsActive = dto.IsActive,
                DepartmentId = dto.DepartmentId,
                CreatedBy = createdBy
            };

            var createdDesignation = await _designationRepository.CreateAsync(designation);
            return await MapToResponseDtoAsync(createdDesignation);
        }

        public async Task<DesignationResponseDto?> GetDesignationByIdAsync(string id)
        {
            var designation = await _designationRepository.GetByIdAsync(id);
            return designation != null ? await MapToResponseDtoAsync(designation) : null;
        }

        public async Task<DesignationResponseDto?> GetDesignationByCodeAsync(string designationCode)
        {
            var designation = await _designationRepository.GetByDesignationCodeAsync(designationCode);
            return designation != null ? await MapToResponseDtoAsync(designation) : null;
        }

        public async Task<PagedResultDto<DesignationResponseDto>> GetFilteredDesignationsAsync(DesignationFilterDto filter)
        {
            var (items, totalCount) = await _designationRepository.GetFilteredDesignationsAsync(filter);

            var designationDtos = new List<DesignationResponseDto>();
            foreach (var item in items)
                designationDtos.Add(await MapToResponseDtoAsync(item));

            return new PagedResultDto<DesignationResponseDto>(
                designationDtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        public async Task<List<DesignationResponseDto>> GetActiveDesignationsAsync()
        {
            var designations = await _designationRepository.GetActiveDesignationsAsync();
            var result = new List<DesignationResponseDto>();
            foreach (var d in designations)
                result.Add(await MapToResponseDtoAsync(d));
            return result;
        }

        public async Task<List<DesignationResponseDto>> GetDesignationsByLevelAsync(int level)
        {
            var designations = await _designationRepository.GetByLevelAsync(level);
            var result = new List<DesignationResponseDto>();
            foreach (var d in designations)
                result.Add(await MapToResponseDtoAsync(d));
            return result;
        }

        public async Task<List<DesignationResponseDto>> GetDesignationsByDepartmentAsync(string departmentId)
        {
            var designations = await _designationRepository.GetByDepartmentIdAsync(departmentId);
            var result = new List<DesignationResponseDto>();
            foreach (var d in designations)
                result.Add(await MapToResponseDtoAsync(d));
            return result;
        }

        public async Task<DesignationResponseDto?> UpdateDesignationAsync(string id, UpdateDesignationDto dto, string updatedBy)
        {
            var designation = await _designationRepository.GetByIdAsync(id);
            if (designation == null) return null;

            if (!string.IsNullOrEmpty(dto.DesignationCode) && dto.DesignationCode != designation.DesignationCode)
                if (await _designationRepository.IsDesignationCodeExistsAsync(dto.DesignationCode, id))
                    return null;

            if (!string.IsNullOrEmpty(dto.DesignationCode)) designation.DesignationCode = dto.DesignationCode;
            if (!string.IsNullOrEmpty(dto.DesignationName)) designation.DesignationName = dto.DesignationName;
            if (dto.DesignationNameMr != null) designation.DesignationNameMr = dto.DesignationNameMr;
            if (dto.DesignationNameEn != null) designation.DesignationNameEn = dto.DesignationNameEn;
            if (dto.DesignationNameHi != null) designation.DesignationNameHi = dto.DesignationNameHi;
            if (dto.Description != null) designation.Description = dto.Description;
            if (dto.Level.HasValue) designation.Level = dto.Level.Value;
            if (dto.IsActive.HasValue) designation.IsActive = dto.IsActive.Value;
            if (dto.DepartmentId != null) designation.DepartmentId = dto.DepartmentId;

            designation.UpdatedBy = updatedBy;

            var updated = await _designationRepository.UpdateAsync(id, designation);
            return updated ? await MapToResponseDtoAsync(designation) : null;
        }

        public async Task<bool> DeleteDesignationAsync(string id, string deletedBy)
        {
            var designation = await _designationRepository.GetByIdAsync(id);
            if (designation == null) return false;

            var employeeCount = await _designationRepository.GetEmployeeCountByDesignationAsync(id);
            if (employeeCount > 0) return false;

            designation.UpdatedBy = deletedBy;
            designation.DeletedAt = DateTime.UtcNow;

            return await _designationRepository.DeleteAsync(id);
        }

        public async Task<bool> ToggleDesignationStatusAsync(string id, string updatedBy)
        {
            var designation = await _designationRepository.GetByIdAsync(id);
            if (designation == null) return false;

            designation.IsActive = !designation.IsActive;
            designation.UpdatedBy = updatedBy;

            return await _designationRepository.UpdateAsync(id, designation);
        }

        public async Task<Dictionary<int, int>> GetDesignationStatisticsByLevelAsync()
        {
            var all = await _designationRepository.GetAllAsync();
            return all.GroupBy(d => d.Level).ToDictionary(g => g.Key, g => g.Count());
        }

        // ── Private mapping ───────────────────────────────────────────────────
        private async Task<DesignationResponseDto> MapToResponseDtoAsync(Designation designation)
        {
            var employeeCount = await _designationRepository.GetEmployeeCountByDesignationAsync(designation.Id);


            string? departmentName = null;
            string? departmentNameMr = null;
            string? departmentNameEn = null;
            string? departmentNameHi = null;
            if (!string.IsNullOrEmpty(designation.DepartmentId) &&
                Guid.TryParse(designation.DepartmentId, out var deptGuid))
            {
                var department = await _departmentRepository.GetByDepartmentIdAsync(deptGuid);
                if (department != null)
                {
                    departmentName = department.DepartmentName;
                    departmentNameMr = department.DepartmentNameMr;
                    departmentNameEn = department.DepartmentName;   
                    departmentNameHi = department.DepartmentNameHi;
                }
            }

            return new DesignationResponseDto
            {
                Id = designation.Id,
                DesignationCode = designation.DesignationCode,
                DesignationName = designation.DesignationName,
                DesignationNameMr = designation.DesignationNameMr,
                DesignationNameEn = designation.DesignationNameEn,
                DesignationNameHi = designation.DesignationNameHi,
                Description = designation.Description,
                Level = designation.Level,
                IsActive = designation.IsActive,
                EmployeeCount = employeeCount,
                DepartmentId = designation.DepartmentId,
                DepartmentName = departmentName,
                DepartmentNameMr = departmentNameMr,
                DepartmentNameEn = departmentNameEn,
                DepartmentNameHi = departmentNameHi,
                CreatedAt = designation.CreatedAt,
                UpdatedAt = designation.UpdatedAt
            };
        }
    }
}