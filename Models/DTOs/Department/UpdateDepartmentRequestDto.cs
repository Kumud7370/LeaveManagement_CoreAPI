//using System.ComponentModel.DataAnnotations;
//namespace AttendanceManagementSystem.Models.DTOs.Department
//{
//    public class UpdateDepartmentRequestDto
//    {
//        [Required(ErrorMessage = "Department ID is required")]
//        public Guid DepartmentId { get; set; }

//        [Required(ErrorMessage = "Department code is required")]
//        [StringLength(20, MinimumLength = 2, ErrorMessage = "Department code must be between 2 and 20 characters")]
//        [RegularExpression("^[A-Z0-9_-]+$", ErrorMessage = "Department code must contain only uppercase letters, numbers, underscores, and hyphens")]
//        public string DepartmentCode { get; set; } = string.Empty;

//        [Required(ErrorMessage = "Department name is required")]
//        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department name must be between 2 and 100 characters")]
//        public string DepartmentName { get; set; } = string.Empty;

//        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
//        public string? Description { get; set; }

//        [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
//        public int DisplayOrder { get; set; } = 0;

//        public bool IsActive { get; set; } = true;
//    }
//}

using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models.DTOs.Department
{
    public class UpdateDepartmentRequestDto
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        [RegularExpression("^[A-Z0-9_-]+$")]
        public string DepartmentCode { get; set; } = string.Empty;

        // ── Marathi name (required) ───────────────────────────────────────
        [Required(ErrorMessage = "Department name in Marathi is required")]
        [StringLength(100, MinimumLength = 2)]
        public string DepartmentNameMr { get; set; } = string.Empty;

        // ── English name (optional) ───────────────────────────────────────
        [StringLength(100)]
        public string? DepartmentName { get; set; }

        // ── Hindi name (optional) ─────────────────────────────────────────
        [StringLength(100)]
        public string? DepartmentNameHi { get; set; }

        // ── Descriptions ──────────────────────────────────────────────────
        [StringLength(500)]
        public string? DescriptionMr { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? DescriptionHi { get; set; }

        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}