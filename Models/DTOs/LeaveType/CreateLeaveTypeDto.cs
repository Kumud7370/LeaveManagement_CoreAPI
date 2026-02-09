namespace AttendanceManagementSystem.Models.DTOs.LeaveType
{
    public class CreateLeaveTypeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxDaysPerYear { get; set; }
        public bool IsCarryForward { get; set; } = false;
        public int MaxCarryForwardDays { get; set; } = 0;
        public bool RequiresApproval { get; set; } = true;
        public bool RequiresDocument { get; set; } = false;
        public int MinimumNoticeDays { get; set; } = 0;
        public string Color { get; set; } = "#000000";
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}
