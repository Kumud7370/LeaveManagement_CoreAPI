//using AttendanceManagementSystem.Models.Enums;
//using System.Net;

//namespace AttendanceManagementSystem.Models.DTOs.Employee
//{
//    public class UpdateEmployeeDto
//    {
//        public string? FirstName { get; set; }
//        public string? MiddleName { get; set; }
//        public string? LastName { get; set; }
//        public string? Email { get; set; }
//        public string? PhoneNumber { get; set; }
//        public string? AlternatePhoneNumber { get; set; }
//        public DateTime? DateOfBirth { get; set; }
//        public Gender? Gender { get; set; }
//        public AttendanceManagementSystem.Models.ValueObjects.Address? Address { get; set; }
//        public string? DepartmentId { get; set; }
//        public string? DesignationId { get; set; }
//        public string? ManagerId { get; set; }
//        public DateTime? DateOfJoining { get; set; }
//        public DateTime? DateOfLeaving { get; set; }
//        public EmploymentType? EmploymentType { get; set; }
//        public EmployeeStatus? EmployeeStatus { get; set; }
//        public string? ProfileImageUrl { get; set; }
//        public string? BiometricId { get; set; }
//    }
//}


using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Models.ValueObjects;

namespace AttendanceManagementSystem.Models.DTOs.Employee
{
    public class UpdateEmployeeDto
    {
        // ── Marathi names ─────────────────────────────────────────────────
        public string? FirstNameMr { get; set; }
        public string? MiddleNameMr { get; set; }
        public string? LastNameMr { get; set; }

        // ── English names ─────────────────────────────────────────────────
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }

        // ── Hindi names ───────────────────────────────────────────────────
        public string? FirstNameHi { get; set; }
        public string? MiddleNameHi { get; set; }
        public string? LastNameHi { get; set; }

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AlternatePhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public Address? Address { get; set; }
        public string? DepartmentId { get; set; }
        public string? DesignationId { get; set; }
        public string? ManagerId { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public DateTime? DateOfLeaving { get; set; }
        public EmploymentType? EmploymentType { get; set; }
        public EmployeeStatus? EmployeeStatus { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? BiometricId { get; set; }
    }
}
