using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.WorkFromHome;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class WorkFromHomeRequestService : IWorkFromHomeRequestService
    {
        private readonly IWorkFromHomeRequestRepository _wfhRequestRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public WorkFromHomeRequestService(
            IWorkFromHomeRequestRepository wfhRequestRepository,
            IEmployeeRepository employeeRepository)
        {
            _wfhRequestRepository = wfhRequestRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<WfhRequestResponseDto?> CreateWfhRequestAsync(string employeeId, CreateWfhRequestDto dto, string createdBy)
        {
            // Validate employee exists
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                return null;

            // Check for overlapping requests
            if (await _wfhRequestRepository.HasOverlappingRequestAsync(employeeId, dto.StartDate, dto.EndDate))
                return null;

            var wfhRequest = new WorkFromHomeRequest
            {
                EmployeeId = employeeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                Status = ApprovalStatus.Pending,
                CreatedBy = createdBy
            };

            var createdRequest = await _wfhRequestRepository.CreateAsync(wfhRequest);
            return await MapToResponseDtoAsync(createdRequest);
        }

        public async Task<WfhRequestResponseDto?> GetWfhRequestByIdAsync(string id)
        {
            var wfhRequest = await _wfhRequestRepository.GetByIdAsync(id);
            return wfhRequest != null ? await MapToResponseDtoAsync(wfhRequest) : null;
        }

        public async Task<PagedResultDto<WfhRequestResponseDto>> GetFilteredWfhRequestsAsync(WfhRequestFilterDto filter)
        {
            var (items, totalCount) = await _wfhRequestRepository.GetFilteredWfhRequestsAsync(filter);

            var wfhRequestDtos = new List<WfhRequestResponseDto>();
            foreach (var item in items)
            {
                wfhRequestDtos.Add(await MapToResponseDtoAsync(item));
            }

            return new PagedResultDto<WfhRequestResponseDto>(
                wfhRequestDtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        public async Task<List<WfhRequestResponseDto>> GetEmployeeWfhRequestsAsync(string employeeId)
        {
            var wfhRequests = await _wfhRequestRepository.GetByEmployeeIdAsync(employeeId);
            var result = new List<WfhRequestResponseDto>();

            foreach (var request in wfhRequests)
            {
                result.Add(await MapToResponseDtoAsync(request));
            }

            return result;
        }

        public async Task<List<WfhRequestResponseDto>> GetPendingWfhRequestsAsync()
        {
            var wfhRequests = await _wfhRequestRepository.GetPendingRequestsAsync();
            var result = new List<WfhRequestResponseDto>();

            foreach (var request in wfhRequests)
            {
                result.Add(await MapToResponseDtoAsync(request));
            }

            return result;
        }

        public async Task<List<WfhRequestResponseDto>> GetActiveWfhRequestsAsync()
        {
            var wfhRequests = await _wfhRequestRepository.GetActiveWfhRequestsAsync();
            var result = new List<WfhRequestResponseDto>();

            foreach (var request in wfhRequests)
            {
                result.Add(await MapToResponseDtoAsync(request));
            }

            return result;
        }

        public async Task<WfhRequestResponseDto?> UpdateWfhRequestAsync(string id, UpdateWfhRequestDto dto, string updatedBy)
        {
            var wfhRequest = await _wfhRequestRepository.GetByIdAsync(id);
            if (wfhRequest == null)
                return null;

            // Only allow updates to pending requests
            if (wfhRequest.Status != ApprovalStatus.Pending)
                return null;

            // Update only provided fields
            if (dto.StartDate.HasValue)
            {
                // Check for overlapping requests if dates are changed
                var endDate = dto.EndDate ?? wfhRequest.EndDate;
                if (await _wfhRequestRepository.HasOverlappingRequestAsync(
                    wfhRequest.EmployeeId, dto.StartDate.Value, endDate, id))
                    return null;

                wfhRequest.StartDate = dto.StartDate.Value;
            }

            if (dto.EndDate.HasValue)
            {
                // Check for overlapping requests if dates are changed
                var startDate = dto.StartDate ?? wfhRequest.StartDate;
                if (await _wfhRequestRepository.HasOverlappingRequestAsync(
                    wfhRequest.EmployeeId, startDate, dto.EndDate.Value, id))
                    return null;

                wfhRequest.EndDate = dto.EndDate.Value;
            }

            if (!string.IsNullOrEmpty(dto.Reason))
                wfhRequest.Reason = dto.Reason;

            wfhRequest.UpdatedBy = updatedBy;

            var updated = await _wfhRequestRepository.UpdateAsync(id, wfhRequest);
            return updated ? await MapToResponseDtoAsync(wfhRequest) : null;
        }

        public async Task<bool> DeleteWfhRequestAsync(string id, string deletedBy)
        {
            var wfhRequest = await _wfhRequestRepository.GetByIdAsync(id);
            if (wfhRequest == null)
                return false;

            wfhRequest.UpdatedBy = deletedBy;
            wfhRequest.DeletedAt = DateTime.UtcNow;

            return await _wfhRequestRepository.DeleteAsync(id);
        }

        public async Task<WfhRequestResponseDto?> ApproveRejectWfhRequestAsync(string id, ApproveRejectWfhRequestDto dto, string approverId)
        {
            var wfhRequest = await _wfhRequestRepository.GetByIdAsync(id);
            if (wfhRequest == null)
                return null;

            // Only allow approval/rejection of pending requests
            if (wfhRequest.Status != ApprovalStatus.Pending)
                return null;

            // Validate status
            if (dto.Status != ApprovalStatus.Approved && dto.Status != ApprovalStatus.Rejected)
                return null;

            wfhRequest.Status = dto.Status;
            wfhRequest.ApprovedBy = approverId;
            wfhRequest.ApprovedDate = DateTime.UtcNow;
            wfhRequest.UpdatedBy = approverId;

            if (dto.Status == ApprovalStatus.Rejected)
            {
                wfhRequest.RejectionReason = dto.RejectionReason;
            }

            var updated = await _wfhRequestRepository.UpdateAsync(id, wfhRequest);
            return updated ? await MapToResponseDtoAsync(wfhRequest) : null;
        }

        public async Task<bool> CancelWfhRequestAsync(string id, string cancelledBy)
        {
            var wfhRequest = await _wfhRequestRepository.GetByIdAsync(id);
            if (wfhRequest == null)
                return false;

            // Check if request can be cancelled
            if (!wfhRequest.CanBeCancelled())
                return false;

            wfhRequest.Status = ApprovalStatus.Cancelled;
            wfhRequest.UpdatedBy = cancelledBy;

            return await _wfhRequestRepository.UpdateAsync(id, wfhRequest);
        }

        public async Task<Dictionary<string, int>> GetWfhRequestStatisticsByStatusAsync()
        {
            var statistics = new Dictionary<string, int>();

            foreach (ApprovalStatus status in Enum.GetValues(typeof(ApprovalStatus)))
            {
                var count = await _wfhRequestRepository.GetWfhRequestCountByStatusAsync(status);
                statistics[status.ToString()] = count;
            }

            return statistics;
        }

        public async Task<List<WfhRequestResponseDto>> GetEmployeeWfhRequestsByDateRangeAsync(string employeeId, DateTime startDate, DateTime endDate)
        {
            var wfhRequests = await _wfhRequestRepository.GetEmployeeWfhRequestsByDateRangeAsync(employeeId, startDate, endDate);
            var result = new List<WfhRequestResponseDto>();

            foreach (var request in wfhRequests)
            {
                result.Add(await MapToResponseDtoAsync(request));
            }

            return result;
        }

        public async Task<WfhRequestResponseDto?> CreateWfhRequestByUserAsync(string userId, string userEmail, CreateWfhRequestDto dto)
        {
            // Find employee by email
            var employee = await _employeeRepository.GetByEmailAsync(userEmail);
            if (employee == null)
                return null;

            // Check for overlapping requests
            if (await _wfhRequestRepository.HasOverlappingRequestAsync(employee.Id, dto.StartDate, dto.EndDate))
                return null;

            var wfhRequest = new WorkFromHomeRequest
            {
                EmployeeId = employee.Id,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                Status = ApprovalStatus.Pending,
                CreatedBy = userId
            };

            var createdRequest = await _wfhRequestRepository.CreateAsync(wfhRequest);
            return await MapToResponseDtoAsync(createdRequest);
        }

        private async Task<WfhRequestResponseDto> MapToResponseDtoAsync(WorkFromHomeRequest wfhRequest)
        {
            var employee = await _employeeRepository.GetByIdAsync(wfhRequest.EmployeeId);

            Employee? approver = null;
            if (!string.IsNullOrEmpty(wfhRequest.ApprovedBy))
            {
                approver = await _employeeRepository.GetByIdAsync(wfhRequest.ApprovedBy);
            }

            return new WfhRequestResponseDto
            {
                Id = wfhRequest.Id,
                EmployeeId = wfhRequest.EmployeeId,
                EmployeeCode = employee?.EmployeeCode ?? string.Empty,
                EmployeeName = employee?.GetFullName() ?? string.Empty,
                StartDate = wfhRequest.StartDate,
                EndDate = wfhRequest.EndDate,
                TotalDays = wfhRequest.GetTotalDays(),
                Reason = wfhRequest.Reason,
                Status = wfhRequest.Status,
                StatusName = wfhRequest.Status.ToString(),
                ApprovedBy = wfhRequest.ApprovedBy,
                ApproverName = approver?.GetFullName(),
                ApprovedDate = wfhRequest.ApprovedDate,
                RejectionReason = wfhRequest.RejectionReason,
                IsActive = wfhRequest.IsActive(),
                CanBeCancelled = wfhRequest.CanBeCancelled(),
                CreatedAt = wfhRequest.CreatedAt,
                UpdatedAt = wfhRequest.UpdatedAt
            };
        }
    }
}