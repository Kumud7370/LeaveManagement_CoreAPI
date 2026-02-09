namespace AttendanceManagementSystem.Models.DTOs.LeaveType
{
    public class LeaveTypeFilterDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public bool? RequiresApproval { get; set; }
        public bool? RequiresDocument { get; set; }
        public bool? IsCarryForward { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "DisplayOrder";
        public bool SortDescending { get; set; } = false;
    }
}
