using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Employee;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // ── Create ────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<EmployeeResponseDto>>> CreateEmployee(
            [FromBody] CreateEmployeeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _employeeService.CreateEmployeeAsync(dto, userId);
            if (result == null)
                return BadRequest(ApiResponseDto<EmployeeResponseDto>.ErrorResponse(
                    "Employee code or email already exists"));

            return Ok(ApiResponseDto<EmployeeResponseDto>.SuccessResponse(result, "Employee created successfully"));
        }

        // ── Update ────────────────────────────────────────────────────────────
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<EmployeeResponseDto>>> UpdateEmployee(
            string id, [FromBody] UpdateEmployeeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _employeeService.UpdateEmployeeAsync(id, dto, userId);
            if (result == null)
                return BadRequest(ApiResponseDto<EmployeeResponseDto>.ErrorResponse(
                    "Failed to update employee or email already exists"));

            return Ok(ApiResponseDto<EmployeeResponseDto>.SuccessResponse(result, "Employee updated successfully"));
        }

        // ── Delete ────────────────────────────────────────────────────────────
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteEmployee(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _employeeService.DeleteEmployeeAsync(id, userId);
            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Employee not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Employee deleted successfully"));
        }

        // ── Status ────────────────────────────────────────────────────────────
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ChangeEmployeeStatus(
            string id, [FromBody] EmployeeStatus status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _employeeService.ChangeEmployeeStatusAsync(id, status, userId);
            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Employee not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Employee status updated successfully"));
        }

        // ── Get by ID ─────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Tehsildar,NayabTehsildar")]
        public async Task<ActionResult<ApiResponseDto<EmployeeResponseDto>>> GetEmployeeById(string id)
        {
            var result = await _employeeService.GetEmployeeByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("Employee not found"));

            return Ok(ApiResponseDto<EmployeeResponseDto>.SuccessResponse(result));
        }

        // ── Get by Code ───────────────────────────────────────────────────────
        [HttpGet("code/{employeeCode}")]
        [Authorize(Roles = "Admin,Tehsildar,NayabTehsildar")]
        public async Task<ActionResult<ApiResponseDto<EmployeeResponseDto>>> GetEmployeeByCode(string employeeCode)
        {
            var result = await _employeeService.GetEmployeeByCodeAsync(employeeCode);
            if (result == null)
                return NotFound(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("Employee not found"));

            return Ok(ApiResponseDto<EmployeeResponseDto>.SuccessResponse(result));
        }

        // ── Get by Email ──────────────────────────────────────────────────────
        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin,Tehsildar,NayabTehsildar")]
        public async Task<ActionResult<ApiResponseDto<EmployeeResponseDto>>> GetEmployeeByEmail(string email)
        {
            var result = await _employeeService.GetEmployeeByEmailAsync(email);
            if (result == null)
                return NotFound(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("Employee not found"));

            return Ok(ApiResponseDto<EmployeeResponseDto>.SuccessResponse(result));
        }

        // ── Filter ────────────────────────────────────────────────────────────
        [HttpPost("filter")]
        [Authorize(Roles = "Admin,Tehsildar,NayabTehsildar")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<EmployeeResponseDto>>>> GetFilteredEmployees(
            [FromBody] EmployeeFilterDto filter)
        {
            var result = await _employeeService.GetFilteredEmployeesAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<EmployeeResponseDto>>.SuccessResponse(result));
        }

        // ── By Department ─────────────────────────────────────────────────────
        [HttpGet("department/{departmentId}")]
        [Authorize(Roles = "Admin,Tehsildar,NayabTehsildar")]
        public async Task<ActionResult<ApiResponseDto<List<EmployeeResponseDto>>>> GetEmployeesByDepartment(
            string departmentId)
        {
            var result = await _employeeService.GetEmployeesByDepartmentAsync(departmentId);
            return Ok(ApiResponseDto<List<EmployeeResponseDto>>.SuccessResponse(result));
        }

        // ── Single Reassign ───────────────────────────────────────────────────
        [HttpPost("{id}/reassign")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<EmployeeResponseDto>>> ReassignEmployee(
            string id, [FromBody] ReassignEmployeeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _employeeService.ReassignEmployeeAsync(id, dto, userId);
            if (result == null)
                return BadRequest(ApiResponseDto<EmployeeResponseDto>.ErrorResponse(
                    "Failed to reassign. Employee, department, or designation not found."));

            return Ok(ApiResponseDto<EmployeeResponseDto>.SuccessResponse(result, "Employee reassigned successfully"));
        }

        // ── Bulk Reassign ─────────────────────────────────────────────────────
    
        [HttpPost("bulk-reassign")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<BulkReassignResultDto>>> BulkReassignEmployees(
            [FromBody] BulkReassignEmployeeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<BulkReassignResultDto>.ErrorResponse("User not authenticated"));

            if (dto.EmployeeIds == null || dto.EmployeeIds.Count == 0)
                return BadRequest(ApiResponseDto<BulkReassignResultDto>.ErrorResponse(
                    "At least one employee ID is required."));

            if (string.IsNullOrEmpty(dto.ToDepartmentId) || string.IsNullOrEmpty(dto.ToDesignationId))
                return BadRequest(ApiResponseDto<BulkReassignResultDto>.ErrorResponse(
                    "ToDepartmentId and ToDesignationId are required."));

            var result = await _employeeService.BulkReassignEmployeesAsync(dto, userId);

            // Return 207 Multi-Status when some succeeded and some failed
            if (result.Failed > 0 && result.Succeeded > 0)
                return StatusCode(207, ApiResponseDto<BulkReassignResultDto>.SuccessResponse(
                    result, result.Message));

            if (result.Succeeded == 0)
                return BadRequest(ApiResponseDto<BulkReassignResultDto>.ErrorResponse(
                    result.Message));

            return Ok(ApiResponseDto<BulkReassignResultDto>.SuccessResponse(result, result.Message));
        }

        // ── Assignment History ─────────────────────────────────────────────────
        [HttpGet("{id}/assignment-history")]
        [Authorize(Roles = "Admin,Tehsildar,NayabTehsildar")]
        public async Task<ActionResult<ApiResponseDto<List<AssignmentHistoryResponseDto>>>> GetAssignmentHistory(string id)
        {
            var result = await _employeeService.GetAssignmentHistoryAsync(id);
            return Ok(ApiResponseDto<List<AssignmentHistoryResponseDto>>.SuccessResponse(result));
        }

        // ── By Manager ────────────────────────────────────────────────────────
        [HttpGet("manager/{managerId}")]
        [Authorize(Roles = "Admin,Tehsildar,NayabTehsildar")]
        public async Task<ActionResult<ApiResponseDto<List<EmployeeResponseDto>>>> GetEmployeesByManager(
            string managerId)
        {
            var result = await _employeeService.GetEmployeesByManagerAsync(managerId);
            return Ok(ApiResponseDto<List<EmployeeResponseDto>>.SuccessResponse(result));
        }

        // ── Active ────────────────────────────────────────────────────────────
        [HttpGet("active")]
        [Authorize(Roles = "Admin,Tehsildar,NayabTehsildar")]
        public async Task<ActionResult<ApiResponseDto<List<EmployeeResponseDto>>>> GetActiveEmployees()
        {
            var result = await _employeeService.GetActiveEmployeesAsync();
            return Ok(ApiResponseDto<List<EmployeeResponseDto>>.SuccessResponse(result));
        }

        // ── Statistics ────────────────────────────────────────────────────────
        [HttpGet("statistics/status")]
        [Authorize(Roles = "Admin,Tehsildar,NayabTehsildar")]
        public async Task<ActionResult<ApiResponseDto<Dictionary<string, int>>>> GetEmployeeStatisticsByStatus()
        {
            var result = await _employeeService.GetEmployeeStatisticsByStatusAsync();
            return Ok(ApiResponseDto<Dictionary<string, int>>.SuccessResponse(result));
        }
    }
}