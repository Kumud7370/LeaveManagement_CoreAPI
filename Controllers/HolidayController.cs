using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Holiday;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayService _holidayService;

        public HolidayController(IHolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<HolidayResponseDto>>> CreateHoliday(
            [FromBody] CreateHolidayDto dto)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(ApiResponseDto<HolidayResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _holidayService.CreateHolidayAsync(dto, userId);

            if (result == null)
                return Conflict(ApiResponseDto<HolidayResponseDto>.ErrorResponse(
                    "A holiday with the same name already exists on this date."));

            return CreatedAtAction(nameof(GetHolidayById), new { id = result.Id },
                ApiResponseDto<HolidayResponseDto>.SuccessResponse(result, "Holiday created successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<HolidayResponseDto>>> GetHolidayById(string id)
        {
            var result = await _holidayService.GetHolidayByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponseDto<HolidayResponseDto>.ErrorResponse("Holiday not found"));

            return Ok(ApiResponseDto<HolidayResponseDto>.SuccessResponse(result));
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<HolidayResponseDto>>>> GetFilteredHolidays(
            [FromBody] HolidayFilterDto filter)
        {
            var result = await _holidayService.GetFilteredHolidaysAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetHolidaysByDepartment(
            string departmentId)
        {
            var result = await _holidayService.GetHolidaysByDepartmentAsync(departmentId);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("date-range")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetHolidaysByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest(ApiResponseDto<List<HolidayResponseDto>>.ErrorResponse(
                    "startDate must be before or equal to endDate"));

            var result = await _holidayService.GetHolidaysByDateRangeAsync(startDate, endDate);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetUpcomingHolidays(
            [FromQuery] int count = 10)
        {
            if (count < 1 || count > 100)
                return BadRequest(ApiResponseDto<List<HolidayResponseDto>>.ErrorResponse(
                    "count must be between 1 and 100"));

            var result = await _holidayService.GetUpcomingHolidaysAsync(count);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("year/{year:int}")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetHolidaysByYear(int year)
        {
            var result = await _holidayService.GetHolidaysByYearAsync(year);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("month/{year:int}/{month:int}")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetHolidaysByMonth(
            int year, int month)
        {
            if (month < 1 || month > 12)
                return BadRequest(ApiResponseDto<List<HolidayResponseDto>>.ErrorResponse(
                    "month must be between 1 and 12"));

            var result = await _holidayService.GetHolidaysByMonthAsync(year, month);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("type/{holidayType}")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetHolidaysByType(
            HolidayType holidayType)
        {
            var result = await _holidayService.GetHolidaysByTypeAsync(holidayType);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<HolidayResponseDto>>> UpdateHoliday(
            string id, [FromBody] UpdateHolidayDto dto)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(ApiResponseDto<HolidayResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _holidayService.UpdateHolidayAsync(id, dto, userId);
            if (result == null)
                return Conflict(ApiResponseDto<HolidayResponseDto>.ErrorResponse(
                    "Holiday not found, or another holiday with the same name already exists on that date."));

            return Ok(ApiResponseDto<HolidayResponseDto>.SuccessResponse(result, "Holiday updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteHoliday(string id)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _holidayService.DeleteHolidayAsync(id, userId);
            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Holiday not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Holiday deleted successfully"));
        }

        [HttpGet("check-date")]
        public async Task<ActionResult<ApiResponseDto<bool>>> IsHolidayOnDate([FromQuery] DateTime date)
        {
            var result = await _holidayService.IsHolidayOnDateAsync(date);
            return Ok(ApiResponseDto<bool>.SuccessResponse(result));
        }

        [HttpGet("statistics/type")]
        public async Task<ActionResult<ApiResponseDto<Dictionary<string, int>>>> GetHolidayStatisticsByType()
        {
            var result = await _holidayService.GetHolidayStatisticsByTypeAsync();
            return Ok(ApiResponseDto<Dictionary<string, int>>.SuccessResponse(result));
        }
        private string? GetUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value is { Length: > 0 } v ? v : null;
    }
}