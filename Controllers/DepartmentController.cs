using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Department;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<DepartmentResponseDto>>> CreateDepartment([FromBody] CreateDepartmentRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized(ApiResponseDto<DepartmentResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _departmentService.CreateDepartmentAsync(dto, userGuid);

            if (result == null)
                return BadRequest(ApiResponseDto<DepartmentResponseDto>.ErrorResponse("Department code already exists or parent department/head not found"));

            return Ok(ApiResponseDto<DepartmentResponseDto>.SuccessResponse(result, "Department created successfully"));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponseDto<DepartmentResponseDto>>> GetDepartmentById(Guid id)
        {
            var result = await _departmentService.GetDepartmentByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponseDto<DepartmentResponseDto>.ErrorResponse("Department not found"));

            return Ok(ApiResponseDto<DepartmentResponseDto>.SuccessResponse(result));
        }

        [HttpGet("code/{departmentCode}")]
        public async Task<ActionResult<ApiResponseDto<DepartmentResponseDto>>> GetDepartmentByCode(string departmentCode)
        {
            var result = await _departmentService.GetDepartmentByCodeAsync(departmentCode);

            if (result == null)
                return NotFound(ApiResponseDto<DepartmentResponseDto>.ErrorResponse("Department not found"));

            return Ok(ApiResponseDto<DepartmentResponseDto>.SuccessResponse(result));
        }

        [HttpGet("{id:guid}/details")]
        public async Task<ActionResult<ApiResponseDto<DepartmentDetailResponseDto>>> GetDepartmentDetails(Guid id)
        {
            var result = await _departmentService.GetDepartmentDetailsAsync(id);

            if (result == null)
                return NotFound(ApiResponseDto<DepartmentDetailResponseDto>.ErrorResponse("Department not found"));

            return Ok(ApiResponseDto<DepartmentDetailResponseDto>.SuccessResponse(result));
        }

        [HttpPost("filter")]
        public async Task<ActionResult<ApiResponseDto<PaginatedResponseDto<DepartmentResponseDto>>>> GetFilteredDepartments([FromBody] DepartmentFilterRequestDto filter)
        {
            var result = await _departmentService.GetFilteredDepartmentsAsync(filter);
            return Ok(ApiResponseDto<PaginatedResponseDto<DepartmentResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("{parentId:guid}/children")]
        public async Task<ActionResult<ApiResponseDto<List<DepartmentResponseDto>>>> GetChildDepartments(Guid parentId)
        {
            var result = await _departmentService.GetChildDepartmentsAsync(parentId);
            return Ok(ApiResponseDto<List<DepartmentResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("root")]
        public async Task<ActionResult<ApiResponseDto<List<DepartmentResponseDto>>>> GetRootDepartments()
        {
            var result = await _departmentService.GetRootDepartmentsAsync();
            return Ok(ApiResponseDto<List<DepartmentResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponseDto<List<DepartmentResponseDto>>>> GetActiveDepartments()
        {
            var result = await _departmentService.GetActiveDepartmentsAsync();
            return Ok(ApiResponseDto<List<DepartmentResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("hierarchy")]
        public async Task<ActionResult<ApiResponseDto<List<DepartmentHierarchyDto>>>> GetDepartmentHierarchy()
        {
            var result = await _departmentService.GetDepartmentHierarchyAsync();
            return Ok(ApiResponseDto<List<DepartmentHierarchyDto>>.SuccessResponse(result));
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponseDto<DepartmentResponseDto>>> UpdateDepartment([FromBody] UpdateDepartmentRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized(ApiResponseDto<DepartmentResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _departmentService.UpdateDepartmentAsync(dto, userGuid);

            if (result == null)
                return BadRequest(ApiResponseDto<DepartmentResponseDto>.ErrorResponse("Failed to update department. Check if department code is unique and parent department exists."));

            return Ok(ApiResponseDto<DepartmentResponseDto>.SuccessResponse(result, "Department updated successfully"));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteDepartment(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var canDelete = await _departmentService.CanDeleteDepartmentAsync(id);
            if (!canDelete)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Cannot delete department. It has employees or child departments."));

            var result = await _departmentService.DeleteDepartmentAsync(id, userGuid);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Department not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Department deleted successfully"));
        }

        [HttpPatch("{id:guid}/toggle-status")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ToggleDepartmentStatus(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _departmentService.ToggleDepartmentStatusAsync(id, userGuid);

            if (!result)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Department not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Department status updated successfully"));
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponseDto<Dictionary<string, int>>>> GetDepartmentStatistics()
        {
            var result = await _departmentService.GetDepartmentStatisticsAsync();
            return Ok(ApiResponseDto<Dictionary<string, int>>.SuccessResponse(result));
        }

        [HttpGet("{id:guid}/can-delete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CanDeleteDepartment(Guid id)
        {
            var result = await _departmentService.CanDeleteDepartmentAsync(id);
            return Ok(ApiResponseDto<bool>.SuccessResponse(result));
        }
    }
}