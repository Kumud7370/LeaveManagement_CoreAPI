namespace AttendanceManagementSystem.Models.DTOs.Employee
{
    public class ReassignEmployeeDto
    {
        public string ToDepartmentId { get; set; } = string.Empty;
        public string ToDesignationId { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
