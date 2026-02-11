using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.LeaveType;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveTypeController : ControllerBase
    {
        private readonly ILeaveTypeService _leaveTypeService;

        public LeaveTypeController(ILeaveTypeService leaveTypeService)
        {
            _leaveTypeService = leaveTypeService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<LeaveTypeResponseDto>>> CreateLeaveType([FromBody] CreateLeaveTypeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<LeaveTypeResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _leaveTypeService.CreateLeaveTypeAsync(dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<LeaveTypeResponseDto>.ErrorResponse("Leave type code already exists"));

            return Ok(ApiResponseDto<LeaveTypeResponseDto>.SuccessResponse(result, "Leave type created successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<LeaveTypeResponseDto>>> GetLeaveTypeById(string id)
        {
            var result = await _leaveTypeService.GetLeaveTypeByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponseDto<LeaveTypeResponseDto>.ErrorResponse("Leave type not found"));

            return Ok(ApiResponseDto<LeaveTypeResponseDto>.SuccessResponse(result));
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<ApiResponseDto<LeaveTypeResponseDto>>> GetLeaveTypeByCode(string code)
        {
            var result = await _leaveTypeService.GetLeaveTypeByCodeAsync(code);

            if (result == null)
                return NotFound(ApiResponseDto<LeaveTypeResponseDto>.ErrorResponse("Leave type not found"));

            return Ok(ApiResponseDto<LeaveTypeResponseDto>.SuccessResponse(result));
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<LeaveTypeResponseDto>>>> GetFilteredLeaveTypes([FromBody] LeaveTypeFilterDto filter)
        {
            var result = await _leaveTypeService.GetFilteredLeaveTypesAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<LeaveTypeResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponseDto<List<LeaveTypeResponseDto>>>> GetActiveLeaveTypes()
        {
            var result = await _leaveTypeService.GetActiveLeaveTypesAsync();
            return Ok(ApiResponseDto<List<LeaveTypeResponseDto>>.SuccessResponse(result));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<LeaveTypeResponseDto>>> UpdateLeaveType(string id, [FromBody] UpdateLeaveTypeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<LeaveTypeResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _leaveTypeService.UpdateLeaveTypeAsync(id, dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<LeaveTypeResponseDto>.ErrorResponse("Failed to update leave type or code already exists"));

            return Ok(ApiResponseDto<LeaveTypeResponseDto>.SuccessResponse(result, "Leave type updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteLeaveType(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveTypeService.DeleteLeaveTypeAsync(id, userId);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Leave type not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave type deleted successfully"));
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ToggleLeaveTypeStatus(string id, [FromBody] ToggleStatusRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _leaveTypeService.ToggleLeaveTypeStatusAsync(id, request.IsActive, userId);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Leave type not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Leave type status updated successfully"));
        }
    }

    public class ToggleStatusRequestDto
    {
        public bool IsActive { get; set; }
    }
}