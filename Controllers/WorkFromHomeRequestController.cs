using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.WorkFromHome;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkFromHomeRequestController : ControllerBase
    {
        private readonly IWorkFromHomeRequestService _wfhRequestService;

        public WorkFromHomeRequestController(IWorkFromHomeRequestService wfhRequestService)
        {
            _wfhRequestService = wfhRequestService;
        }

        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<WfhRequestResponseDto>>> CreateWfhRequest([FromBody] CreateWfhRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<WfhRequestResponseDto>.ErrorResponse("User not authenticated"));

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized(ApiResponseDto<WfhRequestResponseDto>.ErrorResponse("User email not found"));

            var result = await _wfhRequestService.CreateWfhRequestByUserAsync(userId, userEmail, dto);

            if (result == null)
                return BadRequest(ApiResponseDto<WfhRequestResponseDto>.ErrorResponse(
                    "Failed to create WFH request. Employee record not found or overlapping request exists."));

            return Ok(ApiResponseDto<WfhRequestResponseDto>.SuccessResponse(result, "WFH request created successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<WfhRequestResponseDto>>> GetWfhRequestById(string id)
        {
            var result = await _wfhRequestService.GetWfhRequestByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponseDto<WfhRequestResponseDto>.ErrorResponse("WFH request not found"));

            return Ok(ApiResponseDto<WfhRequestResponseDto>.SuccessResponse(result));
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<WfhRequestResponseDto>>>> GetFilteredWfhRequests(
            [FromBody] WfhRequestFilterDto filter)
        {
            var result = await _wfhRequestService.GetFilteredWfhRequestsAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<WfhRequestResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<ApiResponseDto<List<WfhRequestResponseDto>>>> GetEmployeeWfhRequests(string employeeId)
        {
            var result = await _wfhRequestService.GetEmployeeWfhRequestsAsync(employeeId);
            return Ok(ApiResponseDto<List<WfhRequestResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("my-requests")]
        public async Task<ActionResult<ApiResponseDto<List<WfhRequestResponseDto>>>> GetMyWfhRequests()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<List<WfhRequestResponseDto>>.ErrorResponse("User not authenticated"));

            // Look up employee by email from JWT — this correctly resolves the Employee.Id
            // which is what WFH requests are stored against (not the User.Id)
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var result = await _wfhRequestService.GetMyWfhRequestsByUserAsync(userId, userEmail);
            return Ok(ApiResponseDto<List<WfhRequestResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponseDto<List<WfhRequestResponseDto>>>> GetPendingWfhRequests()
        {
            var result = await _wfhRequestService.GetPendingWfhRequestsAsync();
            return Ok(ApiResponseDto<List<WfhRequestResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponseDto<List<WfhRequestResponseDto>>>> GetActiveWfhRequests()
        {
            var result = await _wfhRequestService.GetActiveWfhRequestsAsync();
            return Ok(ApiResponseDto<List<WfhRequestResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("employee/{employeeId}/daterange")]
        public async Task<ActionResult<ApiResponseDto<List<WfhRequestResponseDto>>>> GetEmployeeWfhRequestsByDateRange(
            string employeeId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var result = await _wfhRequestService.GetEmployeeWfhRequestsByDateRangeAsync(employeeId, startDate, endDate);
            return Ok(ApiResponseDto<List<WfhRequestResponseDto>>.SuccessResponse(result));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<WfhRequestResponseDto>>> UpdateWfhRequest(
            string id,
            [FromBody] UpdateWfhRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<WfhRequestResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _wfhRequestService.UpdateWfhRequestAsync(id, dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<WfhRequestResponseDto>.ErrorResponse(
                    "Failed to update WFH request. Request not found, not pending, or overlapping request exists."));

            return Ok(ApiResponseDto<WfhRequestResponseDto>.SuccessResponse(result, "WFH request updated successfully"));
        }

        [HttpPatch("{id}/approve-reject")]
        public async Task<ActionResult<ApiResponseDto<WfhRequestResponseDto>>> ApproveRejectWfhRequest(
            string id,
            [FromBody] ApproveRejectWfhRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<WfhRequestResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _wfhRequestService.ApproveRejectWfhRequestAsync(id, dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<WfhRequestResponseDto>.ErrorResponse(
                    "Failed to process WFH request. Request not found or not in pending status."));

            var message = dto.Status == Models.Enums.ApprovalStatus.Approved
                ? "WFH request approved successfully"
                : "WFH request rejected successfully";

            return Ok(ApiResponseDto<WfhRequestResponseDto>.SuccessResponse(result, message));
        }

        [HttpPatch("{id}/cancel")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CancelWfhRequest(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _wfhRequestService.CancelWfhRequestAsync(id, userId);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse(
                    "Failed to cancel WFH request. Request not found or cannot be cancelled."));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "WFH request cancelled successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteWfhRequest(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _wfhRequestService.DeleteWfhRequestAsync(id, userId);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("WFH request not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "WFH request deleted successfully"));
        }

        [HttpGet("statistics/status")]
        public async Task<ActionResult<ApiResponseDto<Dictionary<string, int>>>> GetWfhRequestStatisticsByStatus()
        {
            var result = await _wfhRequestService.GetWfhRequestStatisticsByStatusAsync();
            return Ok(ApiResponseDto<Dictionary<string, int>>.SuccessResponse(result));
        }
    }
}