namespace AttendanceManagementSystem.Models.DTOs.LeaveBalance
{
    public class EmployeeLeaveBalanceSummaryDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public int Year { get; set; }
        public List<LeaveTypeBalanceDto> LeaveBalances { get; set; } = new List<LeaveTypeBalanceDto>();
        public decimal TotalAllocated { get; set; }
        public decimal TotalConsumed { get; set; }
        public decimal TotalAvailable { get; set; }
        public decimal OverallUtilizationPercentage { get; set; }
    }

}
