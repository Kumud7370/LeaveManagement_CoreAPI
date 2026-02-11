using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Leave;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
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
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<LeaveResponseDto>>>> GetFilteredLeaves([FromBody] LeaveFilterDto filter)
        {
            var result = await _leaveService.GetFilteredLeavesAsync(filter);
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

        [HttpPatch("{id}/approve")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ApproveLeave(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveService.ApproveLeaveAsync(id, userId);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to approve leave. Leave may not be in pending status"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave approved successfully"));
        }

        [HttpPatch("{id}/reject")]
        public async Task<ActionResult<ApiResponseDto<bool>>> RejectLeave(string id, [FromBody] RejectLeaveRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveService.RejectLeaveAsync(id, userId, request.RejectionReason);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to reject leave. Leave may not be in pending status"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave rejected successfully"));
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
    }

    public class RejectLeaveRequestDto
    {
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class CancelLeaveRequestDto
    {
        public string CancellationReason { get; set; } = string.Empty;
    }
    public class ValidateLeaveRequestDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string LeaveTypeId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ExcludeLeaveId { get; set; }
    }
}