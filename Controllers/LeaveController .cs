using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Holiday;
using AttendanceManagementSystem.Models.DTOs.Leave;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Implementations;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;
        private readonly IUserRepository _userRepository;
        private readonly HolidayRepository _holidayRepository;

        public LeaveController(
        ILeaveService leaveService,
        IUserRepository userRepository,
         HolidayRepository holidayRepository)                 
        {
            _leaveService = leaveService;
            _userRepository = userRepository;
            _holidayRepository = holidayRepository;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<LeaveResponseDto>>> CreateLeave([FromBody] CreateLeaveDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<LeaveResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _leaveService.CreateLeaveAsync(dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<LeaveResponseDto>.ErrorResponse("Failed to create leave. Check for overlapping leaves or insufficient leave balance"));

            return Ok(ApiResponseDto<LeaveResponseDto>.SuccessResponse(result, "Leave created successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<LeaveResponseDto>>> GetLeaveById(string id)
        {
            var result = await _leaveService.GetLeaveByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponseDto<LeaveResponseDto>.ErrorResponse("Leave not found"));

            return Ok(ApiResponseDto<LeaveResponseDto>.SuccessResponse(result));
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<LeaveResponseDto>>>> GetFilteredLeaves(
    [FromBody] LeaveFilterDto filter)
        {
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            bool isPrivileged = roles.Any(r =>
                r is "Admin" or "Tehsildar" or "NayabTehsildar" or "HR");

            if (!isPrivileged)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value
                         ?? User.FindFirst("email")?.Value;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(ApiResponseDto<PagedResultDto<LeaveResponseDto>>
                        .ErrorResponse("Not authenticated"));

                var result = await _leaveService.GetMyLeavesAsync(filter, email);
                return Ok(ApiResponseDto<PagedResultDto<LeaveResponseDto>>.SuccessResponse(result));
            }

            var adminResult = await _leaveService.GetFilteredLeavesAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<LeaveResponseDto>>.SuccessResponse(adminResult));
        }

        [HttpPost("my-leaves")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<LeaveResponseDto>>>> GetMyLeaves([FromBody] LeaveFilterDto filter)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                     ?? User.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized(ApiResponseDto<PagedResultDto<LeaveResponseDto>>.ErrorResponse("User email not found in token"));

            var result = await _leaveService.GetMyLeavesAsync(filter, email);
            return Ok(ApiResponseDto<PagedResultDto<LeaveResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<ApiResponseDto<List<LeaveResponseDto>>>> GetLeavesByEmployee(string employeeId)
        {
            var result = await _leaveService.GetLeavesByEmployeeIdAsync(employeeId);
            return Ok(ApiResponseDto<List<LeaveResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponseDto<List<LeaveResponseDto>>>> GetPendingLeaves()
        {
            var result = await _leaveService.GetPendingLeavesAsync();
            return Ok(ApiResponseDto<List<LeaveResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<ApiResponseDto<List<LeaveResponseDto>>>> GetUpcomingLeaves([FromQuery] int days = 7)
        {
            var result = await _leaveService.GetUpcomingLeavesAsync(days);
            return Ok(ApiResponseDto<List<LeaveResponseDto>>.SuccessResponse(result));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<LeaveResponseDto>>> UpdateLeave(string id, [FromBody] UpdateLeaveDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<LeaveResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _leaveService.UpdateLeaveAsync(id, dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<LeaveResponseDto>.ErrorResponse("Failed to update leave. Leave may not be in pending status or has overlapping dates"));

            return Ok(ApiResponseDto<LeaveResponseDto>.SuccessResponse(result, "Leave updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteLeave(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveService.DeleteLeaveAsync(id, userId);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Leave not found or cannot be deleted"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave deleted successfully"));
        }

        [HttpGet("statistics/my-status")]
        public async Task<ActionResult<ApiResponseDto<Dictionary<string, int>>>> GetMyLeaveStatisticsByStatus()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                     ?? User.FindFirst("email")?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(ApiResponseDto<Dictionary<string, int>>
                    .ErrorResponse("User not authenticated"));

            var result = await _leaveService.GetMyLeaveStatisticsByStatusAsync(email);
            return Ok(ApiResponseDto<Dictionary<string, int>>.SuccessResponse(result));
        }

        [HttpPatch("{id}/admin-approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> AdminApprove(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveService.AdminApproveLeaveAsync(id, userId);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse(
                    "Failed to approve leave. Leave must be in Pending status."));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave approved by Admin. Forwarded to Nayab Tehsildar."));
        }

        [HttpPatch("{id}/nayab-approve")]
        [Authorize(Roles = "NayabTehsildar")]
        public async Task<ActionResult<ApiResponseDto<bool>>> NayabApprove(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveService.NayabApproveLeaveAsync(id, userId);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse(
                    "Failed to approve leave. Leave must be in AdminApproved status."));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave approved by Nayab Tehsildar. Forwarded to Tehsildar."));
        }

        [HttpPatch("{id}/tehsildar-approve")]
        [Authorize(Roles = "Tehsildar")]
        public async Task<ActionResult<ApiResponseDto<bool>>> TehsildarApprove(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveService.TehsildarApproveLeaveAsync(id, userId);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse(
                    "Failed to approve leave. Leave must be in NayabApproved status or insufficient balance."));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave fully approved by Tehsildar."));
        }

        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Admin,NayabTehsildar,Tehsildar")]
        public async Task<ActionResult<ApiResponseDto<bool>>> Reject(
            string id, [FromBody] RejectLeaveRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveService.RejectLeaveAsync(id, userId, dto.RejectionReason);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse(
                    "Failed to reject leave. Leave may already be fully approved or cancelled."));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave rejected successfully."));
        }

        [HttpPatch("{id}/cancel")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CancelLeave(string id, [FromBody] CancelLeaveRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveService.CancelLeaveAsync(id, userId, request.CancellationReason);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Leave cannot be cancelled"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave cancelled successfully"));
        }

        [HttpGet("statistics/status")]
        public async Task<ActionResult<ApiResponseDto<Dictionary<string, int>>>> GetLeaveStatisticsByStatus()
        {
            var result = await _leaveService.GetLeaveStatisticsByStatusAsync();
            return Ok(ApiResponseDto<Dictionary<string, int>>.SuccessResponse(result));
        }

        [HttpGet("balance/{employeeId}/{leaveTypeId}/{year}")]
        public async Task<ActionResult<ApiResponseDto<int>>> GetRemainingLeaveDays(string employeeId, string leaveTypeId, int year)
        {
            var result = await _leaveService.GetRemainingLeaveDaysAsync(employeeId, leaveTypeId, year);
            return Ok(ApiResponseDto<int>.SuccessResponse(result));
        }

        [HttpPost("department-leaves")]
        [Authorize(Roles = "HR,Admin,Tehsildar,NayabTehsildar")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<LeaveResponseDto>>>> GetDepartmentLeaves(
         [FromBody] LeaveFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            if (roles.Contains("HR"))
            {
                var hrUser = await _userRepository.GetByIdAsync(userId!); 
                if (hrUser == null || string.IsNullOrEmpty(hrUser.DepartmentId))
                    return Forbid();

                filter.DepartmentId = hrUser.DepartmentId;
            }

            var result = await _leaveService.GetDepartmentLeavesAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<LeaveResponseDto>>.SuccessResponse(result));
        }

        [HttpPost("validate")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ValidateLeaveRequest([FromBody] ValidateLeaveRequestDto request)
        {
            var result = await _leaveService.ValidateLeaveRequestAsync(
                request.EmployeeId,
                request.LeaveTypeId,
                request.StartDate,
                request.EndDate,
                request.ExcludeLeaveId
            );

            return Ok(ApiResponseDto<bool>.SuccessResponse(result));
        }

        [HttpGet("holidays/{year:int}")]
        [AllowAnonymous] // or keep [Authorize] — your choice
        public async Task<ActionResult<ApiResponseDto<List<HolidayDto>>>>
    GetHolidays(int year)
        {
            var holidays = await _holidayRepository.GetByYearAsync(year);
            var dtos = holidays.Select(h => new HolidayDto
            {
                Id = h.Id,
                Date = h.Date.ToString("yyyy-MM-dd"),
                Name = h.Name,
                NameMr = h.NameMr,
                Year = h.Year
            }).ToList();
            return Ok(ApiResponseDto<List<HolidayDto>>.SuccessResponse(dtos));
        }

        [HttpPost("holidays")]
        public async Task<ActionResult<ApiResponseDto<object>>> CreateHoliday(
    [FromBody] CreateHolidayDto dto)
        {
            // Parse and strip time component — force to UTC midnight
            if (!DateTime.TryParse(dto.Date, out var parsedDate))
                return BadRequest(ApiResponseDto<object>.ErrorResponse("Invalid date format"));

            var date = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc); // ← KEY FIX

            var holiday = new Holiday
            {
                Date = date,
                Name = dto.Name ?? dto.Date,
                NameMr = dto.NameMr,
                Year = date.Year
            };

            var created = await _holidayRepository.CreateAsync(holiday);
            return Ok(ApiResponseDto<object>.SuccessResponse(new
            {
                id = created.Id,
                date = created.Date.ToString("yyyy-MM-dd"),
                name = created.Name,
                nameMr = created.NameMr,
                year = created.Year
            }));
        }

        [HttpDelete("holidays/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>>
            DeleteHoliday(string id)
        {
            var result = await _holidayRepository.DeleteAsync(id);
            return Ok(ApiResponseDto<bool>.SuccessResponse(result));
        }
    }
}