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

        public DesignationService(IDesignationRepository designationRepository)
        {
            _designationRepository = designationRepository;
        }

        public async Task<DesignationResponseDto?> CreateDesignationAsync(CreateDesignationDto dto, string createdBy)
        {
            // Check if designation code already exists
            if (await _designationRepository.IsDesignationCodeExistsAsync(dto.DesignationCode))
                return null;

            var designation = new Designation
            {
                DesignationCode = dto.DesignationCode,
                DesignationName = dto.DesignationName,
                Description = dto.Description,
                Level = dto.Level,
                IsActive = dto.IsActive,
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
            {
                designationDtos.Add(await MapToResponseDtoAsync(item));
            }

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
            var designationDtos = new List<DesignationResponseDto>();

            foreach (var designation in designations)
            {
                designationDtos.Add(await MapToResponseDtoAsync(designation));
            }

            return designationDtos;
        }

        public async Task<List<DesignationResponseDto>> GetDesignationsByLevelAsync(int level)
        {
            var designations = await _designationRepository.GetByLevelAsync(level);
            var designationDtos = new List<DesignationResponseDto>();

            foreach (var designation in designations)
            {
                designationDtos.Add(await MapToResponseDtoAsync(designation));
            }

            return designationDtos;
        }

        public async Task<DesignationResponseDto?> UpdateDesignationAsync(string id, UpdateDesignationDto dto, string updatedBy)
        {
            var designation = await _designationRepository.GetByIdAsync(id);
            if (designation == null)
                return null;

            // Check if designation code is being updated and if it already exists
            if (!string.IsNullOrEmpty(dto.DesignationCode) && dto.DesignationCode != designation.DesignationCode)
            {
                if (await _designationRepository.IsDesignationCodeExistsAsync(dto.DesignationCode, id))
                    return null;
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.DesignationCode))
                designation.DesignationCode = dto.DesignationCode;

            if (!string.IsNullOrEmpty(dto.DesignationName))
                designation.DesignationName = dto.DesignationName;

            if (dto.Description != null)
                designation.Description = dto.Description;

            if (dto.Level.HasValue)
                designation.Level = dto.Level.Value;

            if (dto.IsActive.HasValue)
                designation.IsActive = dto.IsActive.Value;

            designation.UpdatedBy = updatedBy;

            var updated = await _designationRepository.UpdateAsync(id, designation);
            return updated ? await MapToResponseDtoAsync(designation) : null;
        }

        public async Task<bool> DeleteDesignationAsync(string id, string deletedBy)
        {
            var designation = await _designationRepository.GetByIdAsync(id);
            if (designation == null)
                return false;

            // Check if designation is assigned to any employees
            var employeeCount = await _designationRepository.GetEmployeeCountByDesignationAsync(id);
            if (employeeCount > 0)
                return false; // Cannot delete designation that is in use

            designation.UpdatedBy = deletedBy;
            designation.DeletedAt = DateTime.UtcNow;

            return await _designationRepository.DeleteAsync(id);
        }

        public async Task<bool> ToggleDesignationStatusAsync(string id, string updatedBy)
        {
            var designation = await _designationRepository.GetByIdAsync(id);
            if (designation == null)
                return false;

            designation.IsActive = !designation.IsActive;
            designation.UpdatedBy = updatedBy;

            return await _designationRepository.UpdateAsync(id, designation);
        }

        public async Task<Dictionary<int, int>> GetDesignationStatisticsByLevelAsync()
        {
            var allDesignations = await _designationRepository.GetAllAsync();

            return allDesignations
                .GroupBy(d => d.Level)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private async Task<DesignationResponseDto> MapToResponseDtoAsync(Designation designation)
        {
            var employeeCount = await _designationRepository.GetEmployeeCountByDesignationAsync(designation.Id);

            return new DesignationResponseDto
            {
                Id = designation.Id,
                DesignationCode = designation.DesignationCode,
                DesignationName = designation.DesignationName,
                Description = designation.Description,
                Level = designation.Level,
                IsActive = designation.IsActive,
                EmployeeCount = employeeCount,
                CreatedAt = designation.CreatedAt,
                UpdatedAt = designation.UpdatedAt
            };
        }
    }
}