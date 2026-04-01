namespace AttendanceManagementSystem.Models.DTOs.Designation
{
    public class CreateDesignationDto
    {
        public string? DepartmentId { get; set; }
        public string DesignationCode { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
        public string? DesignationNameMr { get; set; }
        public string? DesignationNameEn { get; set; }
        public string? DesignationNameHi { get; set; }
        public string? Description { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; } = true;
    }
}