namespace AttendanceManagementSystem.Models.DTOs.Leave
{
    public class UpdateLeaveDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? TotalDays { get; set; }
        public string? Reason { get; set; }
        public bool? IsEmergencyLeave { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}
