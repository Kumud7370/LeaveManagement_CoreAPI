using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.AttendanceRegularization
{
    public class RegularizationFilterDto
    {
        public string? EmployeeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public RegularizationType? RegularizationType { get; set; }
        public RegularizationStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "RequestedAt";
        public bool SortDescending { get; set; } = true;
    }
}