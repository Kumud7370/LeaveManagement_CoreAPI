using AttendanceManagementSystem.Models.DTOs.Designation;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AttendanceManagementSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DesignationController : ControllerBase
    {
        private readonly IDesignationService _designationService;

        public DesignationController(IDesignationService designationService)
        {
            _designationService = designationService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDesignation([FromBody] CreateDesignationDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _designationService.CreateDesignationAsync(dto, userId);

            if (result == null)
                return BadRequest(new { message = "Designation code already exists" });

            return CreatedAtAction(nameof(GetDesignationById), new { id = result.Id }, new
            {
                message = "Designation created successfully",
                data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDesignationById(string id)
        {
            var result = await _designationService.GetDesignationByIdAsync(id);

            if (result == null)
                return NotFound(new { message = "Designation not found" });

            return Ok(new
            {
                message = "Designation retrieved successfully",
                data = result
            });
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetDesignationByCode(string code)
        {
            var result = await _designationService.GetDesignationByCodeAsync(code);

            if (result == null)
                return NotFound(new { message = "Designation not found" });

            return Ok(new
            {
                message = "Designation retrieved successfully",
                data = result
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredDesignations([FromQuery] DesignationFilterDto filter)
        {
            var result = await _designationService.GetFilteredDesignationsAsync(filter);

            return Ok(new
            {
                message = "Designations retrieved successfully",
                data = result.Items,
                pagination = new
                {
                    result.TotalCount,
                    result.PageNumber,
                    result.PageSize,
                    result.TotalPages
                }
            });
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveDesignations()
        {
            var result = await _designationService.GetActiveDesignationsAsync();

            return Ok(new
            {
                message = "Active designations retrieved successfully",
                data = result
            });
        }

        [HttpGet("level/{level}")]
        public async Task<IActionResult> GetDesignationsByLevel(int level)
        {
            var result = await _designationService.GetDesignationsByLevelAsync(level);

            return Ok(new
            {
                message = $"Designations for level {level} retrieved successfully",
                data = result
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDesignation(string id, [FromBody] UpdateDesignationDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _designationService.UpdateDesignationAsync(id, dto, userId);

            if (result == null)
                return BadRequest(new { message = "Designation not found or code already exists" });

            return Ok(new
            {
                message = "Designation updated successfully",
                data = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDesignation(string id)
        {
            var userId = GetCurrentUserId();
            var success = await _designationService.DeleteDesignationAsync(id, userId);

            if (!success)
                return BadRequest(new { message = "Designation not found or is assigned to employees" });

            return Ok(new { message = "Designation deleted successfully" });
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleDesignationStatus(string id)
        {
            var userId = GetCurrentUserId();
            var success = await _designationService.ToggleDesignationStatusAsync(id, userId);

            if (!success)
                return NotFound(new { message = "Designation not found" });

            return Ok(new { message = "Designation status toggled successfully" });
        }

        [HttpGet("statistics/by-level")]
        public async Task<IActionResult> GetDesignationStatisticsByLevel()
        {
            var result = await _designationService.GetDesignationStatisticsByLevelAsync();

            return Ok(new
            {
                message = "Designation statistics retrieved successfully",
                data = result
            });
        }
    }
}