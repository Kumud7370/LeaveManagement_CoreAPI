using System.Text;
using AttendanceManagementSystem.Common.Helpers;
using AttendanceManagementSystem.Data.Implementations;
using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Data.Seeders;
using AttendanceManagementSystem.Data.Migrations;  
using AttendanceManagementSystem.Models.Settings;
using AttendanceManagementSystem.Repositories.Implementations;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Implementations;
using AttendanceManagementSystem.Services.Interfaces;
using AttendanceManagementSystem.Validators.Auth;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AttendanceManagementSystem.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            services.AddSingleton<IMongoDbContext, MongoDbContext>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IAdminInvitationRepository, AdminInvitationRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<ILeaveRepository, LeaveRepository>();
            services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
            services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IShiftRepository, ShiftRepository>();
            services.AddScoped<IEmployeeShiftRepository, EmployeeShiftRepository>();
            services.AddScoped<IDesignationRepository, DesignationRepository>();
            services.AddScoped<IHolidayRepository, HolidayRepository>();
            services.AddScoped<IWorkFromHomeRequestRepository, WorkFromHomeRequestRepository>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAdminManagementService, AdminManagementService>();
            services.AddScoped<ILeaveService, LeaveService>();
            services.AddScoped<ILeaveTypeService, LeaveTypeService>();
            services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IShiftService, ShiftService>();
            services.AddScoped<IEmployeeShiftService, EmployeeShiftService>();
            services.AddScoped<IDesignationService, DesignationService>();
            services.AddScoped<IHolidayService, HolidayService>();
            services.AddScoped<IWorkFromHomeRequestService, WorkFromHomeRequestService>();

            services.AddScoped<JwtHelper>();

            services.AddScoped<InitialDataSeeder>();

        
            services.AddScoped<EnumMigrationService>();

            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings!.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Attendance Management System API",
                    Version = "v1",
                    Description = "API for Attendance Management System with JWT Authentication"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
    }
}