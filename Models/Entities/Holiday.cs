using AttendanceManagementSystem.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class Holiday : BaseEntity
    {
        [BsonElement("holidayName")]
        public string HolidayName { get; set; } = string.Empty;

        [BsonElement("holidayDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime HolidayDate { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("holidayType")]
        public HolidayType HolidayType { get; set; }

        [BsonElement("isOptional")]
        public bool IsOptional { get; set; }

        [BsonElement("applicableDepartments")]
        public List<string> ApplicableDepartments { get; set; } = new List<string>();

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }

        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DeletedAt { get; set; }
        public bool IsUpcoming() => HolidayDate.Date >= DateTime.UtcNow.Date;

        public bool IsToday() => HolidayDate.Date == DateTime.UtcNow.Date;

        public int DaysUntilHoliday() => (HolidayDate.Date - DateTime.UtcNow.Date).Days;
    }
}