using AttendanceManagementSystem.Models.DTOs.AttendanceRegularization;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class AttendanceRegularizationService : IAttendanceRegularizationService
    {
        private readonly IAttendanceRegularizationRepository _regularizationRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private const int MAX_PENDING_REQUESTS = 3;
        private const int MAX_DAYS_BACK = 7;

        public AttendanceRegularizationService(
            IAttendanceRegularizationRepository regularizationRepository,
            IAttendanceRepository attendanceRepository,
            IEmployeeRepository employeeRepository)
        {
            _regularizationRepository = regularizationRepository;
            _attendanceRepository = attendanceRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<RegularizationResponseDto?> RequestRegularizationAsync(RegularizationRequestDto dto, string requestedBy)
        {
           
            var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
                return null;

          
            var pendingCount = await _regularizationRepository.GetPendingCountByEmployeeAsync(dto.EmployeeId);
            if (pendingCount >= MAX_PENDING_REQUESTS)
                throw new InvalidOperationException($"Cannot have more than {MAX_PENDING_REQUESTS} pending regularization requests");

            
            var daysDifference = (DateTime.UtcNow.Date - dto.AttendanceDate.Date).Days;
            if (daysDifference > MAX_DAYS_BACK)
                throw new InvalidOperationException($"Regularization can only be requested within {MAX_DAYS_BACK} days");

           
            var existingRegularization = await _regularizationRepository.GetByEmployeeAndDateAsync(
                dto.EmployeeId,
                dto.AttendanceDate);
            if (existingRegularization != null)
                throw new InvalidOperationException("A pending regularization request already exists for this date");

            
            var attendance = await _attendanceRepository.GetByEmployeeAndDateAsync(dto.EmployeeId, dto.AttendanceDate);

            var regularization = new AttendanceRegularization
            {
                AttendanceId = attendance?.Id,
                EmployeeId = dto.EmployeeId,
                AttendanceDate = dto.AttendanceDate.Date,
                RegularizationType = dto.RegularizationType,
                RequestedCheckIn = dto.RequestedCheckIn,
                RequestedCheckOut = dto.RequestedCheckOut,
                OriginalCheckIn = attendance?.CheckInTime,
                OriginalCheckOut = attendance?.CheckOutTime,
                Reason = dto.Reason,
                Status = RegularizationStatus.Pending,
                RequestedBy = requestedBy,
                RequestedAt = DateTime.UtcNow
            };

           
            if (!regularization.IsValidTimeRange())
                throw new InvalidOperationException("Check-out time must be after check-in time");

            var created = await _regularizationRepository.CreateAsync(regularization);
            return await MapToResponseDtoAsync(created);
        }

        public async Task<RegularizationResponseDto?> ApproveRegularizationAsync(
            string id,
            RegularizationApprovalDto dto,
            string approvedBy)
        {
            var regularization = await _regularizationRepository.GetByIdAsync(id);
            if (regularization == null)
                return null;

            if (!regularization.CanBeApproved())
                throw new InvalidOperationException("Only pending regularizations can be approved or rejected");

            regularization.Status = dto.IsApproved ? RegularizationStatus.Approved : RegularizationStatus.Rejected;
            regularization.ApprovedBy = approvedBy;
            regularization.ApprovedAt = DateTime.UtcNow;
            regularization.RejectionReason = dto.RejectionReason;
            regularization.UpdatedBy = approvedBy;

           
            if (dto.IsApproved)
            {
                await UpdateAttendanceFromRegularizationAsync(regularization, approvedBy);
            }

            await _regularizationRepository.UpdateAsync(id, regularization);
            return await MapToResponseDtoAsync(regularization);
        }

        private async Task UpdateAttendanceFromRegularizationAsync(AttendanceRegularization regularization, string updatedBy)
        {
            var attendance = await _attendanceRepository.GetByEmployeeAndDateAsync(
                regularization.EmployeeId,
                regularization.AttendanceDate);

            if (attendance == null)
            {
              
                attendance = new Attendance
                {
                    EmployeeId = regularization.EmployeeId,
                    AttendanceDate = regularization.AttendanceDate.Date,
                    CheckInTime = regularization.RequestedCheckIn,
                    CheckOutTime = regularization.RequestedCheckOut,
                    Status = AttendanceStatus.Present,
                    CheckInMethod = CheckInMethod.Manual,
                    Remarks = $"Regularized: {regularization.Reason}",
                    CreatedBy = updatedBy
                };

            
                if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
                {
                    attendance.WorkingHours = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
                    attendance.OvertimeHours = attendance.CalculateOvertimeHours(8.0);
                }

                await _attendanceRepository.CreateAsync(attendance);
            }
            else
            {
               
                attendance.CheckInTime = regularization.RequestedCheckIn ?? attendance.CheckInTime;
                attendance.CheckOutTime = regularization.RequestedCheckOut ?? attendance.CheckOutTime;
                attendance.UpdatedBy = updatedBy;

              
                if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
                {
                    attendance.WorkingHours = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
                    attendance.OvertimeHours = attendance.CalculateOvertimeHours(8.0);
                    attendance.Status = AttendanceStatus.Present;
                }

                if (!string.IsNullOrEmpty(attendance.Remarks))
                {
                    attendance.Remarks += $"; Regularized: {regularization.Reason}";
                }
                else
                {
                    attendance.Remarks = $"Regularized: {regularization.Reason}";
                }

                await _attendanceRepository.UpdateAsync(attendance.Id, attendance);
            }
        }

        public async Task<RegularizationResponseDto?> GetByIdAsync(string id)
        {
            var regularization = await _regularizationRepository.GetByIdAsync(id);
            return regularization != null ? await MapToResponseDtoAsync(regularization) : null;
        }

        public async Task<List<RegularizationResponseDto>> GetByEmployeeIdAsync(string employeeId)
        {
            var regularizations = await _regularizationRepository.GetByEmployeeIdAsync(employeeId);
            var dtos = new List<RegularizationResponseDto>();

            foreach (var regularization in regularizations)
            {
                dtos.Add(await MapToResponseDtoAsync(regularization));
            }

            return dtos;
        }

        public async Task<PagedResultDto<RegularizationResponseDto>> GetFilteredAsync(RegularizationFilterDto filter)
        {
            var (items, totalCount) = await _regularizationRepository.GetFilteredAsync(filter);

            var dtos = new List<RegularizationResponseDto>();
            foreach (var item in items)
            {
                dtos.Add(await MapToResponseDtoAsync(item));
            }

            return new PagedResultDto<RegularizationResponseDto>(
                dtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        public async Task<List<RegularizationResponseDto>> GetPendingRegularizationsAsync()
        {
            var regularizations = await _regularizationRepository.GetPendingRegularizationsAsync();
            var dtos = new List<RegularizationResponseDto>();

            foreach (var regularization in regularizations)
            {
                dtos.Add(await MapToResponseDtoAsync(regularization));
            }

            return dtos;
        }

        public async Task<bool> CancelRegularizationAsync(string id, string cancelledBy)
        {
            var regularization = await _regularizationRepository.GetByIdAsync(id);
            if (regularization == null)
                return false;

            if (!regularization.CanBeApproved())
                return false;

            regularization.Status = RegularizationStatus.Cancelled;
            regularization.UpdatedBy = cancelledBy;

            return await _regularizationRepository.UpdateAsync(id, regularization);
        }

        public async Task<int> GetPendingCountByEmployeeAsync(string employeeId)
        {
            return await _regularizationRepository.GetPendingCountByEmployeeAsync(employeeId);
        }

        private async Task<RegularizationResponseDto> MapToResponseDtoAsync(AttendanceRegularization regularization)
        {
            var employee = await _employeeRepository.GetByIdAsync(regularization.EmployeeId);
            var approver = regularization.ApprovedBy != null
                ? await _employeeRepository.GetByIdAsync(regularization.ApprovedBy)
                : null;

            return new RegularizationResponseDto
            {
                Id = regularization.Id,
                AttendanceId = regularization.AttendanceId,
                EmployeeId = regularization.EmployeeId,
                EmployeeCode = employee?.EmployeeCode ?? "",
                EmployeeName = employee?.GetFullName() ?? "",
                AttendanceDate = regularization.AttendanceDate,
                RegularizationType = regularization.RegularizationType,
                RegularizationTypeName = regularization.RegularizationType.ToString(),
                RequestedCheckIn = regularization.RequestedCheckIn,
                RequestedCheckOut = regularization.RequestedCheckOut,
                OriginalCheckIn = regularization.OriginalCheckIn,
                OriginalCheckOut = regularization.OriginalCheckOut,
                Reason = regularization.Reason,
                Status = regularization.Status,
                StatusName = regularization.Status.ToString(),
                ApprovedBy = regularization.ApprovedBy,
                ApprovedByName = approver?.GetFullName(),
                ApprovedAt = regularization.ApprovedAt,
                RejectionReason = regularization.RejectionReason,
                RequestedBy = regularization.RequestedBy,
                RequestedAt = regularization.RequestedAt,
                CreatedAt = regularization.CreatedAt,
                UpdatedAt = regularization.UpdatedAt
            };
        }
    }
}