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
                    Name = "Admin",
                    Description = "Administrator with full access",
                    Permissions = new List<string> { "all" }
                },
                new Role
                {
                    Name = "Manager",
                    Description = "Manager with limited access",
                    Permissions = new List<string> { "read", "write" }
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

            var adminRole = (await _roleRepository.GetAllAsync())
                .FirstOrDefault(r => r.Name == "Admin");

            if (adminRole == null)
            {
                _logger.LogWarning("Admin role not found. Cannot create admin user.");
                return;
            }

            // Password: Admin@123
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@attendance.com",
                PasswordHash = PasswordHelper.HashPassword("Admin@123"),
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                RoleIds = new List<string> { adminRole.Id }
            };

            await _userRepository.CreateAsync(adminUser);

            _logger.LogInformation("===========================================");
            _logger.LogInformation("ADMIN USER CREATED SUCCESSFULLY");
            _logger.LogInformation("Username: admin");
            _logger.LogInformation("Password: Admin@123");
            _logger.LogInformation("Email: admin@attendance.com");
            _logger.LogInformation("===========================================");
        }
    }
}