using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models.DTOs.AdminManagement
{
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// Must be one of: "Tehsildar", "NayabTehsildar", "Employee"
        public string Role { get; set; } = string.Empty;

        /// Required when Role is "Employee" — links to an existing Employee record.
        public string? EmployeeId { get; set; }
    }
}