//using AttendanceManagementSystem.Models.Enums;
//using AttendanceManagementSystem.Models.ValueObjects;
//using System.Net;

//namespace AttendanceManagementSystem.Models.DTOs.Employee
//{
//    public class CreateEmployeeDto
//    {
//        public string EmployeeCode { get; set; } = string.Empty;
//        public string FirstName { get; set; } = string.Empty;
//        public string? MiddleName { get; set; }
//        public string LastName { get; set; } = string.Empty;
//        public string Email { get; set; } = string.Empty;
//        public string PhoneNumber { get; set; } = string.Empty;
//        public string? AlternatePhoneNumber { get; set; }
//        public DateTime DateOfBirth { get; set; }
//        public Gender Gender { get; set; }
//        public Address Address { get; set; } = new Address();
//        public string DepartmentId { get; set; } = string.Empty;
//        public string DesignationId { get; set; } = string.Empty;
//        public string? ManagerId { get; set; }
//        public DateTime DateOfJoining { get; set; }
//        public DateTime? DateOfLeaving { get; set; }
//        public EmploymentType EmploymentType { get; set; }
//        public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Active;
//        public string? ProfileImageUrl { get; set; }
//        public string? BiometricId { get; set; }
//    }
//}

using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Models.ValueObjects;

namespace AttendanceManagementSystem.Models.DTOs.Employee
{
    public class CreateEmployeeDto
    {
        public string EmployeeCode { get; set; } = string.Empty;

        // ── Marathi names (required) ──────────────────────────────────────
        public string FirstNameMr { get; set; } = string.Empty;
        public string? MiddleNameMr { get; set; }
        public string LastNameMr { get; set; } = string.Empty;

        // ── English names (optional) ──────────────────────────────────────
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }

        // ── Hindi names (optional) ────────────────────────────────────────
        public string? FirstNameHi { get; set; }
        public string? MiddleNameHi { get; set; }
        public string? LastNameHi { get; set; }

        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AlternatePhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public Address Address { get; set; } = new Address();
        public string DepartmentId { get; set; } = string.Empty;
        public string DesignationId { get; set; } = string.Empty;
        public string? ManagerId { get; set; }
        public DateTime DateOfJoining { get; set; }
        public DateTime? DateOfLeaving { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Active;
        public string? ProfileImageUrl { get; set; }
        public string? BiometricId { get; set; }
    }
}
