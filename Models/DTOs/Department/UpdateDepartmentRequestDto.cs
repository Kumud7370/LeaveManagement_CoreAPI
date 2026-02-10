using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models.DTOs.Department
{
    public class UpdateDepartmentRequestDto
    {
        [Required(ErrorMessage = "Department ID is required")]
        public Guid DepartmentId { get; set; }

        [Required(ErrorMessage = "Department code is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Department code must be between 2 and 20 characters")]
        [RegularExpression("^[A-Z0-9_-]+$", ErrorMessage = "Department code must contain only uppercase letters, numbers, underscores, and hyphens")]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department name must be between 2 and 100 characters")]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public Guid? HeadOfDepartment { get; set; }

        public Guid? ParentDepartmentId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
