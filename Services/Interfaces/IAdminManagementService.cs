using AttendanceManagementSystem.Models.DTOs.AdminManagement;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IAdminManagementService
    {
        // Create any user (Tehsildar, NayabTehsildar, Employee) directly
        Task<CreateUserResponseDto?> CreateUserAsync(CreateUserDto dto, string createdById, string createdByName, string? ipAddress = null);

        // Update an existing user's details
        Task<CreateUserResponseDto?> UpdateUserAsync(string userId, UpdateUserDto dto, string updatedById, string? ipAddress = null);

        // Deactivate / activate a user account
        Task<bool> SetUserActiveStatusAsync(string userId, bool isActive, string changedById, string? ipAddress = null);

        // Delete a user (soft delete)
        Task<bool> DeleteUserAsync(string userId, string deletedById, string? ipAddress = null);

        // Get all users (admin view)
        Task<IEnumerable<CreateUserResponseDto>> GetAllUsersAsync();

        // Get a single user by ID
        Task<CreateUserResponseDto?> GetUserByIdAsync(string userId);

        // Admin resets another user's password directly
        Task<bool> ResetUserPasswordAsync(string userId, string newPassword, string resetById, string? ipAddress = null);
    }
}