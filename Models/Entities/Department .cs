using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class Department : BaseEntity
    {
        [BsonId]
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

        [BsonElement("headOfDepartment")]
        [BsonRepresentation(BsonType.String)]
        public Guid? HeadOfDepartment { get; set; }

        [BsonElement("parentDepartmentId")]
        [BsonRepresentation(BsonType.String)]
        public Guid? ParentDepartmentId { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; } = false;

        [BsonElement("displayOrder")]
        public int DisplayOrder { get; set; } = 0;

        [BsonElement("metadata")]
        [BsonIgnoreIfNull]
        public Dictionary<string, object>? Metadata { get; set; }

        // Audit fields
        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }

        [BsonElement("createdBy")]
        [BsonRepresentation(BsonType.String)]
        [BsonIgnoreIfNull]
        public Guid? CreatedBy { get; set; }

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonIgnoreIfNull]
        public DateTime? UpdatedAt { get; set; }

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

        // Navigation Properties (not stored in MongoDB)
        [BsonIgnore]
        public virtual ICollection<Employee>? Employees { get; set; }

        [BsonIgnore]
        public virtual Department? ParentDepartment { get; set; }

        [BsonIgnore]
        public virtual ICollection<Department>? ChildDepartments { get; set; }

        [BsonIgnore]
        public virtual Employee? DepartmentHead { get; set; }

        // Computed Properties
        [BsonIgnore]
        public string FullPath => GetFullPath();

        [BsonIgnore]
        public int Level => GetDepartmentLevel();

        public Department()
        {
            DepartmentId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        private string GetFullPath()
        {
            var path = new List<string> { DepartmentName };
            var current = ParentDepartment;

            while (current != null)
            {
                path.Insert(0, current.DepartmentName);
                current = current.ParentDepartment;
            }

            return string.Join(" > ", path);
        }

        private int GetDepartmentLevel()
        {
            int level = 0;
            var current = ParentDepartment;

            while (current != null)
            {
                level++;
                current = current.ParentDepartment;
            }

            return level;
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