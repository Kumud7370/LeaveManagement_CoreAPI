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
    public class AdminManagementController : ControllerBase
    {
        private readonly IAdminManagementService _adminManagementService;

        public AdminManagementController(IAdminManagementService adminManagementService)
        {
            _adminManagementService = adminManagementService;
        }

        [HttpPost("invitations/send")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<ActionResult<ApiResponseDto<InvitationResponseDto>>> SendInvitation([FromBody] SendInvitationDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
            {
                return Unauthorized(ApiResponseDto<InvitationResponseDto>.ErrorResponse("User not authenticated"));
            }

            var result = await _adminManagementService.SendInvitationAsync(dto, userId, userName, ipAddress);

            if (result == null)
            {
                return BadRequest(ApiResponseDto<InvitationResponseDto>.ErrorResponse(
                    "Failed to send invitation. User may already exist or you don't have permission to invite this role."));
            }

            return Ok(ApiResponseDto<InvitationResponseDto>.SuccessResponse(result, "Invitation sent successfully"));
        }

        [HttpPut("invitations/{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<ActionResult<ApiResponseDto<InvitationResponseDto>>> UpdateInvitation(
            string id,
            [FromBody] EditInvitationDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponseDto<InvitationResponseDto>.ErrorResponse("User not authenticated"));
            }

            var result = await _adminManagementService.UpdateInvitationAsync(id, dto, userId, ipAddress);

            if (result == null)
            {
                return BadRequest(ApiResponseDto<InvitationResponseDto>.ErrorResponse(
                    "Failed to update invitation. Invitation may not exist or is not pending."));
            }

            return Ok(ApiResponseDto<InvitationResponseDto>.SuccessResponse(result, "Invitation updated successfully"));
        }

        [HttpPost("invitations/{id}/revoke")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<ActionResult<ApiResponseDto<bool>>> RevokeInvitation(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));
            }

            var result = await _adminManagementService.RevokeInvitationAsync(id, userId, ipAddress);

            if (!result)
            {
                return BadRequest(ApiResponseDto<bool>.ErrorResponse(
                    "Failed to revoke invitation. Invitation may not exist or is not pending."));
            }

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Invitation revoked successfully"));
        }

        [HttpDelete("invitations/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteInvitation(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));
            }

            var result = await _adminManagementService.DeleteInvitationAsync(id, userId, ipAddress);

            if (!result)
            {
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to delete invitation"));
            }

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Invitation deleted successfully"));
        }

        [HttpGet("invitations")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<InvitationResponseDto>>>> GetAllInvitations()
        {
            var invitations = await _adminManagementService.GetAllInvitationsAsync();
            return Ok(ApiResponseDto<IEnumerable<InvitationResponseDto>>.SuccessResponse(
                invitations,
                "Invitations retrieved successfully"));
        }

        [HttpGet("invitations/my-invitations")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<InvitationResponseDto>>>> GetMyInvitations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponseDto<IEnumerable<InvitationResponseDto>>.ErrorResponse("User not authenticated"));
            }

            var invitations = await _adminManagementService.GetMyInvitationsAsync(userId);
            return Ok(ApiResponseDto<IEnumerable<InvitationResponseDto>>.SuccessResponse(
                invitations,
                "Your invitations retrieved successfully"));
        }

        [HttpGet("invitations/validate/{token}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<InvitationResponseDto>>> ValidateToken(string token)
        {
            var result = await _adminManagementService.ValidateTokenAsync(token);

            if (result == null)
            {
                return BadRequest(ApiResponseDto<InvitationResponseDto>.ErrorResponse(
                    "Invalid or expired invitation token"));
            }

            return Ok(ApiResponseDto<InvitationResponseDto>.SuccessResponse(result, "Token is valid"));
        }

        [HttpPost("invitations/accept")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<bool>>> AcceptInvitation([FromBody] AcceptInvitationDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _adminManagementService.AcceptInvitationAsync(dto, ipAddress);

            if (!result)
            {
                return BadRequest(ApiResponseDto<bool>.ErrorResponse(
                    "Failed to accept invitation. Token may be invalid, expired, or username already exists."));
            }

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Invitation accepted successfully. You can now login."));
        }
    }
}