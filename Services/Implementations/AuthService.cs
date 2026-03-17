using AttendanceManagementSystem.Common.Helpers;
using AttendanceManagementSystem.Models.DTOs.Auth;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly JwtHelper _jwtHelper;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            JwtHelper jwtHelper)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _jwtHelper = jwtHelper;
        }

        //  LOGIN
        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);

            if (user == null || !user.IsActive)
                return null;

            if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
                return null;

            return await BuildLoginResponseAsync(user);
        }

        //  REFRESH TOKEN
        public async Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return null;

            return await BuildLoginResponseAsync(user);
        }

        //  CHANGE PASSWORD  (available to ALL roles)
        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            if (!PasswordHelper.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                return false;

            if (request.NewPassword != request.ConfirmPassword)
                return false;

            user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            return await _userRepository.UpdateAsync(userId, user);
        }

        //  LOGOUT
        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            await _userRepository.UpdateRefreshTokenAsync(userId, string.Empty, DateTime.UtcNow);
            return true;
        }

        //  PRIVATE HELPERS
        private async Task<LoginResponseDto> BuildLoginResponseAsync(User user)
        {
            var roles = await _roleRepository.GetRolesByIdsAsync(user.RoleIds);
            var roleNames = roles.Select(r => r.Name).ToList();

            var accessToken = _jwtHelper.GenerateAccessToken(user, roleNames);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            await _userRepository.UpdateRefreshTokenAsync(
                user.Id,
                refreshToken,
                DateTime.UtcNow.AddDays(_jwtHelper.GetRefreshTokenExpirationDays()));

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtHelper.GetAccessTokenExpirationMinutes()),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roleNames,
                    EmployeeId = user.EmployeeId,
                    DepartmentId = user.DepartmentId
                }
            };
        }
    }
}