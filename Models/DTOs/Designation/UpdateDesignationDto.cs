namespace AttendanceManagementSystem.Models.DTOs.Designation
{
    public class UpdateDesignationDto
    {
        public string? DesignationCode { get; set; }
        public string? DesignationName { get; set; }
        public string? Description { get; set; }
        public int? Level { get; set; }
        public bool? IsActive { get; set; }
    }
}