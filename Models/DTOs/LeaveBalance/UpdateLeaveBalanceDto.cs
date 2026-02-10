namespace AttendanceManagementSystem.Models.DTOs.LeaveBalance
{
    public class UpdateLeaveBalanceDto
    {
        public decimal? TotalAllocated { get; set; }
        public decimal? Consumed { get; set; }
        public decimal? CarriedForward { get; set; }
    }
}
