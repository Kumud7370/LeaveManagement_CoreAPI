using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.WorkFromHome
{
    public class ApproveRejectWfhRequestDto
    {
        public ApprovalStatus Status { get; set; }
        public string? RejectionReason { get; set; }
    }
}
