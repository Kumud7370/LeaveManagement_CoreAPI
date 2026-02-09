using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models.DTOs.Department
{
    public class CreateDepartmentRequestDto
    {/// <summary>
     /// Unique department code (e.g., "IT", "HR", "FIN")
     /// </summary>
        [Required(ErrorMessage = "Department code is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Department code must be between 2 and 20 characters")]
        [RegularExpression("^[A-Z0-9_-]+$", ErrorMessage = "Department code must contain only uppercase letters, numbers, underscores, and hyphens")]
        public string DepartmentCode { get; set; } = string.Empty;

        /// <summary>
        /// Full department name
        /// </summary>
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department name must be between 2 and 100 characters")]
        public string DepartmentName { get; set; } = string.Empty;

        /// <summary>
        /// Department description
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// Employee ID of the department head
        /// </summary>
        public Guid? HeadOfDepartment { get; set; }

        /// <summary>
        /// Parent department ID (for sub-departments)
        /// </summary>
        public Guid? ParentDepartmentId { get; set; }

        /// <summary>
        /// Display order for sorting
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Whether the department is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
