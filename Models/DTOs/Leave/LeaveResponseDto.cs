using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.Leave
{
    public class LeaveResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string LeaveTypeId { get; set; } = string.Empty;
        public string? LeaveTypeName { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeColor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public LeaveStatus LeaveStatus { get; set; }
        public string LeaveStatusName { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
       public string? AdminApprovedBy { get; set; }
        public DateTime? AdminApprovedDate { get; set; }
        public string? NayabApprovedBy { get; set; }
        public DateTime? NayabApprovedDate { get; set; }
        public string? TehsildarApprovedBy { get; set; }
        public DateTime? TehsildarApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? RejectedBy { get; set; }
        public string? RejectedByName { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancellationReason { get; set; }
        public bool IsEmergencyLeave { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool IsActiveLeave { get; set; }
        public bool CanBeCancelled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}