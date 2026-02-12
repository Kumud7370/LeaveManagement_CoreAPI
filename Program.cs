//using AttendanceManagementSystem.Common.Extensions;
//using System.Text.Json.Serialization;


//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
//    });

//builder.Services.AddEndpointsApiExplorer();

//// Custom services
//builder.Services.AddCustomServices(builder.Configuration);
//builder.Services.AddJwtAuthentication(builder.Configuration);
//builder.Services.AddSwaggerDocumentation();

//// CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReactApp", policy =>
//    {
//        policy.WithOrigins("http://localhost:4200")
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials();
//    });
//});

//var app = builder.Build();

//// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseCustomExceptionHandler();

//app.UseHttpsRedirection();

//app.UseCors("AllowAll");

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//// Run database migrations BEFORE seeding data
//await app.RunDatabaseMigrationsAsync();

//// Seed initial data
//await app.SeedDataAsync();

//app.Run();


using AttendanceManagementSystem.Common.Extensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// CORS Configuration - Fixed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")  // Your Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCustomExceptionHandler();

app.UseHttpsRedirection();

// ? CRITICAL FIX: Use the correct policy name that matches the one defined above
app.UseCors("AllowAngularApp");  // Changed from "AllowAll" to "AllowAngularApp"

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunDatabaseMigrationsAsync();

await app.SeedDataAsync();

app.Run();