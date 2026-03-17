using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models.DTOs.AdminManagement
{
    public class AdminResetPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}