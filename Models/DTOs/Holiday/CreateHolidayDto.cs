namespace AttendanceManagementSystem.Models.DTOs.Holiday
{
    public class CreateHolidayDto
    {
        public string Date { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameMr { get; set; }
    }
}
