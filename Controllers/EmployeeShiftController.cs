using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.EmployeeShift;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeShiftController : ControllerBase
    {
        private readonly IEmployeeShiftService _employeeShiftService;

        public EmployeeShiftController(IEmployeeShiftService employeeShiftService)
        {
            _employeeShiftService = employeeShiftService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<EmployeeShiftResponseDto>>> CreateEmployeeShift([FromBody] CreateEmployeeShiftDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<EmployeeShiftResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _employeeShiftService.CreateEmployeeShiftAsync(dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<EmployeeShiftResponseDto>.ErrorResponse("Failed to create shift assignment. Check for overlapping shifts, employee exists, shift is active, or pending limit reached (max 3)"));

            return Ok(ApiResponseDto<EmployeeShiftResponseDto>.SuccessResponse(result, "Shift assignment created successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<EmployeeShiftResponseDto>>> GetEmployeeShiftById(string id)
        {
            var result = await _employeeShiftService.GetEmployeeShiftByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponseDto<EmployeeShiftResponseDto>.ErrorResponse("Employee shift not found"));

            return Ok(ApiResponseDto<EmployeeShiftResponseDto>.SuccessResponse(result));
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<EmployeeShiftResponseDto>>>> GetFilteredEmployeeShifts([FromBody] EmployeeShiftFilterDto filter)
        {
            var result = await _employeeShiftService.GetFilteredEmployeeShiftsAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<EmployeeShiftResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<ApiResponseDto<List<EmployeeShiftResponseDto>>>> GetEmployeeShiftsByEmployee(string employeeId)
        {
            var result = await _employeeShiftService.GetEmployeeShiftsByEmployeeIdAsync(employeeId);
            return Ok(ApiResponseDto<List<EmployeeShiftResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("employee/{employeeId}/current")]
        public async Task<ActionResult<ApiResponseDto<EmployeeShiftResponseDto>>> GetCurrentShiftForEmployee(string employeeId)
        {
            var result = await _employeeShiftService.GetCurrentShiftForEmployeeAsync(employeeId);

            if (result == null)
                return NotFound(ApiResponseDto<EmployeeShiftResponseDto>.ErrorResponse("No current shift found for employee"));

            return Ok(ApiResponseDto<EmployeeShiftResponseDto>.SuccessResponse(result));
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponseDto<List<EmployeeShiftResponseDto>>>> GetPendingShiftChanges()
        {
            var result = await _employeeShiftService.GetPendingShiftChangesAsync();
            return Ok(ApiResponseDto<List<EmployeeShiftResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<ApiResponseDto<List<EmployeeShiftResponseDto>>>> GetUpcomingShiftChanges([FromQuery] int days = 7)
        {
            var result = await _employeeShiftService.GetUpcomingShiftChangesAsync(days);
            return Ok(ApiResponseDto<List<EmployeeShiftResponseDto>>.SuccessResponse(result));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<EmployeeShiftResponseDto>>> UpdateEmployeeShift(string id, [FromBody] UpdateEmployeeShiftDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<EmployeeShiftResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _employeeShiftService.UpdateEmployeeShiftAsync(id, dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<EmployeeShiftResponseDto>.ErrorResponse("Failed to update shift assignment. Assignment may not be modifiable or has overlapping dates"));

            return Ok(ApiResponseDto<EmployeeShiftResponseDto>.SuccessResponse(result, "Shift assignment updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteEmployeeShift(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _employeeShiftService.DeleteEmployeeShiftAsync(id, userId);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Employee shift not found or cannot be deleted"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Shift assignment deleted successfully"));
        }

        [HttpPatch("{id}/approve")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ApproveShiftChange(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _employeeShiftService.ApproveShiftChangeAsync(id, userId);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to approve shift change. Assignment may not be in pending status"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Shift change approved successfully"));
        }

        [HttpPatch("{id}/reject")]
        public async Task<ActionResult<ApiResponseDto<bool>>> RejectShiftChange(string id, [FromBody] RejectShiftChangeRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _employeeShiftService.RejectShiftChangeAsync(id, userId, request.RejectionReason);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to reject shift change. Assignment may not be in pending status"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Shift change rejected successfully"));
        }

        [HttpPatch("{id}/cancel")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CancelShiftChange(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _employeeShiftService.CancelShiftChangeAsync(id, userId);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Shift change cannot be cancelled"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Shift change cancelled successfully"));
        }

        [HttpGet("statistics/status")]
        public async Task<ActionResult<ApiResponseDto<Dictionary<string, int>>>> GetShiftChangeStatisticsByStatus()
        {
            var result = await _employeeShiftService.GetShiftChangeStatisticsByStatusAsync();
            return Ok(ApiResponseDto<Dictionary<string, int>>.SuccessResponse(result));
        }

        [HttpPost("validate")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ValidateShiftAssignment([FromBody] ValidateShiftAssignmentRequestDto request)
        {
            var result = await _employeeShiftService.ValidateShiftAssignmentAsync(
                request.EmployeeId,
                request.EffectiveFrom,
                request.EffectiveTo,
                request.ExcludeId
            );

            return Ok(ApiResponseDto<bool>.SuccessResponse(result));
        }
    }

    public class RejectShiftChangeRequestDto
    {
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class ValidateShiftAssignmentRequestDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? ExcludeId { get; set; }
    }
}