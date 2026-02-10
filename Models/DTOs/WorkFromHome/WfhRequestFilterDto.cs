using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.WorkFromHome
{
    public class WfhRequestFilterDto
    {
        public string? EmployeeId { get; set; }
        public string? SearchTerm { get; set; }
        public ApprovalStatus? Status { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public string? ApprovedBy { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "StartDate";
        public bool SortDescending { get; set; } = true;
    }
}
