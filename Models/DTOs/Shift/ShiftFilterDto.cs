namespace AttendanceManagementSystem.Models.DTOs.Shift
{
    public class ShiftFilterDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsNightShift { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "DisplayOrder";
        public bool SortDescending { get; set; } = false;
    }
}