namespace AttendanceManagementSystem.Models.DTOs.Holiday
{
    public class HolidayDto
    {
        public string Id { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameMr { get; set; }
        public int Year { get; set; }
    }
}
