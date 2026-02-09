using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Leave;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public LeaveService(
            ILeaveRepository leaveRepository,
            ILeaveTypeRepository leaveTypeRepository,
            IEmployeeRepository employeeRepository)
        {
            _leaveRepository = leaveRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<LeaveResponseDto?> CreateLeaveAsync(CreateLeaveDto dto, string createdBy)
        {
            // Validate employee exists
            var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
                return null;

            // Validate leave type exists
            var leaveType = await _leaveTypeRepository.GetByIdAsync(dto.LeaveTypeId);
            if (leaveType == null || !leaveType.IsActive)
                return null;

            // Check for overlapping leaves
            if (await _leaveRepository.HasOverlappingLeaveAsync(dto.EmployeeId, dto.StartDate, dto.EndDate))
                return null;

            // Validate leave days against available balance
            var year = dto.StartDate.Year;
            var usedDays = await _leaveRepository.GetApprovedLeaveDaysForEmployeeInYearAsync(dto.EmployeeId, dto.LeaveTypeId, year);
            var remainingDays = leaveType.MaxDaysPerYear - usedDays;

            if (dto.TotalDays > remainingDays)
                return null;

            var leave = new Leave
            {
                EmployeeId = dto.EmployeeId,
                LeaveTypeId = dto.LeaveTypeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalDays = dto.TotalDays,
                Reason = dto.Reason,
                IsEmergencyLeave = dto.IsEmergencyLeave,
                AttachmentUrl = dto.AttachmentUrl,
                AppliedDate = DateTime.UtcNow,
                LeaveStatus = LeaveStatus.Pending,
                CreatedBy = createdBy
            };

            var createdLeave = await _leaveRepository.CreateAsync(leave);
            return await MapToResponseDtoAsync(createdLeave);
        }

        public async Task<LeaveResponseDto?> GetLeaveByIdAsync(string id)
        {
            var leave = await _leaveRepository.GetByIdAsync(id);
            return leave != null ? await MapToResponseDtoAsync(leave) : null;
        }

        public async Task<PagedResultDto<LeaveResponseDto>> GetFilteredLeavesAsync(LeaveFilterDto filter)
        {
            var (items, totalCount) = await _leaveRepository.GetFilteredLeavesAsync(filter);

            var leaveDtos = new List<LeaveResponseDto>();
            foreach (var leave in items)
            {
                leaveDtos.Add(await MapToResponseDtoAsync(leave));
            }

            return new PagedResultDto<LeaveResponseDto>(
                leaveDtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        public async Task<List<LeaveResponseDto>> GetLeavesByEmployeeIdAsync(string employeeId)
        {
            var leaves = await _leaveRepository.GetLeavesByEmployeeIdAsync(employeeId);
            var leaveDtos = new List<LeaveResponseDto>();

            foreach (var leave in leaves)
            {
                leaveDtos.Add(await MapToResponseDtoAsync(leave));
            }

            return leaveDtos;
        }

        public async Task<List<LeaveResponseDto>> GetPendingLeavesAsync()
        {
            var leaves = await _leaveRepository.GetPendingLeavesAsync();
            var leaveDtos = new List<LeaveResponseDto>();

            foreach (var leave in leaves)
            {
                leaveDtos.Add(await MapToResponseDtoAsync(leave));
            }

            return leaveDtos;
        }

        public async Task<List<LeaveResponseDto>> GetUpcomingLeavesAsync(int days = 7)
        {
            var leaves = await _leaveRepository.GetUpcomingLeavesAsync(days);
            var leaveDtos = new List<LeaveResponseDto>();

            foreach (var leave in leaves)
            {
                leaveDtos.Add(await MapToResponseDtoAsync(leave));
            }

            return leaveDtos;
        }

        public async Task<LeaveResponseDto?> UpdateLeaveAsync(string id, UpdateLeaveDto dto, string updatedBy)
        {
            var leave = await _leaveRepository.GetByIdAsync(id);
            if (leave == null || leave.LeaveStatus != LeaveStatus.Pending)
                return null;

            // Update only provided fields
            if (dto.StartDate.HasValue)
                leave.StartDate = dto.StartDate.Value;

            if (dto.EndDate.HasValue)
                leave.EndDate = dto.EndDate.Value;

            if (dto.TotalDays.HasValue)
                leave.TotalDays = dto.TotalDays.Value;

            if (!string.IsNullOrEmpty(dto.Reason))
                leave.Reason = dto.Reason;

            if (dto.IsEmergencyLeave.HasValue)
                leave.IsEmergencyLeave = dto.IsEmergencyLeave.Value;

            if (dto.AttachmentUrl != null)
                leave.AttachmentUrl = dto.AttachmentUrl;

            // Check for overlapping leaves if dates changed
            if (dto.StartDate.HasValue || dto.EndDate.HasValue)
            {
                if (await _leaveRepository.HasOverlappingLeaveAsync(leave.EmployeeId, leave.StartDate, leave.EndDate, id))
                    return null;
            }

            leave.UpdatedBy = updatedBy;

            var updated = await _leaveRepository.UpdateAsync(id, leave);
            return updated ? await MapToResponseDtoAsync(leave) : null;
        }

        public async Task<bool> DeleteLeaveAsync(string id, string deletedBy)
        {
            var leave = await _leaveRepository.GetByIdAsync(id);
            if (leave == null || leave.LeaveStatus != LeaveStatus.Pending)
                return false;

            leave.UpdatedBy = deletedBy;
            leave.DeletedAt = DateTime.UtcNow;

            return await _leaveRepository.DeleteAsync(id);
        }

        public async Task<bool> ApproveLeaveAsync(string id, string approvedBy)
        {
            var leave = await _leaveRepository.GetByIdAsync(id);
            if (leave == null || leave.LeaveStatus != LeaveStatus.Pending)
                return false;

            leave.LeaveStatus = LeaveStatus.Approved;
            leave.ApprovedBy = approvedBy;
            leave.ApprovedDate = DateTime.UtcNow;
            leave.UpdatedBy = approvedBy;

            return await _leaveRepository.UpdateAsync(id, leave);
        }

        public async Task<bool> RejectLeaveAsync(string id, string rejectedBy, string rejectionReason)
        {
            var leave = await _leaveRepository.GetByIdAsync(id);
            if (leave == null || leave.LeaveStatus != LeaveStatus.Pending)
                return false;

            leave.LeaveStatus = LeaveStatus.Rejected;
            leave.RejectedBy = rejectedBy;
            leave.RejectedDate = DateTime.UtcNow;
            leave.RejectionReason = rejectionReason;
            leave.UpdatedBy = rejectedBy;

            return await _leaveRepository.UpdateAsync(id, leave);
        }

        public async Task<bool> CancelLeaveAsync(string id, string cancelledBy, string cancellationReason)
        {
            var leave = await _leaveRepository.GetByIdAsync(id);
            if (leave == null || !leave.CanBeCancelled())
                return false;

            leave.LeaveStatus = LeaveStatus.Cancelled;
            leave.CancelledDate = DateTime.UtcNow;
            leave.CancellationReason = cancellationReason;
            leave.UpdatedBy = cancelledBy;

            return await _leaveRepository.UpdateAsync(id, leave);
        }

        public async Task<Dictionary<string, int>> GetLeaveStatisticsByStatusAsync()
        {
            var statistics = new Dictionary<string, int>();

            foreach (LeaveStatus status in Enum.GetValues(typeof(LeaveStatus)))
            {
                var leaves = await _leaveRepository.GetLeavesByStatusAsync(status);
                statistics[status.ToString()] = leaves.Count;
            }

            return statistics;
        }

        public async Task<int> GetRemainingLeaveDaysAsync(string employeeId, string leaveTypeId, int year)
        {
            var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveTypeId);
            if (leaveType == null)
                return 0;

            var usedDays = await _leaveRepository.GetApprovedLeaveDaysForEmployeeInYearAsync(employeeId, leaveTypeId, year);
            return Math.Max(0, leaveType.MaxDaysPerYear - usedDays);
        }

        public async Task<bool> ValidateLeaveRequestAsync(string employeeId, string leaveTypeId, DateTime startDate, DateTime endDate, string? excludeLeaveId = null)
        {
            // Check for overlapping leaves
            if (await _leaveRepository.HasOverlappingLeaveAsync(employeeId, startDate, endDate, excludeLeaveId))
                return false;

            // Validate against leave balance
            var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveTypeId);
            if (leaveType == null || !leaveType.IsActive)
                return false;

            var totalDays = (decimal)(endDate - startDate).TotalDays + 1;
            var year = startDate.Year;
            var usedDays = await _leaveRepository.GetApprovedLeaveDaysForEmployeeInYearAsync(employeeId, leaveTypeId, year);
            var remainingDays = leaveType.MaxDaysPerYear - usedDays;

            return totalDays <= remainingDays;
        }

        private async Task<LeaveResponseDto> MapToResponseDtoAsync(Leave leave)
        {
            var employee = await _employeeRepository.GetByIdAsync(leave.EmployeeId);
            var leaveType = await _leaveTypeRepository.GetByIdAsync(leave.LeaveTypeId);

            Employee? approver = null;
            if (!string.IsNullOrEmpty(leave.ApprovedBy))
                approver = await _employeeRepository.GetByIdAsync(leave.ApprovedBy);

            Employee? rejector = null;
            if (!string.IsNullOrEmpty(leave.RejectedBy))
                rejector = await _employeeRepository.GetByIdAsync(leave.RejectedBy);

            return new LeaveResponseDto
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                EmployeeCode = employee?.EmployeeCode,
                EmployeeName = employee?.GetFullName(),
                LeaveTypeId = leave.LeaveTypeId,
                LeaveTypeName = leaveType?.Name,
                LeaveTypeCode = leaveType?.Code,
                LeaveTypeColor = leaveType?.Color,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                TotalDays = leave.TotalDays,
                Reason = leave.Reason,
                LeaveStatus = leave.LeaveStatus,
                LeaveStatusName = leave.LeaveStatus.ToString(),
                AppliedDate = leave.AppliedDate,
                ApprovedBy = leave.ApprovedBy,
                ApprovedByName = approver?.GetFullName(),
                ApprovedDate = leave.ApprovedDate,
                RejectedBy = leave.RejectedBy,
                RejectedByName = rejector?.GetFullName(),
                RejectedDate = leave.RejectedDate,
                RejectionReason = leave.RejectionReason,
                CancelledDate = leave.CancelledDate,
                CancellationReason = leave.CancellationReason,
                IsEmergencyLeave = leave.IsEmergencyLeave,
                AttachmentUrl = leave.AttachmentUrl,
                IsActiveLeave = leave.IsActiveLeave(),
                CanBeCancelled = leave.CanBeCancelled(),
                CreatedAt = leave.CreatedAt,
                UpdatedAt = leave.UpdatedAt
            };
        }
    }
}