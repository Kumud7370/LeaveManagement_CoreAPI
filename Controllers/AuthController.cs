using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Auth;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ─────────────────────────────────────────────
        //  LOGIN  (all roles)
        // ─────────────────────────────────────────────

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login(
            [FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized(ApiResponseDto<LoginResponseDto>.ErrorResponse(
                    "Invalid username or password"));

            return Ok(ApiResponseDto<LoginResponseDto>.SuccessResponse(result, "Login successful"));
        }

        // ─────────────────────────────────────────────
        //  REFRESH TOKEN
        // ─────────────────────────────────────────────

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> RefreshToken(
            [FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            if (result == null)
                return Unauthorized(ApiResponseDto<LoginResponseDto>.ErrorResponse(
                    "Invalid or expired refresh token"));

            return Ok(ApiResponseDto<LoginResponseDto>.SuccessResponse(result, "Token refreshed successfully"));
        }

        // ─────────────────────────────────────────────
        //  CHANGE PASSWORD  (any authenticated user)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Every role (Admin, Tehsildar, NayabTehsildar, Employee) can change
        /// their own password by providing the current password.
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<bool>>> ChangePassword(
            [FromBody] ChangePasswordRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _authService.ChangePasswordAsync(userId, request);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse(
                    "Failed to change password. Current password may be incorrect or passwords do not match."));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Password changed successfully"));
        }

        // ─────────────────────────────────────────────
        //  LOGOUT
        // ─────────────────────────────────────────────

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<bool>>> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _authService.LogoutAsync(userId);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to logout"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Logout successful"));
        }

        // ─────────────────────────────────────────────
        //  GET CURRENT USER INFO
        // ─────────────────────────────────────────────

        [HttpGet("me")]
        [Authorize]
        public ActionResult<ApiResponseDto<object>> GetCurrentUser()
        {
            var userData = new
            {
                Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Username = User.FindFirst(ClaimTypes.Name)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                FirstName = User.FindFirst("FirstName")?.Value,
                LastName = User.FindFirst("LastName")?.Value,
                Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            };

            return Ok(ApiResponseDto<object>.SuccessResponse(userData, "User information retrieved"));
        }
    }
}