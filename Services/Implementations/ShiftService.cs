using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Shift;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class ShiftService : IShiftService
    {
        private readonly IShiftRepository _shiftRepository;

        public ShiftService(IShiftRepository shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        public async Task<ShiftResponseDto?> CreateShiftAsync(CreateShiftDto dto, string createdBy)
        {
            // Check if code already exists
            if (await _shiftRepository.IsCodeExistsAsync(dto.ShiftCode))
                return null;

            // Get next display order if not provided
            if (dto.DisplayOrder == 0)
            {
                var maxOrder = await _shiftRepository.GetMaxDisplayOrderAsync();
                dto.DisplayOrder = maxOrder + 1;
            }

            var shift = new Shift
            {
                ShiftName = dto.ShiftName,
                ShiftCode = dto.ShiftCode,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                GracePeriodMinutes = dto.GracePeriodMinutes,
                MinimumWorkingMinutes = dto.MinimumWorkingMinutes,
                BreakDurationMinutes = dto.BreakDurationMinutes,
                IsNightShift = dto.IsNightShift,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                Description = dto.Description,
                Color = dto.Color,
                NightShiftAllowancePercentage = dto.NightShiftAllowancePercentage,
                CreatedBy = createdBy
            };

            var createdShift = await _shiftRepository.CreateAsync(shift);
            return MapToResponseDto(createdShift);
        }

        public async Task<ShiftResponseDto?> GetShiftByIdAsync(string id)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            return shift != null ? MapToResponseDto(shift) : null;
        }

        public async Task<ShiftResponseDto?> GetShiftByCodeAsync(string code)
        {
            var shift = await _shiftRepository.GetByCodeAsync(code);
            return shift != null ? MapToResponseDto(shift) : null;
        }

        public async Task<PagedResultDto<ShiftResponseDto>> GetFilteredShiftsAsync(ShiftFilterDto filter)
        {
            var (items, totalCount) = await _shiftRepository.GetFilteredShiftsAsync(filter);

            var shiftDtos = items.Select(MapToResponseDto).ToList();

            return new PagedResultDto<ShiftResponseDto>(
                shiftDtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        public async Task<List<ShiftResponseDto>> GetActiveShiftsAsync()
        {
            var shifts = await _shiftRepository.GetActiveShiftsAsync();
            return shifts.Select(MapToResponseDto).ToList();
        }

        public async Task<List<ShiftResponseDto>> GetNightShiftsAsync()
        {
            var shifts = await _shiftRepository.GetNightShiftsAsync();
            return shifts.Select(MapToResponseDto).ToList();
        }

        public async Task<ShiftResponseDto?> UpdateShiftAsync(string id, UpdateShiftDto dto, string updatedBy)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
                return null;

            // Check if code is being updated and if it already exists
            if (!string.IsNullOrEmpty(dto.ShiftCode) && dto.ShiftCode != shift.ShiftCode)
            {
                if (await _shiftRepository.IsCodeExistsAsync(dto.ShiftCode, id))
                    return null;
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.ShiftName))
                shift.ShiftName = dto.ShiftName;

            if (!string.IsNullOrEmpty(dto.ShiftCode))
                shift.ShiftCode = dto.ShiftCode;

            if (dto.StartTime.HasValue)
                shift.StartTime = dto.StartTime.Value;

            if (dto.EndTime.HasValue)
                shift.EndTime = dto.EndTime.Value;

            if (dto.GracePeriodMinutes.HasValue)
                shift.GracePeriodMinutes = dto.GracePeriodMinutes.Value;

            if (dto.MinimumWorkingMinutes.HasValue)
                shift.MinimumWorkingMinutes = dto.MinimumWorkingMinutes.Value;

            if (dto.BreakDurationMinutes.HasValue)
                shift.BreakDurationMinutes = dto.BreakDurationMinutes.Value;

            if (dto.IsNightShift.HasValue)
                shift.IsNightShift = dto.IsNightShift.Value;

            if (dto.IsActive.HasValue)
                shift.IsActive = dto.IsActive.Value;

            if (dto.DisplayOrder.HasValue)
                shift.DisplayOrder = dto.DisplayOrder.Value;

            if (!string.IsNullOrEmpty(dto.Description))
                shift.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.Color))
                shift.Color = dto.Color;

            if (dto.NightShiftAllowancePercentage.HasValue)
                shift.NightShiftAllowancePercentage = dto.NightShiftAllowancePercentage.Value;

            shift.UpdatedBy = updatedBy;

            var updated = await _shiftRepository.UpdateAsync(id, shift);
            return updated ? MapToResponseDto(shift) : null;
        }

        public async Task<bool> DeleteShiftAsync(string id, string deletedBy)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
                return false;

            shift.UpdatedBy = deletedBy;
            shift.DeletedAt = DateTime.UtcNow;

            return await _shiftRepository.DeleteAsync(id);
        }

        public async Task<bool> ToggleShiftStatusAsync(string id, bool isActive, string updatedBy)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
                return false;

            shift.IsActive = isActive;
            shift.UpdatedBy = updatedBy;

            return await _shiftRepository.UpdateAsync(id, shift);
        }

        private ShiftResponseDto MapToResponseDto(Shift shift)
        {
            return new ShiftResponseDto
            {
                Id = shift.Id,
                ShiftName = shift.ShiftName,
                ShiftCode = shift.ShiftCode,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                StartTimeFormatted = shift.StartTime.ToString("HH:mm"),
                EndTimeFormatted = shift.EndTime.ToString("HH:mm"),
                GracePeriodMinutes = shift.GracePeriodMinutes,
                MinimumWorkingMinutes = shift.MinimumWorkingMinutes,
                MinimumWorkingHoursFormatted = $"{shift.MinimumWorkingMinutes / 60}h {shift.MinimumWorkingMinutes % 60}m",
                BreakDurationMinutes = shift.BreakDurationMinutes,
                IsNightShift = shift.IsNightShift,
                IsActive = shift.IsActive,
                DisplayOrder = shift.DisplayOrder,
                Description = shift.Description,
                Color = shift.Color,
                NightShiftAllowancePercentage = shift.NightShiftAllowancePercentage,
                TotalShiftDuration = FormatTimeSpan(shift.GetTotalShiftDuration()),
                NetWorkingHours = FormatTimeSpan(shift.GetNetWorkingHours()),
                ShiftTimingDisplay = shift.GetShiftTimingDisplay(),
                CreatedAt = shift.CreatedAt,
                UpdatedAt = shift.UpdatedAt
            };
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        }
    }
}