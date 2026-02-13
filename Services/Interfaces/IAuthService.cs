//using AttendanceManagementSystem.Models.DTOs.Auth;

//namespace AttendanceManagementSystem.Services.Interfaces
//{
//    public interface IAuthService
//    {
//        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
//        Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
//        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
//        Task<bool> LogoutAsync(string userId);
//    }
//}

using AttendanceManagementSystem.Models.DTOs.Auth;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
        Task<bool> LogoutAsync(string userId);

        // Registration
        Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto request);

        // Forgot Password
        Task<bool> RequestPasswordResetOtpAsync(string email);
        Task<bool> VerifyPasswordResetOtpAsync(string email, string otp);
        Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);
    }
}