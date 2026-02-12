using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.AttendanceRegularization
{
    public class RegularizationResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string? AttendanceId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
        public RegularizationType RegularizationType { get; set; }
        public string RegularizationTypeName { get; set; } = string.Empty;
        public DateTime? RequestedCheckIn { get; set; }
        public DateTime? RequestedCheckOut { get; set; }
        public DateTime? OriginalCheckIn { get; set; }
        public DateTime? OriginalCheckOut { get; set; }
        public string Reason { get; set; } = string.Empty;
        public RegularizationStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}