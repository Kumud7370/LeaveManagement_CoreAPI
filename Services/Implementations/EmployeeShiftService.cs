using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.EmployeeShift;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class EmployeeShiftService : IEmployeeShiftService
    {
        private readonly IEmployeeShiftRepository _employeeShiftRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeShiftService(
            IEmployeeShiftRepository employeeShiftRepository,
            IShiftRepository shiftRepository,
            IEmployeeRepository employeeRepository)
        {
            _employeeShiftRepository = employeeShiftRepository;
            _shiftRepository = shiftRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<EmployeeShiftResponseDto?> CreateEmployeeShiftAsync(CreateEmployeeShiftDto dto, string createdBy)
        {
            // Validate employee exists
            var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
                return null;

            // Validate shift exists and is active
            var shift = await _shiftRepository.GetByIdAsync(dto.ShiftId);
            if (shift == null || !shift.IsActive)
                return null;

            // Check for overlapping shift assignments
            if (await _employeeShiftRepository.HasOverlappingShiftAssignmentAsync(dto.EmployeeId, dto.EffectiveFrom, dto.EffectiveTo))
                return null;

            // Check pending shift changes limit (max 3)
            var pendingCount = await _employeeShiftRepository.GetPendingShiftChangesCountForEmployeeAsync(dto.EmployeeId);
            if (pendingCount >= 3)
                return null;

            var employeeShift = new EmployeeShift
            {
                EmployeeId = dto.EmployeeId,
                ShiftId = dto.ShiftId,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                ChangeReason = dto.ChangeReason,
                RequestedDate = DateTime.UtcNow,
                Status = ShiftChangeStatus.Pending,
                IsActive = true,
                CreatedBy = createdBy
            };

            var createdEmployeeShift = await _employeeShiftRepository.CreateAsync(employeeShift);
            return await MapToResponseDtoAsync(createdEmployeeShift);
        }

        public async Task<EmployeeShiftResponseDto?> GetEmployeeShiftByIdAsync(string id)
        {
            var employeeShift = await _employeeShiftRepository.GetByIdAsync(id);
            return employeeShift != null ? await MapToResponseDtoAsync(employeeShift) : null;
        }

        public async Task<PagedResultDto<EmployeeShiftResponseDto>> GetFilteredEmployeeShiftsAsync(EmployeeShiftFilterDto filter)
        {
            var (items, totalCount) = await _employeeShiftRepository.GetFilteredEmployeeShiftsAsync(filter);

            var employeeShiftDtos = new List<EmployeeShiftResponseDto>();
            foreach (var employeeShift in items)
            {
                employeeShiftDtos.Add(await MapToResponseDtoAsync(employeeShift));
            }

            return new PagedResultDto<EmployeeShiftResponseDto>(
                employeeShiftDtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        public async Task<List<EmployeeShiftResponseDto>> GetEmployeeShiftsByEmployeeIdAsync(string employeeId)
        {
            var employeeShifts = await _employeeShiftRepository.GetByEmployeeIdAsync(employeeId);
            var employeeShiftDtos = new List<EmployeeShiftResponseDto>();

            foreach (var employeeShift in employeeShifts)
            {
                employeeShiftDtos.Add(await MapToResponseDtoAsync(employeeShift));
            }

            return employeeShiftDtos;
        }

        public async Task<EmployeeShiftResponseDto?> GetCurrentShiftForEmployeeAsync(string employeeId)
        {
            var employeeShift = await _employeeShiftRepository.GetCurrentShiftForEmployeeAsync(employeeId);
            return employeeShift != null ? await MapToResponseDtoAsync(employeeShift) : null;
        }

        public async Task<List<EmployeeShiftResponseDto>> GetPendingShiftChangesAsync()
        {
            var employeeShifts = await _employeeShiftRepository.GetPendingShiftChangesAsync();
            var employeeShiftDtos = new List<EmployeeShiftResponseDto>();

            foreach (var employeeShift in employeeShifts)
            {
                employeeShiftDtos.Add(await MapToResponseDtoAsync(employeeShift));
            }

            return employeeShiftDtos;
        }

        public async Task<List<EmployeeShiftResponseDto>> GetUpcomingShiftChangesAsync(int days = 7)
        {
            var employeeShifts = await _employeeShiftRepository.GetUpcomingShiftChangesAsync(days);
            var employeeShiftDtos = new List<EmployeeShiftResponseDto>();

            foreach (var employeeShift in employeeShifts)
            {
                employeeShiftDtos.Add(await MapToResponseDtoAsync(employeeShift));
            }

            return employeeShiftDtos;
        }

        public async Task<EmployeeShiftResponseDto?> UpdateEmployeeShiftAsync(string id, UpdateEmployeeShiftDto dto, string updatedBy)
        {
            var employeeShift = await _employeeShiftRepository.GetByIdAsync(id);
            if (employeeShift == null || !employeeShift.CanBeModified())
                return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.ShiftId))
            {
                var shift = await _shiftRepository.GetByIdAsync(dto.ShiftId);
                if (shift == null || !shift.IsActive)
                    return null;
                employeeShift.ShiftId = dto.ShiftId;
            }

            if (dto.EffectiveFrom.HasValue)
                employeeShift.EffectiveFrom = dto.EffectiveFrom.Value;

            if (dto.EffectiveTo.HasValue)
                employeeShift.EffectiveTo = dto.EffectiveTo.Value;

            if (!string.IsNullOrEmpty(dto.ChangeReason))
                employeeShift.ChangeReason = dto.ChangeReason;

            // Check for overlapping assignments if dates changed
            if (dto.EffectiveFrom.HasValue || dto.EffectiveTo.HasValue)
            {
                if (await _employeeShiftRepository.HasOverlappingShiftAssignmentAsync(
                    employeeShift.EmployeeId,
                    employeeShift.EffectiveFrom,
                    employeeShift.EffectiveTo,
                    id))
                    return null;
            }

            employeeShift.UpdatedBy = updatedBy;

            var updated = await _employeeShiftRepository.UpdateAsync(id, employeeShift);
            return updated ? await MapToResponseDtoAsync(employeeShift) : null;
        }

        public async Task<bool> DeleteEmployeeShiftAsync(string id, string deletedBy)
        {
            var employeeShift = await _employeeShiftRepository.GetByIdAsync(id);
            if (employeeShift == null || !employeeShift.CanBeModified())
                return false;

            employeeShift.UpdatedBy = deletedBy;
            employeeShift.DeletedAt = DateTime.UtcNow;

            return await _employeeShiftRepository.DeleteAsync(id);
        }

        public async Task<bool> ApproveShiftChangeAsync(string id, string approvedBy)
        {
            var employeeShift = await _employeeShiftRepository.GetByIdAsync(id);
            if (employeeShift == null || employeeShift.Status != ShiftChangeStatus.Pending)
                return false;

            // Deactivate any overlapping shift assignments
            var overlappingShifts = await _employeeShiftRepository.GetByEmployeeIdAsync(employeeShift.EmployeeId);
            foreach (var overlap in overlappingShifts)
            {
                if (overlap.Id != id &&
                    overlap.Status == ShiftChangeStatus.Approved &&
                    overlap.IsActive &&
                    !overlap.EffectiveTo.HasValue)
                {
                    // Set end date to one day before new shift starts
                    overlap.EffectiveTo = employeeShift.EffectiveFrom.AddDays(-1);
                    await _employeeShiftRepository.UpdateAsync(overlap.Id, overlap);
                }
            }

            employeeShift.Status = ShiftChangeStatus.Approved;
            employeeShift.ApprovedBy = approvedBy;
            employeeShift.ApprovedDate = DateTime.UtcNow;
            employeeShift.UpdatedBy = approvedBy;

            return await _employeeShiftRepository.UpdateAsync(id, employeeShift);
        }

        public async Task<bool> RejectShiftChangeAsync(string id, string rejectedBy, string rejectionReason)
        {
            var employeeShift = await _employeeShiftRepository.GetByIdAsync(id);
            if (employeeShift == null || employeeShift.Status != ShiftChangeStatus.Pending)
                return false;

            employeeShift.Status = ShiftChangeStatus.Rejected;
            employeeShift.RejectedBy = rejectedBy;
            employeeShift.RejectedDate = DateTime.UtcNow;
            employeeShift.RejectionReason = rejectionReason;
            employeeShift.UpdatedBy = rejectedBy;

            return await _employeeShiftRepository.UpdateAsync(id, employeeShift);
        }

        public async Task<bool> CancelShiftChangeAsync(string id, string updatedBy)
        {
            var employeeShift = await _employeeShiftRepository.GetByIdAsync(id);
            if (employeeShift == null || !employeeShift.CanBeModified())
                return false;

            employeeShift.Status = ShiftChangeStatus.Cancelled;
            employeeShift.UpdatedBy = updatedBy;

            return await _employeeShiftRepository.UpdateAsync(id, employeeShift);
        }

        public async Task<Dictionary<string, int>> GetShiftChangeStatisticsByStatusAsync()
        {
            var statistics = new Dictionary<string, int>();

            foreach (ShiftChangeStatus status in Enum.GetValues(typeof(ShiftChangeStatus)))
            {
                var shifts = await _employeeShiftRepository.GetByStatusAsync(status);
                statistics[status.ToString()] = shifts.Count;
            }

            return statistics;
        }

        public async Task<bool> ValidateShiftAssignmentAsync(string employeeId, DateTime effectiveFrom, DateTime? effectiveTo, string? excludeId = null)
        {
            // Check employee exists
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                return false;

            // Check for overlapping assignments
            return !await _employeeShiftRepository.HasOverlappingShiftAssignmentAsync(employeeId, effectiveFrom, effectiveTo, excludeId);
        }

        private async Task<EmployeeShiftResponseDto> MapToResponseDtoAsync(EmployeeShift employeeShift)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeShift.EmployeeId);
            var shift = await _shiftRepository.GetByIdAsync(employeeShift.ShiftId);

            Employee? approver = null;
            if (!string.IsNullOrEmpty(employeeShift.ApprovedBy))
                approver = await _employeeRepository.GetByIdAsync(employeeShift.ApprovedBy);

            Employee? rejector = null;
            if (!string.IsNullOrEmpty(employeeShift.RejectedBy))
                rejector = await _employeeRepository.GetByIdAsync(employeeShift.RejectedBy);

            return new EmployeeShiftResponseDto
            {
                Id = employeeShift.Id,
                EmployeeId = employeeShift.EmployeeId,
                EmployeeCode = employee?.EmployeeCode,
                EmployeeName = employee?.GetFullName(),
                ShiftId = employeeShift.ShiftId,
                ShiftName = shift?.ShiftName,
                ShiftCode = shift?.ShiftCode,
                ShiftColor = shift?.Color,
                ShiftStartTime = shift?.StartTime,
                ShiftEndTime = shift?.EndTime,
                ShiftTimingDisplay = shift?.GetShiftTimingDisplay(),
                EffectiveFrom = employeeShift.EffectiveFrom,
                EffectiveTo = employeeShift.EffectiveTo,
                IsActive = employeeShift.IsActive,
                ChangeReason = employeeShift.ChangeReason,
                Status = employeeShift.Status,
                StatusName = employeeShift.Status.ToString(),
                RequestedDate = employeeShift.RequestedDate,
                ApprovedBy = employeeShift.ApprovedBy,
                ApprovedByName = approver?.GetFullName(),
                ApprovedDate = employeeShift.ApprovedDate,
                RejectedBy = employeeShift.RejectedBy,
                RejectedByName = rejector?.GetFullName(),
                RejectedDate = employeeShift.RejectedDate,
                RejectionReason = employeeShift.RejectionReason,
                IsCurrentlyActive = employeeShift.IsCurrentlyActive(),
                CanBeModified = employeeShift.CanBeModified(),
                DurationInDays = employeeShift.GetDurationInDays(),
                CreatedAt = employeeShift.CreatedAt,
                UpdatedAt = employeeShift.UpdatedAt
            };
        }
    }
}