using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Leave;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Implementations;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILeaveBalanceRepository _leaveBalanceRepository;
        private readonly IUserRepository _userRepository;

        public LeaveService(
            ILeaveRepository leaveRepository,
            ILeaveTypeRepository leaveTypeRepository,
            IEmployeeRepository employeeRepository,
            ILeaveBalanceRepository leaveBalanceRepository,
            IUserRepository userRepository)
        {
            _leaveRepository = leaveRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _employeeRepository = employeeRepository;
            _leaveBalanceRepository = leaveBalanceRepository;
            _userRepository = userRepository;
        }

        public async Task<LeaveResponseDto?> CreateLeaveAsync(CreateLeaveDto dto, string createdBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
                return null;

            var leaveType = await _leaveTypeRepository.GetByIdAsync(dto.LeaveTypeId);
            if (leaveType == null || !leaveType.IsActive)
                return null;

            if (await _leaveRepository.HasOverlappingLeaveAsync(dto.EmployeeId, dto.StartDate, dto.EndDate))
                return null;

            var year = dto.StartDate.Year;
            var leaveBalance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(
                dto.EmployeeId, dto.LeaveTypeId, year);

            if (leaveBalance == null)
                return null;

            if (!leaveBalance.HasSufficientBalance(dto.TotalDays))
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

        public async Task<PagedResultDto<LeaveResponseDto>> GetMyLeavesAsync(LeaveFilterDto filter, string userEmail)
        {
            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null)
                return new PagedResultDto<LeaveResponseDto>(
                    new List<LeaveResponseDto>(), 0, filter.PageNumber, filter.PageSize);

            var employee = await _employeeRepository.GetByUserIdAsync(user.Id);
            if (employee == null)
                return new PagedResultDto<LeaveResponseDto>(
                    new List<LeaveResponseDto>(), 0, filter.PageNumber, filter.PageSize);

            filter.EmployeeId = employee.Id;

            var (items, totalCount) = await _leaveRepository.GetFilteredLeavesAsync(filter);

            var leaveDtos = new List<LeaveResponseDto>();
            foreach (var leave in items)
                leaveDtos.Add(await MapToResponseDtoAsync(leave));

            return new PagedResultDto<LeaveResponseDto>(
                leaveDtos, totalCount, filter.PageNumber, filter.PageSize);
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

            var originalTotalDays = leave.TotalDays;
            var originalLeaveTypeId = leave.LeaveTypeId;

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

            if (dto.StartDate.HasValue || dto.EndDate.HasValue)
            {
                if (await _leaveRepository.HasOverlappingLeaveAsync(leave.EmployeeId, leave.StartDate, leave.EndDate, id))
                    return null;
            }

            if (dto.TotalDays.HasValue && dto.TotalDays.Value != originalTotalDays)
            {
                var year = leave.StartDate.Year;
                var leaveBalance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(
                    leave.EmployeeId, leave.LeaveTypeId, year);

                if (leaveBalance == null)
                    return null;

                if (!leaveBalance.HasSufficientBalance(leave.TotalDays))
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

            var year = leave.StartDate.Year;
            var leaveBalance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(
                leave.EmployeeId, leave.LeaveTypeId, year);

            if (leaveBalance == null || !leaveBalance.ConsumeLeave(leave.TotalDays))
                return false;

            await _leaveBalanceRepository.UpdateAsync(leaveBalance.Id, leaveBalance);

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

            if (leave.LeaveStatus == LeaveStatus.Approved)
            {
                var year = leave.StartDate.Year;
                var leaveBalance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(
                    leave.EmployeeId, leave.LeaveTypeId, year);

                if (leaveBalance != null)
                {
                    leaveBalance.RestoreLeave(leave.TotalDays);
                    await _leaveBalanceRepository.UpdateAsync(leaveBalance.Id, leaveBalance);
                }
            }

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
            var leaveBalance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(
                employeeId, leaveTypeId, year);

            if (leaveBalance == null)
                return 0;

            return (int)Math.Floor(leaveBalance.Available);
        }

        public async Task<bool> ValidateLeaveRequestAsync(string employeeId, string leaveTypeId, DateTime startDate, DateTime endDate, string? excludeLeaveId = null)
        {
            if (await _leaveRepository.HasOverlappingLeaveAsync(employeeId, startDate, endDate, excludeLeaveId))
                return false;

            var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveTypeId);
            if (leaveType == null || !leaveType.IsActive)
                return false;

            var totalDays = (decimal)(endDate - startDate).TotalDays + 1;
            var year = startDate.Year;

            var leaveBalance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(
                employeeId, leaveTypeId, year);

            if (leaveBalance == null)
                return false;

            return leaveBalance.HasSufficientBalance(totalDays);
        }

        private async Task<LeaveResponseDto> MapToResponseDtoAsync(Leave leave)
        {
            var employee = await _employeeRepository.GetByIdAsync(leave.EmployeeId);
            var leaveType = await _leaveTypeRepository.GetByIdAsync(leave.LeaveTypeId);

            string? approvedByName = null;
            if (!string.IsNullOrEmpty(leave.ApprovedBy))
            {
                var approver = await _userRepository.GetByIdAsync(leave.ApprovedBy);
                approvedByName = approver != null
                    ? $"{approver.FirstName} {approver.LastName}".Trim()
                    : null;
            }

            string? rejectedByName = null;
            if (!string.IsNullOrEmpty(leave.RejectedBy))
            {
                var rejector = await _userRepository.GetByIdAsync(leave.RejectedBy);
                rejectedByName = rejector != null
                    ? $"{rejector.FirstName} {rejector.LastName}".Trim()
                    : null;
            }

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
                ApprovedByName = approvedByName,
                ApprovedDate = leave.ApprovedDate,
                RejectedBy = leave.RejectedBy,
                RejectedByName = rejectedByName,
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
    };
}