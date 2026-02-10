namespace AttendanceManagementSystem.Models.DTOs.Shift
{
    public class UpdateShiftDto
    {
        public string? ShiftName { get; set; }
        public string? ShiftCode { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public int? GracePeriodMinutes { get; set; }
        public int? MinimumWorkingMinutes { get; set; }
        public int? BreakDurationMinutes { get; set; }
        public bool? IsNightShift { get; set; }
        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public decimal? NightShiftAllowancePercentage { get; set; }
    }
}