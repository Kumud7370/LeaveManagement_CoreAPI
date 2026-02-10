namespace AttendanceManagementSystem.Models.DTOs.LeaveBalance
{
    public class AdjustLeaveBalanceDto
    {
        public decimal AdjustmentAmount { get; set; }
        public string AdjustmentReason { get; set; } = string.Empty;
        public string AdjustmentType { get; set; } = "Manual";
    }
}
