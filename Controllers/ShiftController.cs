using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Shift;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShiftController : ControllerBase
    {
        private readonly IShiftService _shiftService;

        public ShiftController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<ShiftResponseDto>>> CreateShift([FromBody] CreateShiftDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<ShiftResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _shiftService.CreateShiftAsync(dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<ShiftResponseDto>.ErrorResponse("Shift code already exists"));

            return Ok(ApiResponseDto<ShiftResponseDto>.SuccessResponse(result, "Shift created successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<ShiftResponseDto>>> GetShiftById(string id)
        {
            var result = await _shiftService.GetShiftByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponseDto<ShiftResponseDto>.ErrorResponse("Shift not found"));

            return Ok(ApiResponseDto<ShiftResponseDto>.SuccessResponse(result));
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<ApiResponseDto<ShiftResponseDto>>> GetShiftByCode(string code)
        {
            var result = await _shiftService.GetShiftByCodeAsync(code);

            if (result == null)
                return NotFound(ApiResponseDto<ShiftResponseDto>.ErrorResponse("Shift not found"));

            return Ok(ApiResponseDto<ShiftResponseDto>.SuccessResponse(result));
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<ShiftResponseDto>>>> GetFilteredShifts([FromBody] ShiftFilterDto filter)
        {
            var result = await _shiftService.GetFilteredShiftsAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<ShiftResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponseDto<List<ShiftResponseDto>>>> GetActiveShifts()
        {
            var result = await _shiftService.GetActiveShiftsAsync();
            return Ok(ApiResponseDto<List<ShiftResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("night-shifts")]
        public async Task<ActionResult<ApiResponseDto<List<ShiftResponseDto>>>> GetNightShifts()
        {
            var result = await _shiftService.GetNightShiftsAsync();
            return Ok(ApiResponseDto<List<ShiftResponseDto>>.SuccessResponse(result));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<ShiftResponseDto>>> UpdateShift(string id, [FromBody] UpdateShiftDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<ShiftResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _shiftService.UpdateShiftAsync(id, dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<ShiftResponseDto>.ErrorResponse("Failed to update shift or code already exists"));

            return Ok(ApiResponseDto<ShiftResponseDto>.SuccessResponse(result, "Shift updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteShift(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _shiftService.DeleteShiftAsync(id, userId);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Shift not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Shift deleted successfully"));
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ToggleShiftStatus(string id, [FromBody] ToggleShiftStatusRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _shiftService.ToggleShiftStatusAsync(id, request.IsActive, userId);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Shift not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Shift status updated successfully"));
        }
    }

    public class ToggleShiftStatusRequestDto
    {
        public bool IsActive { get; set; }
    }
}