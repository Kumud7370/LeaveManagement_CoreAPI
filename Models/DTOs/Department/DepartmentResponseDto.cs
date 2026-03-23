//namespace AttendanceManagementSystem.Models.DTOs.Department
//{
//    public class DepartmentResponseDto
//    {
//        public Guid DepartmentId { get; set; }
//        public string DepartmentCode { get; set; } = string.Empty;
//        public string DepartmentName { get; set; } = string.Empty;
//        public string? Description { get; set; }
//        public bool IsActive { get; set; }
//        public int DisplayOrder { get; set; }
//        public int EmployeeCount { get; set; }
//        public int ChildDepartmentCount { get; set; }
//        public DateTime CreatedAt { get; set; }
//        public DateTime? UpdatedAt { get; set; }
//    }

//    public class DepartmentDetailResponseDto : DepartmentResponseDto
//    {
//        public string FullPath { get; set; } = string.Empty;
//        public int Level { get; set; }
//        public List<DepartmentResponseDto>? ChildDepartments { get; set; }
//        public List<EmployeeSummaryDto>? Employees { get; set; }
//        public AuditInfoDto? AuditInfo { get; set; }
//    }

//    public class DepartmentHierarchyDto
//    {
//        public Guid DepartmentId { get; set; }
//        public string DepartmentCode { get; set; } = string.Empty;
//        public string DepartmentName { get; set; } = string.Empty;
//        public bool IsActive { get; set; }
//        public int EmployeeCount { get; set; }
//        public List<DepartmentHierarchyDto>? Children { get; set; }
//    }

//    public class EmployeeSummaryDto
//    {
//        public Guid EmployeeId { get; set; }
//        public string EmployeeCode { get; set; } = string.Empty;
//        public string FullName { get; set; } = string.Empty;
//        public string? Designation { get; set; }
//        public string? Email { get; set; }
//        public string? ProfileImageUrl { get; set; }
//    }

//    public class AuditInfoDto
//    {
//        public DateTime CreatedAt { get; set; }
//        public Guid? CreatedBy { get; set; }
//        public string? CreatedByName { get; set; }
//        public DateTime? UpdatedAt { get; set; }
//        public Guid? UpdatedBy { get; set; }
//        public string? UpdatedByName { get; set; }
//    }
//}

namespace AttendanceManagementSystem.Models.DTOs.Department
{
    public class DepartmentResponseDto
    {
        public Guid DepartmentId { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;

        // ── Marathi name (primary) ────────────────────────────────────────
        public string DepartmentNameMr { get; set; } = string.Empty;

        // ── English name (secondary) ──────────────────────────────────────
        public string? DepartmentName { get; set; }

        // ── Hindi name ────────────────────────────────────────────────────
        public string? DepartmentNameHi { get; set; }

        // ── Descriptions ──────────────────────────────────────────────────
        public string? DescriptionMr { get; set; }
        public string? Description { get; set; }
        public string? DescriptionHi { get; set; }

        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public int EmployeeCount { get; set; }
        public int ChildDepartmentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DepartmentDetailResponseDto : DepartmentResponseDto
    {
        public string FullPath { get; set; } = string.Empty;
        public int Level { get; set; }
        public List<DepartmentResponseDto>? ChildDepartments { get; set; }
        public List<EmployeeSummaryDto>? Employees { get; set; }
        public AuditInfoDto? AuditInfo { get; set; }
    }

    public class DepartmentHierarchyDto
    {
        public Guid DepartmentId { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentNameMr { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? DepartmentNameHi { get; set; }
        public bool IsActive { get; set; }
        public int EmployeeCount { get; set; }
        public List<DepartmentHierarchyDto>? Children { get; set; }
    }

    public class EmployeeSummaryDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        /// Full name in Marathi (primary)
        public string FullName { get; set; } = string.Empty;
        public string? FullNameEn { get; set; }
        public string? Designation { get; set; }
        public string? Email { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    public class AuditInfoDto
    {
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }
    }
}