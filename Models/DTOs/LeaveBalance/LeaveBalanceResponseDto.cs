namespace AttendanceManagementSystem.Models.DTOs.LeaveBalance
{
    public class LeaveBalanceResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string LeaveTypeId { get; set; } = string.Empty;
        public string? LeaveTypeName { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeColor { get; set; }
        public bool IsPaidLeave { get; set; }
        public int Year { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal Consumed { get; set; }
        public decimal CarriedForward { get; set; }
        public decimal Available { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public bool IsLowBalance { get; set; }
        public bool IsCurrentYear { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
