using AttendanceManagementSystem.Models.Enums;
using System.Net;

namespace AttendanceManagementSystem.Models.DTOs.Employee
{
    public class UpdateEmployeeDto
    {
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AlternatePhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public AttendanceManagementSystem.Models.ValueObjects.Address? Address { get; set; }
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
