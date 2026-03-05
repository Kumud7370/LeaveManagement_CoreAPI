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
        private readonly IEmailService _emailService;
        private readonly JwtHelper _jwtHelper;
        private readonly IEmployeeRepository _employeeRepository;

        private static readonly Dictionary<string, (string Otp, DateTime ExpiryTime)> _otpStore = new();

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IEmailService emailService,
            JwtHelper jwtHelper,
            IEmployeeRepository employeeRepository)  
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _emailService = emailService;
            _jwtHelper = jwtHelper;
            _employeeRepository = employeeRepository; 
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            Console.WriteLine($"[LOGIN] Username: {request.Username}");
            Console.WriteLine($"[LOGIN] Password: {request.Password}");

            var user = await _userRepository.GetByUsernameAsync(request.Username);

            Console.WriteLine($"[LOGIN] User Found: {user != null}");
            if (user != null)
            {
                Console.WriteLine($"[LOGIN] Stored Hash: {user.PasswordHash}");
                Console.WriteLine($"[LOGIN] IsActive: {user.IsActive}");
            }

            if (user == null || !user.IsActive)
                return null;
            var isPasswordValid = PasswordHelper.VerifyPassword(request.Password, user.PasswordHash);

            Console.WriteLine($"[LOGIN] Password Valid: {isPasswordValid}");

            if (!isPasswordValid)
                return null;

            if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
                return null;

            var roles = await _roleRepository.GetRolesByIdsAsync(user.RoleIds);
            var roleNames = roles.Select(r => r.Name).ToList();

            var accessToken = _jwtHelper.GenerateAccessToken(user, roleNames);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            await _userRepository.UpdateRefreshTokenAsync(
                user.Id,
                refreshToken,
                DateTime.UtcNow.AddDays(_jwtHelper.GetRefreshTokenExpirationDays())
            );

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
                    EmployeeId = user.EmployeeId
                }
            };
        }

        public async Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return null;

            var roles = await _roleRepository.GetRolesByIdsAsync(user.RoleIds);
            var roleNames = roles.Select(r => r.Name).ToList();

            var accessToken = _jwtHelper.GenerateAccessToken(user, roleNames);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            await _userRepository.UpdateRefreshTokenAsync(
                user.Id,
                refreshToken,
                DateTime.UtcNow.AddDays(_jwtHelper.GetRefreshTokenExpirationDays())
            );

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
                    EmployeeId = user.EmployeeId
                }
            };
        }

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
            return await _userRepository.UpdateAsync(userId, user);
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return false;

            await _userRepository.UpdateRefreshTokenAsync(userId, string.Empty, DateTime.UtcNow);
            return true;
        }

        public async Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
                throw new Exception("Username already exists");

            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
                throw new Exception("Email already exists");

            var employee = await _employeeRepository.GetByEmailAsync(request.Email);
            if (employee == null)
            {
                var newEmployee = new Employee
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    EmployeeCode = "EMP-" + DateTime.UtcNow.Ticks.ToString().Substring(10), 
                    EmployeeStatus = Models.Enums.EmployeeStatus.Active,
                    EmploymentType = Models.Enums.EmploymentType.FullTime,
                    Gender = Models.Enums.Gender.Male,
                    DateOfJoining = DateTime.UtcNow,
                    DateOfBirth = DateTime.UtcNow.AddYears(-18), 
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                try
                {
                    employee = await _employeeRepository.CreateAsync(newEmployee);
                    Console.WriteLine($"[REGISTER] Employee created: {employee?.Id ?? "NULL"}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[REGISTER] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                    throw;
                }

                if (employee == null)
                    throw new Exception("Failed to create employee record during registration.");
            }

            // Get default Employee role
            var employeeRole = await _roleRepository.GetByNameAsync("Employee");
            if (employeeRole == null)
                throw new Exception("Default Employee role not found");

            var newUser = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Username = request.Username,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                RoleIds = new List<string> { employeeRole.Id },
                IsActive = true,
                EmployeeId = employee.Id,  
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(newUser);

            if (createdUser == null)
                return null;

            employee.UserId = createdUser.Id;
            await _employeeRepository.UpdateAsync(employee.Id, employee);

            await _emailService.SendWelcomeEmailAsync(
                createdUser.Email,
                $"{createdUser.FirstName} {createdUser.LastName}",
                "Employee"
            );


            var roleNames = new List<string> { employeeRole.Name };
            var accessToken = _jwtHelper.GenerateAccessToken(createdUser, roleNames);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            await _userRepository.UpdateRefreshTokenAsync(
                createdUser.Id,
                refreshToken,
                DateTime.UtcNow.AddDays(_jwtHelper.GetRefreshTokenExpirationDays())
            );

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtHelper.GetAccessTokenExpirationMinutes()),
                User = new UserInfoDto
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    Roles = roleNames,
                    EmployeeId = employee.Id
                }
            };
        }

        public async Task<bool> RequestPasswordResetOtpAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return false;

            var otp = new Random().Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(3);

            _otpStore[email.ToLower()] = (otp, expiryTime);

            var subject = "Password Reset OTP";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #1a2a6c; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .otp-box {{ background-color: white; padding: 20px; text-align: center; border-radius: 8px; margin: 20px 0; border: 2px solid #1a2a6c; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #1a2a6c; letter-spacing: 5px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hello {user.FirstName},</p>
            <p>We received a request to reset your password for your Attendance Management System account.</p>
            <p>Your One-Time Password (OTP) is:</p>
            <div class='otp-box'>
                <div class='otp-code'>{otp}</div>
            </div>
            <p><strong>This OTP will expire in 3 minutes.</strong></p>
            <p>If you didn't request this password reset, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>© 2024 Attendance Management System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            await _emailService.SendEmailAsync(email, subject, body);
            return true;
        }

        public async Task<bool> VerifyPasswordResetOtpAsync(string email, string otp)
        {
            var normalizedEmail = email.ToLower();

            if (!_otpStore.TryGetValue(normalizedEmail, out var otpData))
                return false;

            if (DateTime.UtcNow > otpData.ExpiryTime)
            {
                _otpStore.Remove(normalizedEmail);
                return false;
            }

            return otpData.Otp == otp;
        }

        public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            if (!await VerifyPasswordResetOtpAsync(email, otp))
                return false;

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return false;

            Console.WriteLine($"[RESET] Email: {email}");
            Console.WriteLine($"[RESET] New Password: {newPassword}");
            Console.WriteLine($"[RESET] Old Hash: {user.PasswordHash}");

            user.PasswordHash = PasswordHelper.HashPassword(newPassword);

            Console.WriteLine($"[RESET] New Hash: {user.PasswordHash}");

            var result = await _userRepository.UpdateAsync(user.Id, user);

            Console.WriteLine($"[RESET] Update Result: {result}");

            if (result)
            {
                _otpStore.Remove(email.ToLower());

                await _userRepository.UpdateRefreshTokenAsync(user.Id, string.Empty, DateTime.UtcNow);
            }

            return result;
        }
    }
}