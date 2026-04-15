using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.LeaveType;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class LeaveTypeService : ILeaveTypeService
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;

        public LeaveTypeService(ILeaveTypeRepository leaveTypeRepository)
        {
            _leaveTypeRepository = leaveTypeRepository;
        }

        public async Task<LeaveTypeResponseDto?> CreateLeaveTypeAsync(CreateLeaveTypeDto dto, string createdBy)
        {
            if (await _leaveTypeRepository.IsCodeExistsAsync(dto.Code))
                return null;

            if (dto.DisplayOrder == 0)
            {
                var maxOrder = await _leaveTypeRepository.GetMaxDisplayOrderAsync();
                dto.DisplayOrder = maxOrder + 1;
            }

            var leaveType = new LeaveType
            {
                Name = dto.Name,
                Code = dto.Code,
                NameMr = dto.NameMr,
                NameHi = dto.NameHi,
                Description = dto.Description,
                IsPaidLeave = dto.IsPaidLeave,
                MaxDaysPerYear = dto.MaxDaysPerYear,
                IsCarryForward = dto.IsCarryForward,
                MaxCarryForwardDays = dto.MaxCarryForwardDays,
                RequiresApproval = dto.RequiresApproval,
                RequiresDocument = dto.RequiresDocument,
                MinimumNoticeDays = dto.MinimumNoticeDays,
                Color = dto.Color,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                CreatedBy = createdBy
            };

            var createdLeaveType = await _leaveTypeRepository.CreateAsync(leaveType);
            return MapToResponseDto(createdLeaveType);
        }

        public async Task<LeaveTypeResponseDto?> GetLeaveTypeByIdAsync(string id)
        {
            var leaveType = await _leaveTypeRepository.GetByIdAsync(id);
            return leaveType != null ? MapToResponseDto(leaveType) : null;
        }

        public async Task<LeaveTypeResponseDto?> GetLeaveTypeByCodeAsync(string code)
        {
            var leaveType = await _leaveTypeRepository.GetByCodeAsync(code);
            return leaveType != null ? MapToResponseDto(leaveType) : null;
        }

        public async Task<PagedResultDto<LeaveTypeResponseDto>> GetFilteredLeaveTypesAsync(LeaveTypeFilterDto filter)
        {
            var (items, totalCount) = await _leaveTypeRepository.GetFilteredLeaveTypesAsync(filter);

            var leaveTypeDtos = items.Select(MapToResponseDto).ToList();

            return new PagedResultDto<LeaveTypeResponseDto>(
                leaveTypeDtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        public async Task<List<LeaveTypeResponseDto>> GetActiveLeaveTypesAsync()
        {
            var leaveTypes = await _leaveTypeRepository.GetActiveLeaveTypesAsync();
            return leaveTypes.Select(MapToResponseDto).ToList();
        }

        public async Task<LeaveTypeResponseDto?> UpdateLeaveTypeAsync(string id, UpdateLeaveTypeDto dto, string updatedBy)
        {
            var leaveType = await _leaveTypeRepository.GetByIdAsync(id);
            if (leaveType == null)
                return null;

            if (!string.IsNullOrEmpty(dto.Code) && dto.Code != leaveType.Code)
            {
                if (await _leaveTypeRepository.IsCodeExistsAsync(dto.Code, id))
                    return null;
            }

            if (!string.IsNullOrEmpty(dto.Name))
                leaveType.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Code))
                leaveType.Code = dto.Code;

            if (dto.NameMr != null)
                leaveType.NameMr = dto.NameMr;

            if (dto.NameHi != null)
                leaveType.NameHi = dto.NameHi;

            if (!string.IsNullOrEmpty(dto.Description))
                leaveType.Description = dto.Description;

            if (dto.IsPaidLeave.HasValue)
                leaveType.IsPaidLeave = dto.IsPaidLeave.Value;

            if (dto.MaxDaysPerYear.HasValue)
                leaveType.MaxDaysPerYear = dto.MaxDaysPerYear.Value;

            if (dto.IsCarryForward.HasValue)
                leaveType.IsCarryForward = dto.IsCarryForward.Value;

            if (dto.MaxCarryForwardDays.HasValue)
                leaveType.MaxCarryForwardDays = dto.MaxCarryForwardDays.Value;

            if (dto.RequiresApproval.HasValue)
                leaveType.RequiresApproval = dto.RequiresApproval.Value;

            if (dto.RequiresDocument.HasValue)
                leaveType.RequiresDocument = dto.RequiresDocument.Value;

            if (dto.MinimumNoticeDays.HasValue)
                leaveType.MinimumNoticeDays = dto.MinimumNoticeDays.Value;

            if (!string.IsNullOrEmpty(dto.Color))
                leaveType.Color = dto.Color;

            if (dto.IsActive.HasValue)
                leaveType.IsActive = dto.IsActive.Value;

            if (dto.DisplayOrder.HasValue)
                leaveType.DisplayOrder = dto.DisplayOrder.Value;

            leaveType.UpdatedBy = updatedBy;

            var updated = await _leaveTypeRepository.UpdateAsync(id, leaveType);
            return updated ? MapToResponseDto(leaveType) : null;
        }

        public async Task<bool> DeleteLeaveTypeAsync(string id, string deletedBy)
        {
            var leaveType = await _leaveTypeRepository.GetByIdAsync(id);
            if (leaveType == null)
                return false;

            leaveType.UpdatedBy = deletedBy;
            leaveType.DeletedAt = DateTime.UtcNow;

            return await _leaveTypeRepository.DeleteAsync(id);
        }

        public async Task<bool> ToggleLeaveTypeStatusAsync(string id, bool isActive, string updatedBy)
        {
            var leaveType = await _leaveTypeRepository.GetByIdAsync(id);
            if (leaveType == null)
                return false;

            leaveType.IsActive = isActive;
            leaveType.UpdatedBy = updatedBy;

            return await _leaveTypeRepository.UpdateAsync(id, leaveType);
        }

        private LeaveTypeResponseDto MapToResponseDto(LeaveType leaveType)
        {
            return new LeaveTypeResponseDto
            {
                Id = leaveType.Id,
                Name = leaveType.Name,
                NameMr = leaveType.NameMr,  
                NameHi = leaveType.NameHi,
                Code = leaveType.Code,
                Description = leaveType.Description,
                IsPaidLeave = leaveType.IsPaidLeave,
                MaxDaysPerYear = leaveType.MaxDaysPerYear,
                IsCarryForward = leaveType.IsCarryForward,
                MaxCarryForwardDays = leaveType.MaxCarryForwardDays,
                AvailableCarryForward = leaveType.GetAvailableCarryForward(),
                RequiresApproval = leaveType.RequiresApproval,
                RequiresDocument = leaveType.RequiresDocument,
                MinimumNoticeDays = leaveType.MinimumNoticeDays,
                Color = leaveType.Color,
                IsActive = leaveType.IsActive,
                DisplayOrder = leaveType.DisplayOrder,
                CreatedAt = leaveType.CreatedAt,
                UpdatedAt = leaveType.UpdatedAt
            };
        }
    }
}