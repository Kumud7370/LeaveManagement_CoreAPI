//using AttendanceManagementSystem.Models.Enums;
//using AttendanceManagementSystem.Models.ValueObjects;
//using System.Net;

//namespace AttendanceManagementSystem.Models.DTOs.Employee
//{
//    public class EmployeeResponseDto
//    {
//        public string Id { get; set; } = string.Empty;
//        public string EmployeeCode { get; set; } = string.Empty;
//        public string FirstName { get; set; } = string.Empty;
//        public string? MiddleName { get; set; }
//        public string LastName { get; set; } = string.Empty;
//        public string FullName { get; set; } = string.Empty;
//        public string Email { get; set; } = string.Empty;
//        public string PhoneNumber { get; set; } = string.Empty;
//        public string? AlternatePhoneNumber { get; set; }
//        public DateTime DateOfBirth { get; set; }
//        public int Age { get; set; }
//        public Gender Gender { get; set; }
//        public string GenderName { get; set; } = string.Empty;
//        public Address Address { get; set; } = new Address();
//        public string DepartmentId { get; set; } = string.Empty;
//        public string? DepartmentName { get; set; }
//        public string DesignationId { get; set; } = string.Empty;
//        public string? DesignationName { get; set; }
//        public string? ManagerId { get; set; }
//        public string? ManagerName { get; set; }
//        public DateTime DateOfJoining { get; set; }
//        public DateTime? DateOfLeaving { get; set; }
//        public EmploymentType EmploymentType { get; set; }
//        public string EmploymentTypeName { get; set; } = string.Empty;
//        public EmployeeStatus EmployeeStatus { get; set; }
//        public string EmployeeStatusName { get; set; } = string.Empty;
//        public string? ProfileImageUrl { get; set; }
//        public string? BiometricId { get; set; }
//        public bool IsCurrentlyEmployed { get; set; }
//        public DateTime CreatedAt { get; set; }
//        public DateTime? UpdatedAt { get; set; }
//    }
//}

using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Models.ValueObjects;

namespace AttendanceManagementSystem.Models.DTOs.Employee
{
    public class EmployeeResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;

        // ── Marathi names (primary) ───────────────────────────────────────
        public string FirstNameMr { get; set; } = string.Empty;
        public string? MiddleNameMr { get; set; }
        public string LastNameMr { get; set; } = string.Empty;

        // ── English names (secondary) ─────────────────────────────────────
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }

        // ── Hindi names ───────────────────────────────────────────────────
        public string? FirstNameHi { get; set; }
        public string? MiddleNameHi { get; set; }
        public string? LastNameHi { get; set; }

        // ── Computed full names per language ──────────────────────────────
        public string FullName { get; set; } = string.Empty;
        public string? FullNameEn { get; set; }
        public string? FullNameHi { get; set; }

        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AlternatePhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public string GenderName { get; set; } = string.Empty;
        public Address Address { get; set; } = new Address();
        public string DepartmentId { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string DesignationId { get; set; } = string.Empty;
        public string? DesignationName { get; set; }
        public string? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public DateTime DateOfJoining { get; set; }
        public DateTime? DateOfLeaving { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public string EmploymentTypeName { get; set; } = string.Empty;
        public EmployeeStatus EmployeeStatus { get; set; }
        public string EmployeeStatusName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? BiometricId { get; set; }
        public bool IsCurrentlyEmployed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}