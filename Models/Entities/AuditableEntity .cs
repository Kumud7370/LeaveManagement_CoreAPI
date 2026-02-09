using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public abstract class AuditableEntity : BaseEntity
    {
        /// <summary>
        /// Timestamp when the entity was created
        /// </summary>
        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// User ID who created this entity
        /// </summary>
        [BsonElement("createdBy")]
        [BsonRepresentation(BsonType.String)]
        [BsonIgnoreIfNull]
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Timestamp when the entity was last updated
        /// </summary>
        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonIgnoreIfNull]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// User ID who last updated this entity
        /// </summary>
        [BsonElement("updatedBy")]
        [BsonRepresentation(BsonType.String)]
        [BsonIgnoreIfNull]
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Timestamp when the entity was soft deleted
        /// </summary>
        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonIgnoreIfNull]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// User ID who deleted this entity
        /// </summary>
        [BsonElement("deletedBy")]
        [BsonRepresentation(BsonType.String)]
        [BsonIgnoreIfNull]
        public Guid? DeletedBy { get; set; }

        /// <summary>
        /// Updates the modification tracking fields
        /// </summary>
        public void SetUpdated(Guid updatedBy)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Marks the entity as soft deleted
        /// </summary>
        public void SetDeleted(Guid deletedBy)
        {
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }
    }
}