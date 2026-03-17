using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class EmployeeAssignmentHistory : BaseEntity  // ← extend BaseEntity
    {
        [BsonElement("employeeId")]
        public string EmployeeId { get; set; } = string.Empty;

        [BsonElement("fromDepartmentId")]
        public string? FromDepartmentId { get; set; }

        [BsonElement("toDepartmentId")]
        public string ToDepartmentId { get; set; } = string.Empty;

        [BsonElement("fromDesignationId")]
        public string? FromDesignationId { get; set; }

        [BsonElement("toDesignationId")]
        public string ToDesignationId { get; set; } = string.Empty;

        [BsonElement("changedBy")]
        public string ChangedBy { get; set; } = string.Empty;

        [BsonElement("changedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ChangedAt { get; set; }

        [BsonElement("reason")]
        public string? Reason { get; set; }
    }
}