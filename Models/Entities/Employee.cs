using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Models.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Employee : BaseEntity
    {
        [BsonElement("employeeCode")]
        public string EmployeeCode { get; set; } = string.Empty;

        // ── Marathi names (PRIMARY — required) ────────────────────────────
        [BsonElement("firstNameMr")]
        public string FirstNameMr { get; set; } = string.Empty;

        [BsonElement("middleNameMr")]
        public string? MiddleNameMr { get; set; }

        [BsonElement("lastNameMr")]
        public string LastNameMr { get; set; } = string.Empty;

        // ── English names (secondary — optional) ──────────────────────────
        [BsonElement("firstName")]
        public string? FirstName { get; set; }

        [BsonElement("middleName")]
        public string? MiddleName { get; set; }

        [BsonElement("lastName")]
        public string? LastName { get; set; }

        // ── Hindi names (optional) ────────────────────────────────────────
        [BsonElement("firstNameHi")]
        public string? FirstNameHi { get; set; }

        [BsonElement("middleNameHi")]
        public string? MiddleNameHi { get; set; }

        [BsonElement("lastNameHi")]
        public string? LastNameHi { get; set; }

        [BsonElement("userId")]
        public string? UserId { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [BsonElement("alternatePhoneNumber")]
        public string? AlternatePhoneNumber { get; set; }

        [BsonElement("dateOfBirth")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime DateOfBirth { get; set; }

        [BsonElement("gender")]
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

        [BsonElement("employmentType")]
        [BsonRepresentation(BsonType.String)]
        public EmploymentType EmploymentType { get; set; }

        [BsonElement("employeeStatus")]
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


        public string GetFullName() => GetFullName("mr");

        public string GetFullName(string lang)
        {
            return lang switch
            {
                "mr" => BuildName(
                            FirstNameMr,
                            MiddleNameMr,
                            LastNameMr),

                "hi" when !string.IsNullOrWhiteSpace(FirstNameHi)
                    => BuildName(
                            FirstNameHi,
                            MiddleNameHi,
                            LastNameHi ?? LastNameMr),

                "en" when !string.IsNullOrWhiteSpace(FirstName)
                    => BuildName(
                            FirstName,
                            MiddleName,
                            LastName ?? LastNameMr),

                _ => BuildName(FirstNameMr, MiddleNameMr, LastNameMr)
            };
        }

        private static string BuildName(string? first, string? middle, string? last)
        {
            var f = first ?? string.Empty;
            var l = last ?? string.Empty;
            return string.IsNullOrWhiteSpace(middle)
                ? $"{f} {l}".Trim()
                : $"{f} {middle} {l}".Trim();
        }

        public int GetAge()
        {
            var today = DateTime.UtcNow;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        public bool IsCurrentlyEmployed()
            => !DateOfLeaving.HasValue && EmployeeStatus == EmployeeStatus.Active;
    }
}