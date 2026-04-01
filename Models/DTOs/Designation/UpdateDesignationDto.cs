namespace AttendanceManagementSystem.Models.DTOs.Designation
{
    public class UpdateDesignationDto
    {
        public string? DesignationCode { get; set; }
        public string? DesignationName { get; set; }
        public string? DesignationNameMr { get; set; }
        public string? DesignationNameEn { get; set; }
        public string? DesignationNameHi { get; set; }
        public string? DepartmentId { get; set; }
        public string? Description { get; set; }
        public int? Level { get; set; }
        public bool? IsActive { get; set; }
    }
}