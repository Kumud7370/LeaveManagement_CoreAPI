using AttendanceManagementSystem.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class Employee
    {
        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("employeeCode")]
        public string EmployeeCode { get; set; } = string.Empty;

        [BsonElement("department")]
        public string Department { get; set; } = string.Empty;

        [BsonElement("designation")]
        public string Designation { get; set; } = string.Empty;

        [BsonElement("gender")]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Gender Gender { get; set; }

        [BsonElement("dateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [BsonElement("joiningDate")]
        public DateTime JoiningDate { get; set; } = DateTime.UtcNow;

        [BsonElement("phoneNumber")]
        public string? PhoneNumber { get; set; }
    }
}

