using AttendanceManagementSystem.Data.Migrations;

namespace AttendanceManagementSystem.Common.Extensions
{
    public static class MigrationExtensions
    {
       
        public static async Task RunDatabaseMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("🚀 Running database migrations...");

                var enumMigration = services.GetRequiredService<EnumMigrationService>();
                await enumMigration.MigrateEnumsToStringsAsync();

                logger.LogInformation("✅ All database migrations completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Database migration failed: {Message}", ex.Message);
                
            }
        }
    }
}