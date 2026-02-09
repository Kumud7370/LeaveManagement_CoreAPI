using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Models.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Net;
using System.Text.Json.Serialization;

namespace AttendanceManagementSystem.Models.Entities
{
    public class Employee : BaseEntity
    {
        [BsonElement("employeeCode")]
        public string EmployeeCode { get; set; } = string.Empty;

        [BsonElement("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("middleName")]
        public string? MiddleName { get; set; }

        [BsonElement("lastName")]
        public string LastName { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [BsonElement("alternatePhoneNumber")]
        public string? AlternatePhoneNumber { get; set; }

        [BsonElement("dateOfBirth")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime DateOfBirth { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Gender Gender { get; set; }

        [BsonElement("address")]
        public Address Address { get; set; } = new Address();

        [BsonElement("departmentId")]
        public string DepartmentId { get; set; } = string.Empty;

        [BsonElement("designationId")]
        public string DesignationId { get; set; } = string.Empty;

        [BsonElement("managerId")]
        public string? ManagerId { get; set; }

        [BsonElement("dateOfJoining")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime DateOfJoining { get; set; }

        [BsonElement("dateOfLeaving")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DateOfLeaving { get; set; }

        [BsonRepresentation(BsonType.String)]
        public EmploymentType EmploymentType { get; set; }

        [BsonRepresentation(BsonType.String)]
        public EmployeeStatus EmployeeStatus { get; set; }

        [BsonElement("profileImageUrl")]
        public string? ProfileImageUrl { get; set; }

        [BsonElement("biometricId")]
        public string? BiometricId { get; set; }

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }

        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DeletedAt { get; set; }

        public string GetFullName()
        {
            return string.IsNullOrEmpty(MiddleName)
                ? $"{FirstName} {LastName}"
                : $"{FirstName} {MiddleName} {LastName}";
        }

        public int GetAge()
        {
            var today = DateTime.UtcNow;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        public bool IsCurrentlyEmployed()
        {
            return !DateOfLeaving.HasValue && EmployeeStatus == EmployeeStatus.Active;
        }
    }
}