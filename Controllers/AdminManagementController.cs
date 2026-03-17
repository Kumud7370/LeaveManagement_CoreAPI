using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.AdminManagement;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/admin-management")]
    [Authorize(Roles = "Admin")]   // Only Admin can manage users
    public class AdminManagementController : ControllerBase
    {
        private readonly IAdminManagementService _adminManagementService;

        public AdminManagementController(IAdminManagementService adminManagementService)
        {
            _adminManagementService = adminManagementService;
        }

        // ─────────────────────────────────────────────
        //  CREATE USER  (Tehsildar, NayabTehsildar, Employee)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Admin directly creates a user account with a username and password.
        /// Role must be one of: Tehsildar, NayabTehsildar, Employee.
        /// For Employee role, EmployeeId (of an existing employee record) is required.
        /// </summary>
        [HttpPost("users")]
        public async Task<ActionResult<ApiResponseDto<CreateUserResponseDto>>> CreateUser(
            [FromBody] CreateUserDto dto)
        {
            var (adminId, adminName, ipAddress) = GetCallerInfo();
            if (adminId == null)
                return Unauthorized(ApiResponseDto<CreateUserResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _adminManagementService.CreateUserAsync(dto, adminId, adminName!, ipAddress);

            if (result == null)
                return BadRequest(ApiResponseDto<CreateUserResponseDto>.ErrorResponse(
                    "Failed to create user. Username or email may already exist, " +
                    "the role is invalid, or the employee record is not found / already linked."));

            return Ok(ApiResponseDto<CreateUserResponseDto>.SuccessResponse(result, "User created successfully"));
        }

        // ─────────────────────────────────────────────
        //  UPDATE USER
        // ─────────────────────────────────────────────

        [HttpPut("users/{id}")]
        public async Task<ActionResult<ApiResponseDto<CreateUserResponseDto>>> UpdateUser(
            string id,
            [FromBody] UpdateUserDto dto)
        {
            var (adminId, _, ipAddress) = GetCallerInfo();
            if (adminId == null)
                return Unauthorized(ApiResponseDto<CreateUserResponseDto>.ErrorResponse("User not authenticated"));

            var result = await _adminManagementService.UpdateUserAsync(id, dto, adminId, ipAddress);

            if (result == null)
                return BadRequest(ApiResponseDto<CreateUserResponseDto>.ErrorResponse(
                    "Failed to update user. User may not exist or email is already taken."));

            return Ok(ApiResponseDto<CreateUserResponseDto>.SuccessResponse(result, "User updated successfully"));
        }

        // ─────────────────────────────────────────────
        //  ACTIVATE / DEACTIVATE USER
        // ─────────────────────────────────────────────

        [HttpPatch("users/{id}/status")]
        public async Task<ActionResult<ApiResponseDto<bool>>> SetUserStatus(
            string id,
            [FromBody] bool isActive)
        {
            var (adminId, _, ipAddress) = GetCallerInfo();
            if (adminId == null)
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _adminManagementService.SetUserActiveStatusAsync(id, isActive, adminId, ipAddress);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to update user status. User may not exist."));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true,
                isActive ? "User activated successfully" : "User deactivated successfully"));
        }

        // ─────────────────────────────────────────────
        //  DELETE USER
        // ─────────────────────────────────────────────

        [HttpDelete("users/{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteUser(string id)
        {
            var (adminId, _, ipAddress) = GetCallerInfo();
            if (adminId == null)
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _adminManagementService.DeleteUserAsync(id, adminId, ipAddress);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to delete user. User may not exist."));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "User deleted successfully"));
        }

        // ─────────────────────────────────────────────
        //  GET ALL USERS
        // ─────────────────────────────────────────────

        [HttpGet("users")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<CreateUserResponseDto>>>> GetAllUsers()
        {
            var result = await _adminManagementService.GetAllUsersAsync();
            return Ok(ApiResponseDto<IEnumerable<CreateUserResponseDto>>.SuccessResponse(
                result, "Users retrieved successfully"));
        }

        // ─────────────────────────────────────────────
        //  GET USER BY ID
        // ─────────────────────────────────────────────

        [HttpGet("users/{id}")]
        public async Task<ActionResult<ApiResponseDto<CreateUserResponseDto>>> GetUserById(string id)
        {
            var result = await _adminManagementService.GetUserByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponseDto<CreateUserResponseDto>.ErrorResponse("User not found"));

            return Ok(ApiResponseDto<CreateUserResponseDto>.SuccessResponse(result));
        }

        // ─────────────────────────────────────────────
        //  ADMIN RESET PASSWORD FOR ANY USER
        // ─────────────────────────────────────────────

        /// <summary>
        /// Admin can reset the password of any user directly.
        /// </summary>
        [HttpPost("users/{id}/reset-password")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ResetUserPassword(
            string id,
            [FromBody] AdminResetPasswordDto dto)
        {
            var (adminId, _, ipAddress) = GetCallerInfo();
            if (adminId == null)
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _adminManagementService.ResetUserPasswordAsync(id, dto.NewPassword, adminId, ipAddress);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to reset password. User may not exist."));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Password reset successfully"));
        }

        // ─────────────────────────────────────────────
        //  HELPER
        // ─────────────────────────────────────────────

        private (string? id, string? name, string? ip) GetCallerInfo()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            return (id, name, ip);
        }
    }
}