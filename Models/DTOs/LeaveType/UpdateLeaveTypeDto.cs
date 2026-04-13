namespace AttendanceManagementSystem.Models.DTOs.LeaveType
{
    public class UpdateLeaveTypeDto
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? NameMr { get; set; }
        public string? NameHi { get; set; }
        public string? Description { get; set; }
        public int? MaxDaysPerYear { get; set; }
        public bool? IsCarryForward { get; set; }
        public int? MaxCarryForwardDays { get; set; }
        public bool? RequiresApproval { get; set; }
        public bool? RequiresDocument { get; set; }
        public bool? IsPaidLeave { get; set; }
        public int? MinimumNoticeDays { get; set; }
        public string? Color { get; set; }
        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
