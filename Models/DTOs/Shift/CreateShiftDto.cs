namespace AttendanceManagementSystem.Models.DTOs.Shift
{
    public class CreateShiftDto
    {
        public string ShiftName { get; set; } = string.Empty;
        public string ShiftCode { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int GracePeriodMinutes { get; set; } = 15;
        public int MinimumWorkingMinutes { get; set; } = 480;
        public int BreakDurationMinutes { get; set; } = 60;
        public bool IsNightShift { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#3B82F6";
        public decimal NightShiftAllowancePercentage { get; set; } = 0;
    }
}