namespace AttendanceManagementSystem.Models.DTOs.Shift
{
    public class ShiftResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public string ShiftCode { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string StartTimeFormatted { get; set; } = string.Empty;
        public string EndTimeFormatted { get; set; } = string.Empty;
        public int GracePeriodMinutes { get; set; }
        public int MinimumWorkingMinutes { get; set; }
        public string MinimumWorkingHoursFormatted { get; set; } = string.Empty;
        public int BreakDurationMinutes { get; set; }
        public bool IsNightShift { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal NightShiftAllowancePercentage { get; set; }
        public string TotalShiftDuration { get; set; } = string.Empty;
        public string NetWorkingHours { get; set; } = string.Empty;
        public string ShiftTimingDisplay { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}