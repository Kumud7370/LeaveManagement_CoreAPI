using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.EmployeeShift
{
    public class EmployeeShiftFilterDto
    {
        public string? EmployeeId { get; set; }
        public string? ShiftId { get; set; }
        public ShiftChangeStatus? Status { get; set; }
        public DateTime? EffectiveFromStart { get; set; }
        public DateTime? EffectiveFromEnd { get; set; }
        public bool? IsActive { get; set; }
        public bool? OnlyCurrentAssignments { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "EffectiveFrom";
        public bool SortDescending { get; set; } = true;
    }
}