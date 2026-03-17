using AttendanceManagementSystem.Models.DTOs.Auth;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);

        /// <summary>
        /// Allows any authenticated user to change their own password.
        /// </summary>
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);

        Task<bool> LogoutAsync(string userId);
    }
}