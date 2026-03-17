namespace AttendanceManagementSystem.Models.DTOs.Employee
{
    public class AssignmentHistoryResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string? FromDepartmentId { get; set; }
        public string? FromDepartmentName { get; set; }
        public string ToDepartmentId { get; set; } = string.Empty;
        public string ToDepartmentName { get; set; } = string.Empty;
        public string? FromDesignationId { get; set; }
        public string? FromDesignationName { get; set; }
        public string ToDesignationId { get; set; } = string.Empty;
        public string ToDesignationName { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public string? ChangedByName { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Reason { get; set; }
    }
}
