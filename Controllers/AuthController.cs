using System.Security.Claims;
using AttendanceManagementSystem.Models.DTOs.Auth;
using AttendanceManagementSystem.Models.DTOs.Auth.Forgotpassword;
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

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized(ApiResponseDto<LoginResponseDto>.ErrorResponse("Invalid username or password"));

            return Ok(ApiResponseDto<LoginResponseDto>.SuccessResponse(result, "Login successful"));
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);

                if (result == null)
                    return BadRequest(ApiResponseDto<LoginResponseDto>.ErrorResponse("Registration failed"));

                return Ok(ApiResponseDto<LoginResponseDto>.SuccessResponse(result, "Registration successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseDto<LoginResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            if (result == null)
                return Unauthorized(ApiResponseDto<LoginResponseDto>.ErrorResponse("Invalid or expired refresh token"));

            return Ok(ApiResponseDto<LoginResponseDto>.SuccessResponse(result, "Token refreshed successfully"));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<bool>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponseDto<bool>.ErrorResponse("User not authenticated"));

            var result = await _authService.ChangePasswordAsync(userId, request);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to change password"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Password changed successfully"));
        }

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

        [HttpGet("me")]
        [Authorize]
        public ActionResult<ApiResponseDto<object>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var firstName = User.FindFirst("FirstName")?.Value;
            var lastName = User.FindFirst("LastName")?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var userData = new
            {
                Id = userId,
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Roles = roles
            };

            return Ok(ApiResponseDto<object>.SuccessResponse(userData, "User information retrieved"));
        }

        // Forgot Password Endpoints
        [HttpPost("forgot-password/request-otp")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<bool>>> RequestPasswordResetOtp([FromBody] ForgotPasswordRequestDto request)
        {
            var result = await _authService.RequestPasswordResetOtpAsync(request.Email);

            // Always return success to prevent email enumeration
            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "If the email exists, an OTP has been sent"));
        }


        [HttpPost("forgot-password/verify-otp")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<bool>>> VerifyPasswordResetOtp([FromBody] VerifyOtpRequestDto request)
        {
            var result = await _authService.VerifyPasswordResetOtpAsync(request.Email, request.Otp);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Invalid or expired OTP"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "OTP verified successfully"));
        }

        [HttpPost("forgot-password/reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<bool>>> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var result = await _authService.ResetPasswordAsync(request.Email, request.Otp, request.NewPassword);

            if (!result)
                return BadRequest(ApiResponseDto<bool>.ErrorResponse("Failed to reset password. Invalid or expired OTP"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Password reset successfully"));
        }
    }
}