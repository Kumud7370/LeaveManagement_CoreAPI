using AttendanceManagementSystem.Common.Helpers;
using AttendanceManagementSystem.Models.DTOs.AdminManagement;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class AdminManagementService : IAdminManagementService
    {
        private readonly IAdminInvitationRepository _invitationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminManagementService> _logger;
        private readonly IEmployeeRepository _employeeRepository;

        private readonly Dictionary<string, List<string>> _roleHierarchy = new()
        {
            { "SuperAdmin", new List<string> { "Admin", "Manager", "Employee" } },
            { "Admin", new List<string> { "Manager", "Employee" } },
            { "Manager", new List<string> { "Employee" } },
            { "Employee", new List<string>() }
        };

        public AdminManagementService(
            IAdminInvitationRepository invitationRepository,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IAuditLogRepository auditLogRepository,
            IEmailService emailService,
            ILogger<AdminManagementService> logger,
            IEmployeeRepository employeeRepository)
        {
            _invitationRepository = invitationRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _auditLogRepository = auditLogRepository;
            _emailService = emailService;
            _logger = logger;
            _employeeRepository = employeeRepository;
        }

        public async Task<InvitationResponseDto?> SendInvitationAsync(
            SendInvitationDto dto,
            string inviterId,
            string inviterName,
            string? ipAddress = null)
        {
            try
            {
                var inviter = await _userRepository.GetByIdAsync(inviterId);
                if (inviter == null)
                {
                    _logger.LogWarning($"Inviter with ID {inviterId} not found");
                    return null;
                }

                var inviterRoles = await _roleRepository.GetRolesByIdsAsync(inviter.RoleIds);
                var inviterRoleNames = inviterRoles.Select(r => r.Name).ToList();

                if (!CanInviteRole(inviterRoleNames, dto.Role))
                {
                    _logger.LogWarning($"User {inviterName} cannot invite role {dto.Role}");
                    return null;
                }

                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"User with email {dto.Email} already exists");
                    return null;
                }

                if (dto.Role == "Employee")
                {
                    var existingEmployee = await _employeeRepository.GetByEmailAsync(dto.Email);
                    if (existingEmployee == null)
                    {
                        _logger.LogWarning($"No employee record found for {dto.Email}. Create the employee entry first before sending an Employee invitation.");
                        return null;
                    }

                    if (!string.IsNullOrEmpty(existingEmployee.UserId))
                    {
                        _logger.LogWarning($"Employee {dto.Email} is already linked to a user account.");
                        return null;
                    }
                }

                var existingInvitation = await _invitationRepository.GetByEmailAsync(dto.Email);
                if (existingInvitation != null && existingInvitation.Status == "Pending")
                {
                    _logger.LogWarning($"Pending invitation already exists for {dto.Email}");
                    return null;
                }

                var token = GenerateSecureToken();

                var invitation = new AdminInvitation
                {
                    Email = dto.Email,
                    InvitedRole = dto.Role,
                    Token = token,
                    InvitedBy = inviterId,
                    InvitedByName = inviterName,
                    Status = "Pending",
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    Notes = dto.Notes
                };

                await _invitationRepository.CreateAsync(invitation);

                await _emailService.SendInvitationEmailAsync(
                    dto.Email,
                    dto.Email.Split('@')[0],
                    token,
                    dto.Role,
                    inviterName
                );

                await LogAuditAsync(
                    inviterId,
                    inviterName,
                    "SendInvitation",
                    "AdminInvitation",
                    invitation.Id,
                    $"Invited {dto.Email} as {dto.Role}",
                    ipAddress
                );

                return MapToDto(invitation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invitation");
                return null;
            }
        }

        public async Task<InvitationResponseDto?> UpdateInvitationAsync(
            string invitationId,
            EditInvitationDto dto,
            string updatedBy,
            string? ipAddress = null)
        {
            try
            {
                var invitation = await _invitationRepository.GetByIdAsync(invitationId);
                if (invitation == null || invitation.Status != "Pending")
                {
                    return null;
                }

                var changes = new StringBuilder();

                if (!string.IsNullOrEmpty(dto.Email) && dto.Email != invitation.Email)
                {
                    changes.Append($"Email: {invitation.Email} -> {dto.Email}; ");
                    invitation.Email = dto.Email;
                }

                if (!string.IsNullOrEmpty(dto.Role) && dto.Role != invitation.InvitedRole)
                {
                    changes.Append($"Role: {invitation.InvitedRole} -> {dto.Role}; ");
                    invitation.InvitedRole = dto.Role;
                }

                if (dto.Notes != null && dto.Notes != invitation.Notes)
                {
                    changes.Append($"Notes updated; ");
                    invitation.Notes = dto.Notes;
                }

                if (changes.Length > 0)
                {
                    await _invitationRepository.UpdateAsync(invitationId, invitation);

                    var user = await _userRepository.GetByIdAsync(updatedBy);
                    await LogAuditAsync(
                        updatedBy,
                        user?.Username ?? "Unknown",
                        "UpdateInvitation",
                        "AdminInvitation",
                        invitationId,
                        changes.ToString(),
                        ipAddress
                    );
                }

                return MapToDto(invitation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating invitation {invitationId}");
                return null;
            }
        }

        public async Task<bool> RevokeInvitationAsync(string invitationId, string revokedBy, string? ipAddress = null)
        {
            try
            {
                var invitation = await _invitationRepository.GetByIdAsync(invitationId);
                if (invitation == null || invitation.Status != "Pending")
                {
                    return false;
                }

                invitation.Status = "Revoked";
                invitation.RevokedAt = DateTime.UtcNow;
                invitation.RevokedBy = revokedBy;

                await _invitationRepository.UpdateAsync(invitationId, invitation);

                var user = await _userRepository.GetByIdAsync(revokedBy);
                await LogAuditAsync(
                    revokedBy,
                    user?.Username ?? "Unknown",
                    "RevokeInvitation",
                    "AdminInvitation",
                    invitationId,
                    $"Revoked invitation for {invitation.Email}",
                    ipAddress
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error revoking invitation {invitationId}");
                return false;
            }
        }

        public async Task<bool> DeleteInvitationAsync(string invitationId, string deletedBy, string? ipAddress = null)
        {
            try
            {
                var invitation = await _invitationRepository.GetByIdAsync(invitationId);
                if (invitation == null)
                {
                    return false;
                }

                await _invitationRepository.DeleteAsync(invitationId);

                var user = await _userRepository.GetByIdAsync(deletedBy);
                await LogAuditAsync(
                    deletedBy,
                    user?.Username ?? "Unknown",
                    "DeleteInvitation",
                    "AdminInvitation",
                    invitationId,
                    $"Deleted invitation for {invitation.Email}",
                    ipAddress
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting invitation {invitationId}");
                return false;
            }
        }

        public async Task<IEnumerable<InvitationResponseDto>> GetAllInvitationsAsync()
        {
            var invitations = await _invitationRepository.GetAllAsync();
            return invitations.Select(MapToDto);
        }

        public async Task<IEnumerable<InvitationResponseDto>> GetMyInvitationsAsync(string inviterId)
        {
            var invitations = await _invitationRepository.GetByInviterAsync(inviterId);
            return invitations.Select(MapToDto);
        }

        public async Task<InvitationResponseDto?> ValidateTokenAsync(string token)
        {
            var invitation = await _invitationRepository.GetByTokenAsync(token);

            if (invitation == null)
            {
                return null;
            }

            if (invitation.Status != "Pending")
            {
                return null;
            }

            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                invitation.Status = "Expired";
                await _invitationRepository.UpdateAsync(invitation.Id, invitation);
                return null;
            }

            return MapToDto(invitation);
        }

        public async Task<bool> AcceptInvitationAsync(AcceptInvitationDto dto, string? ipAddress = null)
        {
            try
            {
                var invitation = await _invitationRepository.GetByTokenAsync(dto.Token);

                if (invitation == null || invitation.Status != "Pending")
                {
                    _logger.LogWarning($"Invalid or non-pending invitation token");
                    return false;
                }

                if (invitation.ExpiresAt < DateTime.UtcNow)
                {
                    invitation.Status = "Expired";
                    await _invitationRepository.UpdateAsync(invitation.Id, invitation);
                    _logger.LogWarning($"Invitation token expired");
                    return false;
                }

                var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Username {dto.Username} already exists");
                    return false;
                }

                var role = await _roleRepository.GetByNameAsync(invitation.InvitedRole);
                if (role == null)
                {
                    _logger.LogWarning($"Role {invitation.InvitedRole} not found");
                    return false;
                }

                Employee? employee = null;

                if (invitation.InvitedRole == "Employee")
                {
                    // Employee flow: must link to the pre-existing employee record created by admin.
                    // Never create a new one — that would cause the duplicate entry problem.
                    employee = await _employeeRepository.GetByEmailAsync(invitation.Email);
                    if (employee == null)
                    {
                        _logger.LogWarning($"No employee record found for {invitation.Email}. Cannot complete Employee registration.");
                        return false;
                    }
                    if (!string.IsNullOrEmpty(employee.UserId))
                    {
                        _logger.LogWarning($"Employee {invitation.Email} is already linked to a user account.");
                        return false;
                    }
                }
                else
                {
                    // Admin/Manager flow: create a minimal employee record for them since
                    // they are also staff members but weren't pre-created in the employee form.
                    employee = await _employeeRepository.GetByEmailAsync(invitation.Email);
                    if (employee == null)
                    {
                        var newEmployee = new Employee
                        {
                            FirstName = dto.FirstName,
                            LastName = dto.LastName,
                            Email = invitation.Email,
                            EmployeeCode = "EMP-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                            EmployeeStatus = Models.Enums.EmployeeStatus.Active,
                            EmploymentType = Models.Enums.EmploymentType.FullTime,
                            DateOfJoining = DateTime.UtcNow,
                            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            PhoneNumber = string.Empty,
                            DepartmentId = string.Empty,
                            DesignationId = string.Empty,
                            Address = new Models.ValueObjects.Address(),
                            CreatedBy = invitation.InvitedBy,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        employee = await _employeeRepository.CreateAsync(newEmployee);
                    }
                }

                var newUser = new User
                {
                    Username = dto.Username,
                    Email = invitation.Email,
                    PasswordHash = PasswordHelper.HashPassword(dto.Password),
                    FirstName = !string.IsNullOrWhiteSpace(dto.FirstName) ? dto.FirstName : employee.FirstName,
                    LastName = !string.IsNullOrWhiteSpace(dto.LastName) ? dto.LastName : employee.LastName,
                    IsActive = true,
                    RoleIds = new List<string> { role.Id },
                    EmployeeId = employee.Id
                };

                var createdUser = await _userRepository.CreateAsync(newUser);

                // Link the employee record to the new user account
                employee.UserId = createdUser.Id;
                if (!string.IsNullOrWhiteSpace(dto.FirstName)) employee.FirstName = dto.FirstName;
                if (!string.IsNullOrWhiteSpace(dto.LastName)) employee.LastName = dto.LastName;
                await _employeeRepository.UpdateAsync(employee.Id, employee);

                invitation.Status = "Accepted";
                invitation.AcceptedAt = DateTime.UtcNow;
                await _invitationRepository.UpdateAsync(invitation.Id, invitation);

                await _emailService.SendWelcomeEmailAsync(
                    newUser.Email,
                    $"{newUser.FirstName} {newUser.LastName}",
                    invitation.InvitedRole
                );

                await LogAuditAsync(
                    newUser.Id,
                    newUser.Username,
                    "AcceptInvitation",
                    "User",
                    newUser.Id,
                    $"Accepted invitation and created account as {invitation.InvitedRole}",
                    ipAddress
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting invitation");
                return false;
            }
        }

        private bool CanInviteRole(List<string> inviterRoles, string targetRole)
        {
            foreach (var inviterRole in inviterRoles)
            {
                if (_roleHierarchy.ContainsKey(inviterRole) &&
                    _roleHierarchy[inviterRole].Contains(targetRole))
                {
                    return true;
                }
            }
            return false;
        }

        private string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, 32);
        }

        private async Task LogAuditAsync(
            string userId,
            string userName,
            string action,
            string entity,
            string? entityId,
            string? changes,
            string? ipAddress)
        {
            var auditLog = new AuditLog
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

            await _auditLogRepository.CreateAsync(auditLog);
        }

        private InvitationResponseDto MapToDto(AdminInvitation invitation)
        {
            return new InvitationResponseDto
            {
                Id = invitation.Id,
                Email = invitation.Email,
                InvitedRole = invitation.InvitedRole,
                Token = invitation.Token,
                InvitedBy = invitation.InvitedBy,
                InvitedByName = invitation.InvitedByName,
                Status = invitation.Status,
                ExpiresAt = invitation.ExpiresAt,
                CreatedAt = invitation.CreatedAt,
                AcceptedAt = invitation.AcceptedAt,
                RevokedAt = invitation.RevokedAt,
                RevokedBy = invitation.RevokedBy,
                Notes = invitation.Notes
            };
        }
    }
}