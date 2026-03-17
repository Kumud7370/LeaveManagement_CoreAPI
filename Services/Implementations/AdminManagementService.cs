using AttendanceManagementSystem.Common.Helpers;
using AttendanceManagementSystem.Models.DTOs.AdminManagement;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class AdminManagementService : IAdminManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<AdminManagementService> _logger;

        // Roles that Admin is allowed to create
        private static readonly HashSet<string> AllowedRoles = new()
        {
            "Tehsildar",
            "NayabTehsildar",
            "Employee",
            "HR"
        };

        public AdminManagementService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IAuditLogRepository auditLogRepository,
            IEmployeeRepository employeeRepository,
            ILogger<AdminManagementService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _auditLogRepository = auditLogRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        // ─────────────────────────────────────────────
        //  CREATE USER
        // ─────────────────────────────────────────────
        public async Task<CreateUserResponseDto?> CreateUserAsync(
            CreateUserDto dto,
            string createdById,
            string createdByName,
            string? ipAddress = null)
        {
            try
            {
                // 1. Validate role
                if (!AllowedRoles.Contains(dto.Role))
                {
                    _logger.LogWarning("CreateUser: invalid role '{Role}'", dto.Role);
                    return null;
                }

                // 2. Username must be unique
                var existing = await _userRepository.GetByUsernameAsync(dto.Username);
                if (existing != null)
                {
                    _logger.LogWarning("CreateUser: username '{Username}' already exists", dto.Username);
                    return null;
                }

                // 3. Email must be unique
                var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingEmail != null)
                {
                    _logger.LogWarning("CreateUser: email '{Email}' already exists", dto.Email);
                    return null;
                }

                // 4. Resolve the role document
                var role = await _roleRepository.GetByNameAsync(dto.Role);
                if (role == null)
                {
                    _logger.LogWarning("CreateUser: role '{Role}' not found in DB", dto.Role);
                    return null;
                }

                // 5. For Employee role, link to existing employee record
                string? resolvedEmployeeId = null;
                if (dto.Role == "Employee")
                {
                    if (string.IsNullOrWhiteSpace(dto.EmployeeId))
                    {
                        _logger.LogWarning("CreateUser: EmployeeId is required when creating an Employee user");
                        return null;
                    }

                    var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
                    if (employee == null || employee.IsDeleted)
                    {
                        _logger.LogWarning("CreateUser: employee record '{EmployeeId}' not found", dto.EmployeeId);
                        return null;
                    }

                    if (!string.IsNullOrEmpty(employee.UserId))
                    {
                        _logger.LogWarning("CreateUser: employee '{EmployeeId}' already has a user account", dto.EmployeeId);
                        return null;
                    }

                    resolvedEmployeeId = employee.Id;
                }

                if (dto.Role == "HR")
                {
                    if (string.IsNullOrWhiteSpace(dto.DepartmentId))
                    {
                        _logger.LogWarning("CreateUser: DepartmentId is required when creating an HR user");
                        return null;
                    }
                }

                // 6. Create the user
                var newUser = new User
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PasswordHash = PasswordHelper.HashPassword(dto.Password),
                    RoleIds = new List<string> { role.Id },
                    IsActive = true,
                    EmployeeId = resolvedEmployeeId,
                    DepartmentId = dto.Role == "HR" ? dto.DepartmentId : null,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateAsync(newUser);
                if (createdUser == null)
                {
                    _logger.LogError("CreateUser: failed to persist user for '{Username}'", dto.Username);
                    return null;
                }

                // 7. Link user back to employee record
                if (resolvedEmployeeId != null)
                {
                    var employee = await _employeeRepository.GetByIdAsync(resolvedEmployeeId);
                    if (employee != null)
                    {
                        employee.UserId = createdUser.Id;
                        await _employeeRepository.UpdateAsync(employee.Id, employee);
                    }
                }

                // 8. Audit log
                await LogAuditAsync(
                    createdById, createdByName,
                    "CreateUser", "User", createdUser.Id,
                    $"Created user '{dto.Username}' with role '{dto.Role}'",
                    ipAddress);

                return MapToDto(createdUser, new List<string> { dto.Role });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateUser: unexpected error for '{Username}'", dto.Username);
                return null;
            }
        }

        // ─────────────────────────────────────────────
        //  UPDATE USER
        // ─────────────────────────────────────────────
        public async Task<CreateUserResponseDto?> UpdateUserAsync(
            string userId,
            UpdateUserDto dto,
            string updatedById,
            string? ipAddress = null)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || user.IsDeleted)
                    return null;

                if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
                if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;

                if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
                {
                    var emailTaken = await _userRepository.GetByEmailAsync(dto.Email);
                    if (emailTaken != null)
                    {
                        _logger.LogWarning("UpdateUser: email '{Email}' already in use", dto.Email);
                        return null;
                    }
                    user.Email = dto.Email;
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(userId, user);

                var roles = await _roleRepository.GetRolesByIdsAsync(user.RoleIds);
                var roleNames = roles.Select(r => r.Name).ToList();

                var updater = await _userRepository.GetByIdAsync(updatedById);
                await LogAuditAsync(
                    updatedById, updater?.Username ?? "Unknown",
                    "UpdateUser", "User", userId,
                    $"Updated user '{user.Username}'",
                    ipAddress);

                return MapToDto(user, roleNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateUser: unexpected error for user '{UserId}'", userId);
                return null;
            }
        }

        // ─────────────────────────────────────────────
        //  ACTIVATE / DEACTIVATE USER
        // ─────────────────────────────────────────────
        public async Task<bool> SetUserActiveStatusAsync(
            string userId,
            bool isActive,
            string changedById,
            string? ipAddress = null)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || user.IsDeleted)
                    return false;

                user.IsActive = isActive;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(userId, user);

                var changer = await _userRepository.GetByIdAsync(changedById);
                await LogAuditAsync(
                    changedById, changer?.Username ?? "Unknown",
                    isActive ? "ActivateUser" : "DeactivateUser",
                    "User", userId,
                    $"Set IsActive={isActive} for user '{user.Username}'",
                    ipAddress);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetUserActiveStatus: error for user '{UserId}'", userId);
                return false;
            }
        }

        // ─────────────────────────────────────────────
        //  DELETE USER
        // ─────────────────────────────────────────────
        public async Task<bool> DeleteUserAsync(
            string userId,
            string deletedById,
            string? ipAddress = null)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || user.IsDeleted)
                    return false;

                await _userRepository.DeleteAsync(userId);

                var deleter = await _userRepository.GetByIdAsync(deletedById);
                await LogAuditAsync(
                    deletedById, deleter?.Username ?? "Unknown",
                    "DeleteUser", "User", userId,
                    $"Deleted user '{user.Username}'",
                    ipAddress);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteUser: error for user '{UserId}'", userId);
                return false;
            }
        }

        // ─────────────────────────────────────────────
        //  GET ALL USERS
        // ─────────────────────────────────────────────
        public async Task<IEnumerable<CreateUserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var result = new List<CreateUserResponseDto>();

            foreach (var user in users)
            {
                var roles = await _roleRepository.GetRolesByIdsAsync(user.RoleIds);
                result.Add(MapToDto(user, roles.Select(r => r.Name).ToList()));
            }

            return result;
        }

        // ─────────────────────────────────────────────
        //  GET USER BY ID
        // ─────────────────────────────────────────────
        public async Task<CreateUserResponseDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return null;

            var roles = await _roleRepository.GetRolesByIdsAsync(user.RoleIds);
            return MapToDto(user, roles.Select(r => r.Name).ToList());
        }

        // ─────────────────────────────────────────────
        //  ADMIN RESETS USER PASSWORD
        // ─────────────────────────────────────────────
        public async Task<bool> ResetUserPasswordAsync(
            string userId,
            string newPassword,
            string resetById,
            string? ipAddress = null)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || user.IsDeleted)
                    return false;

                user.PasswordHash = PasswordHelper.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                // Invalidate any existing refresh token for security
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                await _userRepository.UpdateAsync(userId, user);

                var resetter = await _userRepository.GetByIdAsync(resetById);
                await LogAuditAsync(
                    resetById, resetter?.Username ?? "Unknown",
                    "AdminResetPassword", "User", userId,
                    $"Admin reset password for user '{user.Username}'",
                    ipAddress);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResetUserPassword: error for user '{UserId}'", userId);
                return false;
            }
        }

        // ─────────────────────────────────────────────
        //  PRIVATE HELPERS
        // ─────────────────────────────────────────────
        private async Task LogAuditAsync(
            string userId, string userName,
            string action, string entity, string? entityId,
            string? changes, string? ipAddress)
        {
            var log = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Changes = changes,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };
            await _auditLogRepository.CreateAsync(log);
        }

        private static CreateUserResponseDto MapToDto(User user, List<string> roleNames)
        {
            return new CreateUserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = roleNames,
                IsActive = user.IsActive,
                EmployeeId = user.EmployeeId,
                DepartmentId = user.DepartmentId,
                CreatedAt = user.CreatedAt
            };
        }
    }
}