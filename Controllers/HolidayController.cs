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
        public async Task<ActionResult<ApiResponseDto<HolidayResponseDto>>> CreateHoliday([FromBody] CreateHolidayDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<HolidayResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _holidayService.CreateHolidayAsync(dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<HolidayResponseDto>.ErrorResponse("Holiday already exists on this date"));

            return Ok(ApiResponseDto<HolidayResponseDto>.SuccessResponse(result, "Holiday created successfully"));
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
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<HolidayResponseDto>>>> GetFilteredHolidays([FromBody] HolidayFilterDto filter)
        {
            var result = await _holidayService.GetFilteredHolidaysAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetHolidaysByDepartment(string departmentId)
        {
            var result = await _holidayService.GetHolidaysByDepartmentAsync(departmentId);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("date-range")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetHolidaysByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var result = await _holidayService.GetHolidaysByDateRangeAsync(startDate, endDate);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetUpcomingHolidays([FromQuery] int count = 10)
        {
            var result = await _holidayService.GetUpcomingHolidaysAsync(count);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("year/{year}")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetHolidaysByYear(int year)
        {
            var result = await _holidayService.GetHolidaysByYearAsync(year);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("month/{year}/{month}")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetHolidaysByMonth(int year, int month)
        {
            var result = await _holidayService.GetHolidaysByMonthAsync(year, month);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("type/{holidayType}")]
        public async Task<ActionResult<ApiResponseDto<List<HolidayResponseDto>>>> GetHolidaysByType(HolidayType holidayType)
        {
            var result = await _holidayService.GetHolidaysByTypeAsync(holidayType);
            return Ok(ApiResponseDto<List<HolidayResponseDto>>.SuccessResponse(result));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<HolidayResponseDto>>> UpdateHoliday(string id, [FromBody] UpdateHolidayDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<HolidayResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _holidayService.UpdateHolidayAsync(id, dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<HolidayResponseDto>.ErrorResponse("Failed to update holiday or holiday already exists"));

            return Ok(ApiResponseDto<HolidayResponseDto>.SuccessResponse(result, "Holiday updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteHoliday(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
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
    }
}