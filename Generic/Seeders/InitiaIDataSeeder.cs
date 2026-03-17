using AttendanceManagementSystem.Common.Helpers;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;

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
                await SeedAdminUserAsync();
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
            var existingNames = existingRoles.Select(r => r.Name).ToHashSet();

            // New role hierarchy — only create roles that don't exist yet
            var desiredRoles = new List<Role>
            {
                new Role
                {
                    Name        = "Admin",
                    Description = "Administrator — full access, creates and manages all users",
                    Permissions = new List<string> { "all" }
                },
                new Role
                {
                    Name        = "Tehsildar",
                    Description = "Senior authority — read-only access to employee data",
                    Permissions = new List<string> { "read" }
                },
                new Role
                {
                    Name        = "NayabTehsildar",
                    Description = "Senior authority — read-only access to employee data",
                    Permissions = new List<string> { "read" }
                },
                new Role
                {
                    Name        = "Employee",
                    Description = "Regular employee — can only login and change own password",
                    Permissions = new List<string> { "read" }
                }
            };

            foreach (var role in desiredRoles)
            {
                if (!existingNames.Contains(role.Name))
                {
                    await _roleRepository.CreateAsync(role);
                    _logger.LogInformation("Created role: {RoleName}", role.Name);
                }
                else
                {
                    _logger.LogInformation("Role already exists, skipping: {RoleName}", role.Name);
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            // Only create the admin user if NO users exist at all
            var existingUsers = await _userRepository.GetAllAsync();
            if (existingUsers.Any())
            {
                _logger.LogInformation("Users already exist. Skipping admin user seeding.");
                return;
            }

            var adminRole = await _roleRepository.GetByNameAsync("Admin");
            if (adminRole == null)
            {
                _logger.LogWarning("Admin role not found. Cannot create default admin user.");
                return;
            }

            // Default admin credentials — change immediately after first login
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@system.local",
                PasswordHash = PasswordHelper.HashPassword("Admin@123"),
                FirstName = "System",
                LastName = "Admin",
                IsActive = true,
                RoleIds = new List<string> { adminRole.Id }
            };

            await _userRepository.CreateAsync(adminUser);

            _logger.LogInformation("===========================================");
            _logger.LogInformation("DEFAULT ADMIN USER CREATED");
            _logger.LogInformation("Username : admin");
            _logger.LogInformation("Password : Admin@123");
            _logger.LogInformation("⚠️  Change this password after first login!");
            _logger.LogInformation("===========================================");
        }
    }
}