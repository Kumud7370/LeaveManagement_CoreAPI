using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Translation : BaseEntity
    {
        [BsonElement("key")]
        public string Key { get; set; } = string.Empty;

        [BsonElement("namespace")]
        public string Namespace { get; set; } = string.Empty;

        [BsonElement("mr")]
        public string Mr { get; set; } = string.Empty;   

        [BsonElement("en")]
        public string? En { get; set; }                   

        [BsonElement("hi")]
        public string? Hi { get; set; }                   

        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }
    }
}