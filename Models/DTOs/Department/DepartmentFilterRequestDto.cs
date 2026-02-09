namespace AttendanceManagementSystem.Models.DTOs.Department
{
    public class DepartmentFilterRequestDto
    {
        public string? SearchTerm { get; set; }
        public bool IncludeDeleted { get; set; }
        public bool? IsActive { get; set; }

        public Guid? ParentDepartmentId { get; set; }
        public bool? RootLevelOnly { get; set; }
        public Guid? HeadOfDepartment { get; set; }

        public string SortBy { get; set; } = "DepartmentName";
        public string SortDirection { get; set; } = "asc";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
