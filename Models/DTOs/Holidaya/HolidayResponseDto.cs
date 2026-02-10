using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.Holiday
{
    public class HolidayResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string HolidayName { get; set; } = string.Empty;
        public DateTime HolidayDate { get; set; }
        public string? Description { get; set; }
        public HolidayType HolidayType { get; set; }
        public string HolidayTypeName { get; set; } = string.Empty;
        public bool IsOptional { get; set; }
        public List<string> ApplicableDepartments { get; set; } = new List<string>();
        public List<string> DepartmentNames { get; set; } = new List<string>();
        public bool IsUpcoming { get; set; }
        public bool IsToday { get; set; }
        public int DaysUntilHoliday { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}