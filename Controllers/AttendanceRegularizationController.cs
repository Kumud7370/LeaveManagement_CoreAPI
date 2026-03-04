using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.AttendanceRegularization;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceRegularizationController : ControllerBase
    {
        private readonly IAttendanceRegularizationService _regularizationService;

        public AttendanceRegularizationController(IAttendanceRegularizationService regularizationService)
        {
            _regularizationService = regularizationService;
        }

        // ── POST api/AttendanceRegularization/request ──────────────────────────
        [HttpPost("request")]
        public async Task<ActionResult<ApiResponseDto<RegularizationResponseDto>>> RequestRegularization(
            [FromBody] RegularizationRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<RegularizationResponseDto>.ErrorResponse("User not authenticated"));

            try
            {
                var result = await _regularizationService.RequestRegularizationAsync(dto, userId);
                if (result == null)
                    return BadRequest(ApiResponseDto<RegularizationResponseDto>.ErrorResponse(
                        "Failed to create regularization request. Employee not found."));

                return Ok(ApiResponseDto<RegularizationResponseDto>.SuccessResponse(
                    result, "Regularization request submitted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseDto<RegularizationResponseDto>.ErrorResponse(ex.Message));
            }
        }

        // ── PATCH api/AttendanceRegularization/{id}/approve ────────────────────
        // SuperAdmin added so the 403 Forbidden is fixed
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<ActionResult<ApiResponseDto<RegularizationResponseDto>>> ApproveRegularization(
            string id,
            [FromBody] RegularizationApprovalDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<RegularizationResponseDto>.ErrorResponse("User not authenticated"));

            try
            {
                var result = await _regularizationService.ApproveRegularizationAsync(id, dto, userId);
                if (result == null)
                    return NotFound(ApiResponseDto<RegularizationResponseDto>.ErrorResponse(
                        "Regularization request not found"));

                var message = dto.IsApproved ? "Regularization approved successfully" : "Regularization rejected";
                return Ok(ApiResponseDto<RegularizationResponseDto>.SuccessResponse(result, message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseDto<RegularizationResponseDto>.ErrorResponse(ex.Message));
            }
        }

        // ── GET api/AttendanceRegularization/{id} ──────────────────────────────
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<RegularizationResponseDto>>> GetById(string id)
        {
            var result = await _regularizationService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponseDto<RegularizationResponseDto>.ErrorResponse(
                    "Regularization request not found"));

            return Ok(ApiResponseDto<RegularizationResponseDto>.SuccessResponse(result));
        }

        // ── GET api/AttendanceRegularization/employee/{employeeId} ─────────────
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<ApiResponseDto<List<RegularizationResponseDto>>>> GetByEmployeeId(
            string employeeId)
        {
            var result = await _regularizationService.GetByEmployeeIdAsync(employeeId);
            return Ok(ApiResponseDto<List<RegularizationResponseDto>>.SuccessResponse(result));
        }

        // ── POST api/AttendanceRegularization/filter ───────────────────────────
        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<RegularizationResponseDto>>>> GetFiltered(
            [FromBody] RegularizationFilterDto filter)
        {
            var result = await _regularizationService.GetFilteredAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<RegularizationResponseDto>>.SuccessResponse(result));
        }

        // ── GET api/AttendanceRegularization/pending ───────────────────────────
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<ActionResult<ApiResponseDto<List<RegularizationResponseDto>>>> GetPending()
        {
            var result = await _regularizationService.GetPendingRegularizationsAsync();
            return Ok(ApiResponseDto<List<RegularizationResponseDto>>.SuccessResponse(result));
        }

        // ── GET api/AttendanceRegularization/pending-count/{employeeId} ────────
        [HttpGet("pending-count/{employeeId}")]
        public async Task<ActionResult<ApiResponseDto<int>>> GetPendingCount(string employeeId)
        {
            var count = await _regularizationService.GetPendingCountByEmployeeAsync(employeeId);
            return Ok(ApiResponseDto<int>.SuccessResponse(count));
        }

        // ── PATCH api/AttendanceRegularization/{id}/cancel ─────────────────────
        [HttpPatch("{id}/cancel")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CancelRegularization(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _regularizationService.CancelRegularizationAsync(id, userId);
            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse(
                    "Regularization request not found or cannot be cancelled"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Regularization request cancelled successfully"));
        }

        // ── DELETE api/AttendanceRegularization/{id} ───────────────────────────
        // Soft-deletes the record (sets IsDeleted = true in MongoDB).
        // Available to all authenticated users so employees can delete their own requests.
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteRegularization(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _regularizationService.DeleteRegularizationAsync(id, userId);
            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse(
                    "Regularization request not found or already deleted"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Regularization request deleted successfully"));
        }
    }
}