// Models/Entities/Holiday.cs
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class Holiday : BaseEntity
    {
        [BsonElement("date")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc, DateOnly = true)]
        public DateTime Date { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("nameMr")]
        public string? NameMr { get; set; }

        [BsonElement("year")]
        public int Year { get; set; }
    }
}