using AttendanceManagementSystem.Common.Helpers;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace AttendanceManagementSystem.Data.Seeders
{
    public class InitialDataSeeder
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<InitialDataSeeder> _logger;

        public InitialDataSeeder(
            IRoleRepository roleRepository,
            IUserRepository userRepository,
            ILogger<InitialDataSeeder> logger)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedRolesAsync();
                await SeedUsersAsync();
                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database seeding");
                throw;
            }
        }

        private async Task SeedRolesAsync()
        {
            var existingRoles = await _roleRepository.GetAllAsync();
            if (existingRoles.Any())
            {
                _logger.LogInformation("Roles already exist. Skipping role seeding.");
                return;
            }

            var roles = new List<Role>
            {
                new Role
                {
                    Name = "SuperAdmin",
                    Description = "Super Administrator with full system access",
                    Permissions = new List<string> { "all", "manage_admins", "manage_invitations" }
                },
                new Role
                {
                    Name = "Admin",
                    Description = "Administrator with full access",
                    Permissions = new List<string> { "all", "manage_invitations" }
                },
                new Role
                {
                    Name = "Manager",
                    Description = "Manager with limited access",
                    Permissions = new List<string> { "read", "write", "manage_invitations" }
                },
                new Role
                {
                    Name = "Employee",
                    Description = "Regular employee",
                    Permissions = new List<string> { "read" }
                }
            };

            foreach (var role in roles)
            {
                await _roleRepository.CreateAsync(role);
                _logger.LogInformation($"Created role: {role.Name}");
            }
        }

        private async Task SeedUsersAsync()
        {
            var existingUsers = await _userRepository.GetAllAsync();
            if (existingUsers.Any())
            {
                _logger.LogInformation("Users already exist. Skipping user seeding.");
                return;
            }

            var superAdminRole = (await _roleRepository.GetAllAsync())
                .FirstOrDefault(r => r.Name == "SuperAdmin");

            if (superAdminRole == null)
            {
                _logger.LogWarning("SuperAdmin role not found. Cannot create SuperAdmin user.");
                return;
            }

            // Password: SuperAdmin@123
            var superAdminUser = new User
            {
                Username = "superadmin",
                Email = "chanderekumud@gmail.com",
                PasswordHash = PasswordHelper.HashPassword("SuperAdmin@123"),
                FirstName = "Super",
                LastName = "Admin",
                IsActive = true,
                RoleIds = new List<string> { superAdminRole.Id }
            };

            await _userRepository.CreateAsync(superAdminUser);

            _logger.LogInformation("===========================================");
            _logger.LogInformation("SUPER ADMIN USER CREATED SUCCESSFULLY");
            _logger.LogInformation("Username: superadmin");
            _logger.LogInformation("Password: SuperAdmin@123");
            _logger.LogInformation("Email: chanderekumud@gmail.com");
            _logger.LogInformation("===========================================");
        }
    }
}