namespace AttendanceManagementSystem.Models.DTOs.LeaveType
{
    public class LeaveTypeResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxDaysPerYear { get; set; }
        public bool IsCarryForward { get; set; }
        public int MaxCarryForwardDays { get; set; }
        public int AvailableCarryForward { get; set; }
        public bool RequiresApproval { get; set; }
        public bool RequiresDocument { get; set; }
        public int MinimumNoticeDays { get; set; }
        public string Color { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
