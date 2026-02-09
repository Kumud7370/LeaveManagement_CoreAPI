using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class LeaveType : BaseEntity
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("code")]
        public string Code { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("maxDaysPerYear")]
        public int MaxDaysPerYear { get; set; }

        [BsonElement("isCarryForward")]
        public bool IsCarryForward { get; set; } = false;

        [BsonElement("maxCarryForwardDays")]
        public int MaxCarryForwardDays { get; set; } = 0;

        [BsonElement("requiresApproval")]
        public bool RequiresApproval { get; set; } = true;

        [BsonElement("requiresDocument")]
        public bool RequiresDocument { get; set; } = false;

        [BsonElement("minimumNoticeDays")]
        public int MinimumNoticeDays { get; set; } = 0;

        [BsonElement("color")]
        public string Color { get; set; } = "#000000";

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("displayOrder")]
        public int DisplayOrder { get; set; } = 0;

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }

        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DeletedAt { get; set; }

        public int GetAvailableCarryForward()
        {
            return IsCarryForward ? MaxCarryForwardDays : 0;
        }

        public bool IsDocumentRequired()
        {
            return RequiresDocument;
        }
    }
}