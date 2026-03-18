namespace AttendanceManagementSystem.Models.DTOs.Employee
{
    public class BulkReassignEmployeeDto
    {
        public List<string> EmployeeIds { get; set; } = new();

        public string ToDepartmentId { get; set; } = string.Empty;
        public string ToDesignationId { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
