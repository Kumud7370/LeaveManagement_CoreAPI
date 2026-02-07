using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.Employee
{
    public class EmployeeFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? DepartmentId { get; set; }
        public string? DesignationId { get; set; }
        public string? ManagerId { get; set; }
        public EmployeeStatus? EmployeeStatus { get; set; }
        public EmploymentType? EmploymentType { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? JoiningDateFrom { get; set; }
        public DateTime? JoiningDateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "EmployeeCode";
        public bool SortDescending { get; set; } = false;
    }
}