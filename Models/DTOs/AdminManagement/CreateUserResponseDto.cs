namespace AttendanceManagementSystem.Models.DTOs.AdminManagement
{
    public class CreateUserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; }
        public string? EmployeeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
