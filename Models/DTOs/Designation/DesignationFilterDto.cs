using AttendanceManagementSystem.Models.DTOs.Common;

namespace AttendanceManagementSystem.Models.DTOs.Designation
{
    public class DesignationFilterDto : PaginationDto  
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int? Level { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public string SortBy { get; set; } = "designationCode";
        public bool SortDescending { get; set; } = false;
    }
}