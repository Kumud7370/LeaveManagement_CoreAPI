using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.Leave
{
    public class LeaveFilterDto
    {
        public string? EmployeeId { get; set; }
        public string? LeaveTypeId { get; set; }
        public LeaveStatus? LeaveStatus { get; set; }
        public string? DepartmentId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public DateTime? AppliedDateFrom { get; set; }
        public DateTime? AppliedDateTo { get; set; }
        public bool? IsEmergencyLeave { get; set; }
        public List<string>? EmployeeIds { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "AppliedDate";
        public bool SortDescending { get; set; } = true;
    }
}