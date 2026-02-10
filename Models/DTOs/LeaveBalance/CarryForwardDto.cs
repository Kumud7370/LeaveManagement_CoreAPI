namespace AttendanceManagementSystem.Models.DTOs.LeaveBalance
{
    public class CarryForwardDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string LeaveTypeId { get; set; } = string.Empty;
        public int FromYear { get; set; }
        public int ToYear { get; set; }
        public decimal CarryForwardDays { get; set; }
    }
}
