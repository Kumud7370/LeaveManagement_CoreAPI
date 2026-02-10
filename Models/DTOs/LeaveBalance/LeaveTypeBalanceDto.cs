namespace AttendanceManagementSystem.Models.DTOs.LeaveBalance
{
    public class LeaveTypeBalanceDto
    {
        public string LeaveTypeId { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeColor { get; set; } = string.Empty;
        public decimal TotalAllocated { get; set; }
        public decimal Consumed { get; set; }
        public decimal CarriedForward { get; set; }
        public decimal Available { get; set; }
        public decimal UtilizationPercentage { get; set; }
    }
}
