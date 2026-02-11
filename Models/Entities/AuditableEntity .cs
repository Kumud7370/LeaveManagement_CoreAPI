using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public abstract class AuditableEntity : BaseEntity
    {
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

        public void SetUpdated(Guid updatedBy)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void SetDeleted(Guid deletedBy)
        {
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }
    }
}