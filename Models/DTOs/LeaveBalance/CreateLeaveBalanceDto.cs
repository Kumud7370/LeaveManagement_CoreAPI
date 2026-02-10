namespace AttendanceManagementSystem.Models.DTOs.LeaveBalance
{
    public class CreateLeaveBalanceDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string LeaveTypeId { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal CarriedForward { get; set; } = 0;
    }
}
