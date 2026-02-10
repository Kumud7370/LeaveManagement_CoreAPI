using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.LeaveBalance;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveBalanceController : ControllerBase
    {
        private readonly ILeaveBalanceService _leaveBalanceService;

        public LeaveBalanceController(ILeaveBalanceService leaveBalanceService)
        {
            _leaveBalanceService = leaveBalanceService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<LeaveBalanceResponseDto>>> CreateLeaveBalance([FromBody] CreateLeaveBalanceDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<LeaveBalanceResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _leaveBalanceService.CreateLeaveBalanceAsync(dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<LeaveBalanceResponseDto>.ErrorResponse("Failed to create leave balance. Balance may already exist or validation failed"));

            return Ok(ApiResponseDto<LeaveBalanceResponseDto>.SuccessResponse(result, "Leave balance created successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<LeaveBalanceResponseDto>>> GetLeaveBalanceById(string id)
        {
            var result = await _leaveBalanceService.GetLeaveBalanceByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponseDto<LeaveBalanceResponseDto>.ErrorResponse("Leave balance not found"));

            return Ok(ApiResponseDto<LeaveBalanceResponseDto>.SuccessResponse(result));
        }

        [HttpGet("employee/{employeeId}/leave-type/{leaveTypeId}/year/{year}")]
        public async Task<ActionResult<ApiResponseDto<LeaveBalanceResponseDto>>> GetByEmployeeAndLeaveType(
            string employeeId, string leaveTypeId, int year)
        {
            var result = await _leaveBalanceService.GetByEmployeeAndLeaveTypeAsync(employeeId, leaveTypeId, year);

            if (result == null)
                return NotFound(ApiResponseDto<LeaveBalanceResponseDto>.ErrorResponse("Leave balance not found"));

            return Ok(ApiResponseDto<LeaveBalanceResponseDto>.SuccessResponse(result));
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<LeaveBalanceResponseDto>>>> GetFilteredLeaveBalances(
            [FromBody] LeaveBalanceFilterDto filter)
        {
            var result = await _leaveBalanceService.GetFilteredLeaveBalancesAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<LeaveBalanceResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<ApiResponseDto<List<LeaveBalanceResponseDto>>>> GetByEmployeeId(
            string employeeId, [FromQuery] int? year = null)
        {
            var result = await _leaveBalanceService.GetByEmployeeIdAsync(employeeId, year);
            return Ok(ApiResponseDto<List<LeaveBalanceResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("employee/{employeeId}/summary/{year}")]
        public async Task<ActionResult<ApiResponseDto<EmployeeLeaveBalanceSummaryDto>>> GetEmployeeBalanceSummary(
            string employeeId, int year)
        {
            var result = await _leaveBalanceService.GetEmployeeBalanceSummaryAsync(employeeId, year);

            if (result == null)
                return NotFound(ApiResponseDto<EmployeeLeaveBalanceSummaryDto>.ErrorResponse("Employee not found"));

            return Ok(ApiResponseDto<EmployeeLeaveBalanceSummaryDto>.SuccessResponse(result));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<LeaveBalanceResponseDto>>> UpdateLeaveBalance(
            string id, [FromBody] UpdateLeaveBalanceDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<LeaveBalanceResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _leaveBalanceService.UpdateLeaveBalanceAsync(id, dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<LeaveBalanceResponseDto>.ErrorResponse("Failed to update leave balance"));

            return Ok(ApiResponseDto<LeaveBalanceResponseDto>.SuccessResponse(result, "Leave balance updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteLeaveBalance(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveBalanceService.DeleteLeaveBalanceAsync(id, userId);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Leave balance not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave balance deleted successfully"));
        }

        [HttpPatch("{id}/adjust")]
        public async Task<ActionResult<ApiResponseDto<bool>>> AdjustLeaveBalance(
            string id, [FromBody] AdjustLeaveBalanceDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveBalanceService.AdjustLeaveBalanceAsync(id, dto, userId);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to adjust leave balance"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave balance adjusted successfully"));
        }

        [HttpPost("carry-forward")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CarryForwardLeave([FromBody] CarryForwardDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveBalanceService.CarryForwardLeaveAsync(dto, userId);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to carry forward leave. Check balance availability and carry forward rules"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave carried forward successfully"));
        }

        [HttpGet("alerts/low-balance")]
        public async Task<ActionResult<ApiResponseDto<List<LeaveBalanceResponseDto>>>> GetLowBalanceAlerts(
            [FromQuery] decimal threshold = 2)
        {
            var result = await _leaveBalanceService.GetLowBalanceAlertsAsync(threshold);
            return Ok(ApiResponseDto<List<LeaveBalanceResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("alerts/expiring-soon/{year}")]
        public async Task<ActionResult<ApiResponseDto<List<LeaveBalanceResponseDto>>>> GetExpiringSoon(
            int year, [FromQuery] int daysThreshold = 30)
        {
            var result = await _leaveBalanceService.GetExpiringSoonAsync(year, daysThreshold);
            return Ok(ApiResponseDto<List<LeaveBalanceResponseDto>>.SuccessResponse(result));
        }

        [HttpPost("initialize/employee/{employeeId}/year/{year}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> InitializeBalanceForEmployee(
            string employeeId, int year)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveBalanceService.InitializeBalanceForEmployeeAsync(employeeId, year, userId);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to initialize leave balance"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave balance initialized successfully"));
        }

        [HttpPost("initialize/bulk")]
        public async Task<ActionResult<ApiResponseDto<Dictionary<string, int>>>> BulkInitializeBalances(
            [FromBody] BulkInitializeBalanceDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<Dictionary<string, int>>.ErrorResponse("User not authenticated"));

            var result = await _leaveBalanceService.BulkInitializeBalancesAsync(dto, userId);
            return Ok(ApiResponseDto<Dictionary<string, int>>.SuccessResponse(result, "Bulk initialization completed"));
        }

        [HttpPatch("{id}/recalculate")]
        public async Task<ActionResult<ApiResponseDto<bool>>> RecalculateBalance(string id)
        {
            var result = await _leaveBalanceService.RecalculateBalanceAsync(id);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Leave balance not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave balance recalculated successfully"));
        }
    }
}