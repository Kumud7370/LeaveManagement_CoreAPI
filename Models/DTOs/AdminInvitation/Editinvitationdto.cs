using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models.DTOs.AdminManagement
{
    public class EditInvitationDto
    {
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        public string? Role { get; set; }

        public string? Notes { get; set; }
    }
}