using AttendanceManagementSystem.Common.Middleware;
using AttendanceManagementSystem.Data.Seeders;

namespace AttendanceManagementSystem.Common.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }

        public static async Task<IApplicationBuilder> SeedDataAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<InitialDataSeeder>();
            await seeder.SeedAsync();
            return app;
        }
    }
}