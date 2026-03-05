using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Department : BaseEntity
    {
        [BsonElement("departmentId")]
        [BsonRepresentation(BsonType.String)]
        public Guid DepartmentId { get; set; }

        [BsonElement("departmentCode")]
        [BsonRequired]
        public string DepartmentCode { get; set; } = string.Empty;

        [BsonElement("departmentName")]
        [BsonRequired]
        public string DepartmentName { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("displayOrder")]
        public int DisplayOrder { get; set; } = 0;

        [BsonElement("metadata")]
        [BsonIgnoreIfNull]
        public Dictionary<string, object>? Metadata { get; set; }

        [BsonElement("createdBy")]
        [BsonRepresentation(BsonType.String)]
        [BsonIgnoreIfNull]
        public Guid? CreatedBy { get; set; }

        [BsonElement("updatedBy")]
        [BsonRepresentation(BsonType.String)]
        [BsonIgnoreIfNull]
        public Guid? UpdatedBy { get; set; }

        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonIgnoreIfNull]
        public DateTime? DeletedAt { get; set; }

        [BsonElement("deletedBy")]
        [BsonRepresentation(BsonType.String)]
        [BsonIgnoreIfNull]
        public Guid? DeletedBy { get; set; }

        [BsonIgnore]
        public virtual ICollection<Employee>? Employees { get; set; }

        [BsonIgnore]
        public virtual ICollection<Department>? ChildDepartments { get; set; }

        [BsonIgnore]
        public string FullPath => DepartmentName;

        [BsonIgnore]
        public int Level => 0;

        public Department()
        {
            DepartmentId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Id = ObjectId.GenerateNewId().ToString();
        }

        public bool CanDelete()
        {
            if (Employees?.Count > 0)
                return false;
            if (ChildDepartments?.Count > 0)
                return false;
            return true;
        }
    }
}