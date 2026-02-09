using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Attendance;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Check in for attendance
        /// </summary>
        [HttpPost("checkin")]
        public async Task<ActionResult<ApiResponseDto<AttendanceResponseDto>>> CheckIn([FromBody] CheckInDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<AttendanceResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _attendanceService.CheckInAsync(dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<AttendanceResponseDto>.ErrorResponse("Check-in failed. Already checked in or employee not found"));

            return Ok(ApiResponseDto<AttendanceResponseDto>.SuccessResponse(result, "Check-in successful"));
        }

        /// <summary>
        /// Check out from attendance
        /// </summary>
        [HttpPost("checkout")]
        public async Task<ActionResult<ApiResponseDto<AttendanceResponseDto>>> CheckOut([FromBody] CheckOutDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<AttendanceResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _attendanceService.CheckOutAsync(dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<AttendanceResponseDto>.ErrorResponse("Check-out failed. Not checked in yet or already checked out"));

            return Ok(ApiResponseDto<AttendanceResponseDto>.SuccessResponse(result, "Check-out successful"));
        }

        /// <summary>
        /// Mark manual attendance (Admin only)
        /// </summary>
        [HttpPost("manual")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponseDto<AttendanceResponseDto>>> MarkManualAttendance([FromBody] ManualAttendanceDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<AttendanceResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _attendanceService.MarkManualAttendanceAsync(dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<AttendanceResponseDto>.ErrorResponse("Failed to mark attendance. Attendance may already exist or employee not found"));

            return Ok(ApiResponseDto<AttendanceResponseDto>.SuccessResponse(result, "Attendance marked successfully"));
        }

        /// <summary>
        /// Get attendance by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<AttendanceResponseDto>>> GetAttendanceById(string id)
        {
            var result = await _attendanceService.GetAttendanceByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponseDto<AttendanceResponseDto>.ErrorResponse("Attendance not found"));

            return Ok(ApiResponseDto<AttendanceResponseDto>.SuccessResponse(result));
        }

        /// <summary>
        /// Get today's attendance for an employee
        /// </summary>
        [HttpGet("today/{employeeId}")]
        public async Task<ActionResult<ApiResponseDto<AttendanceResponseDto>>> GetTodayAttendance(string employeeId)
        {
            var result = await _attendanceService.GetTodayAttendanceAsync(employeeId);

            if (result == null)
                return NotFound(ApiResponseDto<AttendanceResponseDto>.ErrorResponse("No attendance found for today"));

            return Ok(ApiResponseDto<AttendanceResponseDto>.SuccessResponse(result));
        }

        /// <summary>
        /// Get attendance for specific date
        /// </summary>
        [HttpGet("date/{employeeId}/{date}")]
        public async Task<ActionResult<ApiResponseDto<AttendanceResponseDto>>> GetAttendanceByDate(
            string employeeId,
            DateTime date)
        {
            var result = await _attendanceService.GetAttendanceByDateAsync(employeeId, date);

            if (result == null)
                return NotFound(ApiResponseDto<AttendanceResponseDto>.ErrorResponse("Attendance not found for the specified date"));

            return Ok(ApiResponseDto<AttendanceResponseDto>.SuccessResponse(result));
        }

        /// <summary>
        /// Get filtered attendance with pagination
        /// </summary>
        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<AttendanceResponseDto>>>> GetFilteredAttendance(
            [FromBody] AttendanceFilterDto filter)
        {
            var result = await _attendanceService.GetFilteredAttendanceAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<AttendanceResponseDto>>.SuccessResponse(result));
        }

        /// <summary>
        /// Get employee attendance history
        /// </summary>
        [HttpGet("history/{employeeId}")]
        public async Task<ActionResult<ApiResponseDto<List<AttendanceResponseDto>>>> GetEmployeeAttendanceHistory(
            string employeeId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result = await _attendanceService.GetEmployeeAttendanceHistoryAsync(employeeId, startDate, endDate);
            return Ok(ApiResponseDto<List<AttendanceResponseDto>>.SuccessResponse(result));
        }

        /// <summary>
        /// Get attendance summary for an employee
        /// </summary>
        [HttpGet("summary/{employeeId}")]
        public async Task<ActionResult<ApiResponseDto<AttendanceSummaryDto>>> GetAttendanceSummary(
            string employeeId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var result = await _attendanceService.GetAttendanceSummaryAsync(employeeId, startDate, endDate);
            return Ok(ApiResponseDto<AttendanceSummaryDto>.SuccessResponse(result));
        }

        /// <summary>
        /// Get attendance statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponseDto<Dictionary<string, int>>>> GetAttendanceStatistics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var result = await _attendanceService.GetAttendanceStatisticsAsync(startDate, endDate);
            return Ok(ApiResponseDto<Dictionary<string, int>>.SuccessResponse(result));
        }

        /// <summary>
        /// Get late check-ins
        /// </summary>
        [HttpGet("late")]
        public async Task<ActionResult<ApiResponseDto<List<AttendanceResponseDto>>>> GetLateCheckIns(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var result = await _attendanceService.GetLateCheckInsAsync(startDate, endDate);
            return Ok(ApiResponseDto<List<AttendanceResponseDto>>.SuccessResponse(result));
        }

        /// <summary>
        /// Get early leaves
        /// </summary>
        [HttpGet("early-leave")]
        public async Task<ActionResult<ApiResponseDto<List<AttendanceResponseDto>>>> GetEarlyLeaves(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var result = await _attendanceService.GetEarlyLeavesAsync(startDate, endDate);
            return Ok(ApiResponseDto<List<AttendanceResponseDto>>.SuccessResponse(result));
        }

        /// <summary>
        /// Update attendance (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponseDto<AttendanceResponseDto>>> UpdateAttendance(
            string id,
            [FromBody] ManualAttendanceDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<AttendanceResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _attendanceService.UpdateAttendanceAsync(id, dto, userId);

            if (result == null)
                return NotFound(ApiResponseDto<AttendanceResponseDto>.ErrorResponse("Attendance not found"));

            return Ok(ApiResponseDto<AttendanceResponseDto>.SuccessResponse(result, "Attendance updated successfully"));
        }

        /// <summary>
        /// Delete attendance (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteAttendance(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _attendanceService.DeleteAttendanceAsync(id, userId);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Attendance not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Attendance deleted successfully"));
        }

        /// <summary>
        /// Approve attendance (Admin/Manager only)
        /// </summary>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ApproveAttendance(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _attendanceService.ApproveAttendanceAsync(id, userId);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Attendance not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Attendance approved successfully"));
        }

        /// <summary>
        /// Mark absent employees (Admin only - usually triggered by background job)
        /// </summary>
        [HttpPost("mark-absent")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAbsentEmployees([FromQuery] DateTime? date = null)
        {
            var targetDate = date ?? DateTime.UtcNow.Date.AddDays(-1);
            await _attendanceService.MarkAbsentEmployeesAsync(targetDate);
            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Absent employees marked successfully"));
        }
    }
}