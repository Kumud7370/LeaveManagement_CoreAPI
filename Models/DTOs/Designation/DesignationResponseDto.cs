using System.Text.Json.Serialization;

namespace AttendanceManagementSystem.Models.DTOs.Designation
{
    public class DesignationResponseDto
    {
        [JsonPropertyName("designationId")]
        public string Id { get; set; } = string.Empty;

        public string DesignationCode { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
        public string? DesignationNameMr { get; set; }
        public string? DesignationNameEn { get; set; }
        public string? DesignationNameHi { get; set; }
        public string? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentNameMr { get; set; }
        public string? DepartmentNameEn { get; set; }
        public string? DepartmentNameHi { get; set; }
        public string? Description { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; }
        public int EmployeeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}