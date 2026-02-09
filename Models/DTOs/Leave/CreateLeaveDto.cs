namespace AttendanceManagementSystem.Models.DTOs.Leave
{
    public class CreateLeaveDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string LeaveTypeId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsEmergencyLeave { get; set; } = false;
        public string? AttachmentUrl { get; set; }
    }
}
