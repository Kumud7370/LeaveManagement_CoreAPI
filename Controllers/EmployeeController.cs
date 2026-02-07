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

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<EmployeeResponseDto>>> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _employeeService.CreateEmployeeAsync(dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("Employee code or email already exists"));

            return Ok(ApiResponseDto<EmployeeResponseDto>.SuccessResponse(result, "Employee created successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<EmployeeResponseDto>>> GetEmployeeById(string id)
        {
            var result = await _employeeService.GetEmployeeByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("Employee not found"));

            return Ok(ApiResponseDto<EmployeeResponseDto>.SuccessResponse(result));
        }

        [HttpGet("code/{employeeCode}")]
        public async Task<ActionResult<ApiResponseDto<EmployeeResponseDto>>> GetEmployeeByCode(string employeeCode)
        {
            var result = await _employeeService.GetEmployeeByCodeAsync(employeeCode);

            if (result == null)
                return NotFound(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("Employee not found"));

            return Ok(ApiResponseDto<EmployeeResponseDto>.SuccessResponse(result));
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<ApiResponseDto<EmployeeResponseDto>>> GetEmployeeByEmail(string email)
        {
            var result = await _employeeService.GetEmployeeByEmailAsync(email);

            if (result == null)
                return NotFound(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("Employee not found"));

            return Ok(ApiResponseDto<EmployeeResponseDto>.SuccessResponse(result));
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<EmployeeResponseDto>>>> GetFilteredEmployees([FromBody] EmployeeFilterDto filter)
        {
            var result = await _employeeService.GetFilteredEmployeesAsync(filter);
            return Ok(ApiResponseDto<PagedResultDto<EmployeeResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<ApiResponseDto<List<EmployeeResponseDto>>>> GetEmployeesByDepartment(string departmentId)
        {
            var result = await _employeeService.GetEmployeesByDepartmentAsync(departmentId);
            return Ok(ApiResponseDto<List<EmployeeResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("manager/{managerId}")]
        public async Task<ActionResult<ApiResponseDto<List<EmployeeResponseDto>>>> GetEmployeesByManager(string managerId)
        {
            var result = await _employeeService.GetEmployeesByManagerAsync(managerId);
            return Ok(ApiResponseDto<List<EmployeeResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponseDto<List<EmployeeResponseDto>>>> GetActiveEmployees()
        {
            var result = await _employeeService.GetActiveEmployeesAsync();
            return Ok(ApiResponseDto<List<EmployeeResponseDto>>.SuccessResponse(result));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<EmployeeResponseDto>>> UpdateEmployee(string id, [FromBody] UpdateEmployeeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _employeeService.UpdateEmployeeAsync(id, dto, userId);

            if (result == null)
                return BadRequest(ApiResponseDto<EmployeeResponseDto>.ErrorResponse("Failed to update employee or email already exists"));

            return Ok(ApiResponseDto<EmployeeResponseDto>.SuccessResponse(result, "Employee updated successfully"));
        }

        [HttpDelete("{id}")]
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

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ChangeEmployeeStatus(string id, [FromBody] EmployeeStatus status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _employeeService.ChangeEmployeeStatusAsync(id, status, userId);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Employee not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Employee status updated successfully"));
        }

        [HttpGet("statistics/status")]
        public async Task<ActionResult<ApiResponseDto<Dictionary<string, int>>>> GetEmployeeStatisticsByStatus()
        {
            var result = await _employeeService.GetEmployeeStatisticsByStatusAsync();
            return Ok(ApiResponseDto<Dictionary<string, int>>.SuccessResponse(result));
        }
    }
}