using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.AttendanceRegularization
{
    public class RegularizationApprovalDto
    {
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}