using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models.DTOs.AdminManagement
{
    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }
}