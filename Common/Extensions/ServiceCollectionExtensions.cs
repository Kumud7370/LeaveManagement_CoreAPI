using System.Text;
using AttendanceManagementSystem.Common.Helpers;
using AttendanceManagementSystem.Data.Implementations;
using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Data.Seeders;
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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure MongoDB enum serialization FIRST (before any MongoDB operations)
            ConfigureMongoDbEnumSerialization();

            // Settings
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // Database Context
            services.AddSingleton<IMongoDbContext, MongoDbContext>();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IAdminInvitationRepository, AdminInvitationRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAdminManagementService, AdminManagementService>();

            // Helpers
            services.AddScoped<JwtHelper>();

            // Seeders
            services.AddScoped<InitialDataSeeder>();

            // Validators
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

            return services;
        }

        // NEW METHOD: Configure MongoDB to serialize enums as strings
        private static void ConfigureMongoDbEnumSerialization()
        {
            // Check if already registered to avoid duplicate registration errors
            if (!BsonSerializer.SerializerRegistry.GetSerializer<Gender>().GetType().Name.Contains("EnumSerializer"))
            {
                BsonSerializer.RegisterSerializer(new EnumSerializer<Gender>(BsonType.String));
                BsonSerializer.RegisterSerializer(new EnumSerializer<EmploymentType>(BsonType.String));
                BsonSerializer.RegisterSerializer(new EnumSerializer<EmployeeStatus>(BsonType.String));
            }
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