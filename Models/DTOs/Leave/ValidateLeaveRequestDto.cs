namespace AttendanceManagementSystem.Models.DTOs.Leave
{
    public class ValidateLeaveRequestDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string LeaveTypeId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ExcludeLeaveId { get; set; }
    }
}
