namespace AttendanceManagementSystem.Models.DTOs.WorkFromHome
{
    public class UpdateWfhRequestDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Reason { get; set; }
    }
}
