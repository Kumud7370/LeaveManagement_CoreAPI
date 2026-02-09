using AttendanceManagementSystem.Common.Extensions;
using AttendanceManagementSystem.Common.Configuration;
using System.Text.Json.Serialization;

// CRITICAL: Configure MongoDB FIRST before anything else
MongoDbConfiguration.Configure();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Configure JSON options to serialize enums as strings
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This converts all enums to strings in JSON responses
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// Custom services
builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCustomExceptionHandler();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Run database migrations BEFORE seeding data
await app.RunDatabaseMigrationsAsync();

// Seed initial data
await app.SeedDataAsync();

app.Run();