using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.Attendance
{
    public class AttendanceFilterDto
    {
        public string? EmployeeId { get; set; }
        public string? DepartmentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AttendanceStatus? Status { get; set; }
        public bool? IsLate { get; set; }
        public bool? IsEarlyLeave { get; set; }
        public CheckInMethod? CheckInMethod { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "AttendanceDate";
        public bool SortDescending { get; set; } = true;
    }
}